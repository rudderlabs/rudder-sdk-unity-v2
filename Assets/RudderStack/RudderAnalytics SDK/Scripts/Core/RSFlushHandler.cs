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
        private readonly int                     _maxQueueSize;
        private readonly CancellationTokenSource _continue;
        private readonly int                     _flushIntervalInMillis;
        private readonly int                     _threads;
        private readonly Semaphore               _semaphore;
        private          Timer                   _timer;
        private readonly RSStorageManager        _storageManager;
        private readonly int                     _dbThresholdCount;
        
        private bool requestFailed;

        internal RSFlushHandler(IBatchFactory batchFactory, IRequestHandler requestHandler, RSConfig config)
        {
            _queue                 = new List<BaseAction>();
            _batchFactory          = batchFactory;
            _requestHandler        = requestHandler as IRSRequestHandler;
            _maxQueueSize          = config.GetFlushQueueSize();
            _maxBatchSize          = config.Inner.FlushAt;
            _dbThresholdCount      = config.GetDbThresholdCount();
            _continue              = new CancellationTokenSource();
            _flushIntervalInMillis = config.Inner.FlushIntervalInMillis;
            _threads               = 1;
            _semaphore             = new Semaphore(_threads, _threads);

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
            RunInterval();
        }


        private void RunInterval()
        {
            var initialDelay = _queue.Count == 0 ? _flushIntervalInMillis : 0;
            _timer = new Timer(async b => await PerformFlush(), new { }, initialDelay, _flushIntervalInMillis);
        }


        private async Task PerformFlush()
        {
            if (!_semaphore.WaitOne(1))
            {
                Logger.Debug("Skipping flush. Workload limit has been reached");
                return;
            }

            try
            {
                await FlushImpl();
            }
            catch
            {
                Logger.Error("Flush couldn't be completed");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Blocks until all the messages are flushed
        /// </summary>
        public void Flush()
        {
            FlushAsync().GetAwaiter().GetResult();
        }

        public async Task FlushAsync()
        {
            await PerformFlush().ConfigureAwait(false);
            WaitWorkersToBeReleased();
        }

        private void WaitWorkersToBeReleased()
        {
            for (var i = 0; i < _threads; i++) _semaphore.WaitOne();
            _semaphore.Release(_threads);
        }

        private async Task FlushImpl()
        {
            if (_queue.Count == 0) return;
            
            while (_queue.Count > 0 && !_continue.Token.IsCancellationRequested)
            {
                var current = new List<BaseAction>();

                for (int i = 0, currentSize = 0;
                     i < _queue.Count &&
                     i < _maxBatchSize &&
                     !_continue.Token.IsCancellationRequested &&
                     currentSize < BatchMaxSize - ActionMaxSize;
                     i++)
                {
                    current.Add(_queue[i]);
                    currentSize += _queue[i].Size;
                }

                if (current.Count == 0)
                    break;

                // we have a batch that we're trying to send
                var batch = _batchFactory.Create(current);

                Logger.Debug("Created flush batch.", new Dict { { "batch size", current.Count } });

                // make the request here
                _requestHandler.BatchCompleted += OnRequestCompleted;
                await _requestHandler.MakeRequest(batch);
                _requestHandler.BatchCompleted -= OnRequestCompleted;

                if (requestFailed) // request failed! stop flashing and wait for the better occasion
                    break;
            }

            requestFailed = false;

            // save to file
            _storageManager.SaveToFile(_queue, _dbThresholdCount);
        }


        private void OnRequestCompleted(Batch batch, bool succeeded)
        {
            Logger.Debug($"Request completed! Success: {succeeded}");
            if (succeeded)
            {
                foreach (var action in batch.batch) 
                    _queue.Remove(action);
            }
            else
            {
                requestFailed = true;
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

            _semaphore.WaitOne();

            if (_queue.Count < _dbThresholdCount)
            {
                _queue.Add(action);
                _storageManager.SaveToFile(_queue, _dbThresholdCount);
            }

            Logger.Debug("Enqueued action in async loop.", new Dict
            {
                { "message id", action.MessageId },
                { "queue size", _queue.Count }
            });

            var flushRequired = _queue.Count >= _maxQueueSize;

            _semaphore.Release();

            if (flushRequired)
            {
                Logger.Debug("Queue is full. Performing a flush");
                await PerformFlush();
            }

        }

        public void Dispose()
        {
            Logger.Debug("Disposing AsyncIntervalFlushHandler");
            _timer?.Dispose();
#if !NET35
            _semaphore?.Dispose();
#endif
            _continue?.Cancel();
        }

    }
}