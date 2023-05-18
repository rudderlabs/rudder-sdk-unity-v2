using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RudderStack.Flush;
using RudderStack.Model;
using RudderStack.Unity.Utility;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSOfflineFlushHandler : IAsyncFlushHandler
    {
        private readonly int              _dbThresholdCount;
        private readonly RSStorageManager _storageManager;
        private readonly List<BaseAction> _queue;

        public RSOfflineFlushHandler(RSConfig config)
        {
            _queue            = new List<BaseAction>();
            _dbThresholdCount = config.GetDbThresholdCount();
            
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

        public void Process(BaseAction action)
        {
            _queue.Add(action);
            if (_queue.Count > _dbThresholdCount) _queue.RemoveAt(0);
            
            _storageManager.SaveToFile(_queue);

        }

        public void Flush() { }
        public Task FlushAsync() => Task.CompletedTask;
        public void Dispose() { }
    }
}