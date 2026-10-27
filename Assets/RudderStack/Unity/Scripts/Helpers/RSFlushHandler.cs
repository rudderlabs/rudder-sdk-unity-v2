using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RudderStack.Flush;
using RudderStack.Model;
using RudderStack.Request;
using RudderStack.Unity.Utility;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSFlushHandler : IAsyncFlushHandler
    {
        /// <summary>
        /// Our servers only accept payloads smaller than 32KB
        /// </summary>
        private const int ActionMaxSize = 32 * 1024;

        /// <summary>
        /// Our servers only accept request smaller than 512KB we left 12kb as margin error
        /// </summary>
        private const int BatchMaxSize = 500 * 1024;

        private readonly List<BaseAction>        _queue;
        private readonly int                     _maxBatchSize;
        private readonly IBatchFactory           _batchFactory;
        private readonly IRSRequestHandler       _requestHandler;
        private readonly CancellationTokenSource _continue;
        private readonly int                     _flushIntervalInMillis;
        private readonly RSStorageManager        _storageManager;
        private readonly int                     _dbThresholdCount;

        private readonly AutoResetEvent _flushSignal = new AutoResetEvent(false);
        private Thread _worker;
        private bool _disposed;
        
        private          bool   requestFailed;
        private readonly object _queueLock;

        internal RSFlushHandler(IBatchFactory batchFactory, IRequestHandler requestHandler, RSConfig config)
        {
            _queue                 = new List<BaseAction>();
            _batchFactory          = batchFactory;
            _requestHandler        = requestHandler as IRSRequestHandler;
            _maxBatchSize          = config.Inner.GetFlushAt();
            _dbThresholdCount      = config.GetDbThresholdCount();
            _continue              = new CancellationTokenSource();
            _flushIntervalInMillis = config.Inner.GetFlushInterval();

            _queueLock = new object();

            const string keyLocation = "04j4bf5dkd8";
            string       storageKey;
            if (PlayerPrefs.HasKey(keyLocation))
            {
                storageKey = PlayerPrefs.GetString(keyLocation);
            }
            else
            {
                using var aes = System.Security.Cryptography.Aes.Create();
                storageKey = Convert.ToBase64String(aes.Key);
                PlayerPrefs.SetString(keyLocation, storageKey);
            }
            
            _storageManager = new RSStorageManager(storageKey);

            _queue.AddRange(_storageManager.LoadFromFile());
        }

        internal void Start()
        {
            lock (_queueLock)
            {
                if (_disposed || _worker != null) return;
                _worker = new Thread(FlushCycle) { IsBackground = true };
                _worker.Start();
            }
        }


        private void FlushCycle()
        {
            try
            {
                while (!_continue.IsCancellationRequested)
                {
                    _flushSignal.WaitOne(_flushIntervalInMillis);
                    if (_continue.IsCancellationRequested) break;
                    try
                    {
                        FlushImpl().GetAwaiter().GetResult();
                    }
                    catch (System.Exception e)
                    {
                        if (!_continue.IsCancellationRequested)
                            Logger.Error("Flush couldn't be completed\n" + e.Message);
                    }
                }
            }
            finally
            {
                lock (_queueLock)
                {
                    _flushSignal.Dispose();
                    _continue.Dispose();
                }
            }
        }

        /// <summary>
        /// Requests a flush on the background worker.
        /// </summary>
        public void Flush()
        {
            lock (_queueLock)
                if (!_disposed) _flushSignal.Set();
        }

        public Task FlushAsync()
        {
            Flush();
            return Task.CompletedTask;
        }

        private async Task FlushImpl()
        {
            // A manual flush must not bypass backoff or repeatedly rewrite unchanged offline storage.
            if (_requestHandler is RSRecoveringRequestHandler recovering && !recovering.CanMakeRequest)
                return;
            lock (_queueLock) if (_queue.Count == 0) return;

            while (!_continue.Token.IsCancellationRequested)
            {
                List<BaseAction> batchActions;
                
                lock (_queueLock)
                {
                    if (_queue.Count <= 0) break;

                    batchActions = new List<BaseAction>();
                    var batchLength = Math.Min(_queue.Count, _maxBatchSize);

                    for (int i = 0, currentSize = 0;
                         i < batchLength && BatchMaxSize > currentSize + ActionMaxSize &&
                         !_continue.Token.IsCancellationRequested;
                         i++)
                    {
                        batchActions.Add(_queue[i]);
                        currentSize += _queue[i].Size;
                    }

                    if (batchActions.Count == 0) break;
                }


                // we have a batch that we're trying to send
                var batch = _batchFactory.Create(batchActions);

                Logger.Debug("Created flush batch.", new Dict { { "batch size", batchActions.Count } });

                // make the request here
                _requestHandler.BatchCompleted += OnRequestCompleted;
                try
                {
                    await _requestHandler.MakeRequest(batch);
                }
                finally
                {
                    _requestHandler.BatchCompleted -= OnRequestCompleted;
                }

                if (requestFailed) // request failed! stop flashing and wait for the better occasion
                    break;

            }

            requestFailed = false;

            // save to file
            lock (_queueLock) if (!_disposed) _storageManager.SaveToFile(_queue);
        }

        private void OnRequestCompleted(Batch batch, BatchResult result)
        {
            lock (_queueLock)
            {
                if (_disposed) return;
                Logger.Debug($"Request completed! Result: {result}");
                switch (result)
                {
                    case BatchResult.WrongKey:
                        requestFailed = true;
                        _storageManager.ClearFile();
                        _queue.Clear();
                        break;
                    case BatchResult.Success:
                        foreach (var action in batch.batch)
                            _queue.Remove(action);
                        _storageManager.SaveToFile(_queue);
                        break;
                    default:
                        requestFailed = true;
                        break;
                }
            }
        }

        public void Process(BaseAction action)
        {
            action.Size = ActionSizeCalculator.Calculate(action);

            if (action.Size > ActionMaxSize)
            {
                Logger.Error($"Action was dropped cause is bigger than {ActionMaxSize} bytes");
                return;
            }

            bool flushRequired;
            lock (_queueLock)
            {
                if (_disposed) return;
                if (_queue.Count < _dbThresholdCount)
                {
                    _queue.Add(action);
                    _storageManager.SaveToFile(_queue);
                }

                Logger.Debug("Enqueued action in async loop.", new Dict
                {
                    { "message id", action.MessageId },
                    { "queue size", _queue.Count }
                });

                flushRequired = _queue.Count >= _maxBatchSize;
            }

            if (flushRequired)
            {
                Logger.Debug("Queue is full. Performing a flush");
                Flush();
            }

        }

        public void Dispose()
        {
            lock (_queueLock)
            {
                if (_disposed) return;
                _disposed = true;
                _continue.Cancel();
                (_requestHandler as IDisposable)?.Dispose();
                _flushSignal.Set();
                if (_worker == null)
                {
                    _flushSignal.Dispose();
                    _continue.Dispose();
                }
            }
        }

    }
}
