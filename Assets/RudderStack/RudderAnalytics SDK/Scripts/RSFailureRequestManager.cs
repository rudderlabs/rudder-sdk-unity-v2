using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using RudderStack.Model;
using RudderStack.Unity.Utility;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using Page = RudderStack.Model.Page;
using Screen = RudderStack.Model.Screen;

namespace RudderStack.Unity
{
    public class RSFailureRequestManager : IDisposable
    {
        private bool _running;
        
        private readonly string _encryptionKey;
        private readonly int    _timeBetweenAttempts;
        private readonly string _directoryPath;
        private readonly string _filePath;

        private readonly ConcurrentDictionary<string, BaseAction> _requireSendActions;
        private readonly ConcurrentDictionary<string, BaseAction> _queuedActions;
        

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="client"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="timeBetweenAttempts">Time in seconds between attempts to send stored actions</param>
        /// <exception cref="Exception"></exception>
        public RSFailureRequestManager(IRudderAnalyticsClient client, string encryptionKey, int timeBetweenAttempts = 300)
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new ArgumentException(nameof(encryptionKey));

            client.Failed        += OnClientFailed;
            client.Succeeded     += OnClientSucceeded;
            client.Enqueued      += OnClientTriedToSend;
            _requireSendActions  =  new ConcurrentDictionary<string, BaseAction>();
            _queuedActions       =  new ConcurrentDictionary<string, BaseAction>();
            _encryptionKey       =  encryptionKey;
            _timeBetweenAttempts =  timeBetweenAttempts;
            _running             =  true;
            
            _directoryPath = $"{Application.persistentDataPath}/RudderStack/";
            _filePath      = _directoryPath + "FailureRequests";

            new Thread(ResendFailedActions).Start();

            if (!File.Exists(_filePath)) return;

            var entries = Encryptor
                          .Decrypt(_encryptionKey, File.ReadAllText(_filePath))
                          .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                          .Select(Newtonsoft.Json.Linq.JObject.Parse)
                          .ToArray();
            
            Debug.Log($"{entries.Length} actions loaded from file.");
            
            foreach (var entry in entries)
            {
                var type = entry.Value<string>("type");

                BaseAction action = type switch
                {
                    "track"    => entry.ToObject<Track>(),
                    "screen"   => entry.ToObject<Screen>(),
                    "identify" => entry.ToObject<Identify>(),
                    "page"     => entry.ToObject<Page>(),
                    "alias"    => entry.ToObject<Alias>(),
                    _          => throw new System.Exception($"Unknown action of type <{type}> was stored!")
                };

                _queuedActions.TryAdd(action.MessageId, action);
                _requireSendActions.TryAdd(action.MessageId, action);
            }
        }

        private void OnClientTriedToSend(BaseAction action)
        {
            _queuedActions.TryAdd(action.MessageId, action);
            SaveToFile();
        }

        public void Dispose()
        {
            if (!_running) return;

            _running = false;

            Debug.Log($"Saving {_queuedActions.Count} actions.");
            SaveToFile();
        }

        private void OnClientFailed(BaseAction action, System.Exception e)
        {
            if (_requireSendActions.TryAdd(action.MessageId, action))
                Debug.Log($"The action of type {action.GetType()} is stored.");
        }

        private void OnClientSucceeded(BaseAction action)
        {
            if (_queuedActions.TryRemove(action.MessageId, out _)) 
                SaveToFile();

            if (_requireSendActions.TryRemove(action.MessageId, out _))
                Debug.Log($"Action of type {action.GetType()} is successfully resent.");
        }

        private void ResendFailedActions()
        {
            while (_running)
            {
                Thread.Sleep(_timeBetweenAttempts * 1000);
                if (!_running) return; // in case the class was stopped during sleep

                foreach (var pair in _requireSendActions)
                {
                    Debug.Log("Trying to resend!");
                    RSAnalytics.Client.Enqueue(pair.Value);
                }
            }
        }

        private void SaveToFile()
        {
            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);

            File.WriteAllText(_filePath, Encryptor.Encrypt(_encryptionKey, string.Join(
                Environment.NewLine,
                _queuedActions.Select(x => x.Value).Select(JsonConvert.SerializeObject))));
        }
    }
}