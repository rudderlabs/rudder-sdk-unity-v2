using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RudderStack.Flush;
using RudderStack.Model;
using RudderStack.Request;
using RudderStack.Unity.Utility;
using Unity.VisualScripting;
using UnityEngine;
using Timer = System.Threading.Timer;

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
        private          Timer                   _timer;
        private readonly RSStorageManager        _storageManager;
        private readonly int                     _dbThresholdCount;

        private readonly Semaphore _timerSemaphore;
        
        private          bool   requestFailed;
        private readonly object _queueLock;

        internal RSFlushHandler(IBatchFactory batchFactory, IRequestHandler requestHandler, RSConfig config)
        {
            _queue                 = new List<BaseAction>();
            _batchFactory          = batchFactory;
            _requestHandler        = requestHandler as IRSRequestHandler;
            _maxBatchSize          = config.Inner.FlushAt;
            _dbThresholdCount      = config.GetDbThresholdCount();
            _continue              = new CancellationTokenSource();
            _flushIntervalInMillis = config.Inner.FlushIntervalInMillis;

            _timerSemaphore = new Semaphore(1, 1);

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

            new Thread(BlockSemaphore).Start();
            new Thread(FlushCycle).Start();
        }

        private void BlockSemaphore()
        {
            while (!_continue.Token.IsCancellationRequested)
            {
                _timerSemaphore.WaitOne();
            }
        }


        private void FlushCycle()
        {
            while (!_continue.Token.IsCancellationRequested)
            {
                _timerSemaphore.WaitOne(_flushIntervalInMillis);

                try
                {
                    FlushImpl().GetAwaiter().GetResult();
                }
                catch (System.Exception e)
                {
                    
                    Logger.Error("Flush couldn't be completed\n" + e.Message);
                }
            }
        }

        /// <summary>
        /// Blocks until all the messages are flushed
        /// </summary>
        public void Flush()
        {
            _timerSemaphore.Release();
        }

        public async Task FlushAsync()
        {
            _timerSemaphore.Release();
        }

        private async Task FlushImpl()
        {
            lock (_queueLock) if (_queue.Count == 0) return;

            while (_queue.Count > 0 && !_continue.Token.IsCancellationRequested)
            {
                var batchActions = new List<BaseAction>();

                int batchLength;
                
                lock (_queueLock) batchLength = Math.Min(_queue.Count, _maxBatchSize);

                for (int i = 0, currentSize = 0; i < batchLength && BatchMaxSize > currentSize + ActionMaxSize && !_continue.Token.IsCancellationRequested; i++)
                    lock (_queueLock)
                    {
                        batchActions.Add(_queue[i]);
                        currentSize += _queue[i].Size;
                    }

                if (batchActions.Count == 0)
                    break;


                // we have a batch that we're trying to send
                var batch = _batchFactory.Create(batchActions);
                Logger.Debug("Created flush batch.", new Dict { { "batch size", batchActions.Count } });

                // make the request here
                _requestHandler.BatchCompleted += OnRequestCompleted;
                await _requestHandler.MakeRequest(batch);
                _requestHandler.BatchCompleted -= OnRequestCompleted;

                if (requestFailed) // request failed! stop flashing and wait for the better occasion
                    break;
            }

            requestFailed = false;

            // save to file
            lock (_queueLock) _storageManager.SaveToFile(_queue);
        }

        private void OnRequestCompleted(Batch batch, BatchResult result)
        {
            Logger.Debug($"Request completed! Result: {result}");
            switch (result)
            {
                case BatchResult.WrongKey:
                {
                    requestFailed = true;
                    _storageManager.ClearFile();
                    lock (_queueLock) _queue.Clear();
                    break;
                }
                case BatchResult.Success:
                {
                    lock (_queueLock)
                    {
                        foreach (var action in batch.batch)
                            _queue.Remove(action);
                    
                        _storageManager.SaveToFile(_queue);
                    }

                    break;
                }
                default:
                    requestFailed = true;
                    break;
            }
        }

        public async Task Process(BaseAction action)
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
                await FlushAsync();
            }

        }

        public void Dispose()
        {
            Logger.Debug("Disposing AsyncIntervalFlushHandler");
            _timer?.Dispose();
#if !NET35
            _timerSemaphore?.Dispose();
#endif
            _continue?.Cancel();
        }

    }
}