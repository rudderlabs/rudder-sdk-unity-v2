using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
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
    public static class RSFailureRequestManager
    {
        private static bool   _running;
        private static string _encryptionKey;
        private static int    _timeBetweenAttempts;

        private static ConcurrentDictionary<string, BaseAction> _failedActions;

        private static string DirectoryPath => $"{Application.persistentDataPath}/RudderStack/";
        private static string FilePath      => DirectoryPath + "FailureRequests";


        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="client"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="timeBetweenAttempts">Time in seconds between attempts to send stored actions</param>
        /// <exception cref="Exception"></exception>
        public static void Init(RSClient client, string encryptionKey, int timeBetweenAttempts = 300)
        {
            client.Failed        += OnClientFailed;
            client.Succeeded     += OnClientSucceeded;
            _failedActions       =  new ConcurrentDictionary<string, BaseAction>();
            _encryptionKey       =  encryptionKey;
            _timeBetweenAttempts =  timeBetweenAttempts;
            _running             =  true;
            
            new Thread(ResendFailedActions).Start();

            if (!File.Exists(FilePath)) return;

            var entries = Encryptor
                          .Decrypt(_encryptionKey, File.ReadAllText(FilePath))
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

                _failedActions.TryAdd(action.MessageId, action);
            }
        }


        public static void Stop()
        {
            if (!_running) return;

            _running = false;

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            Debug.Log($"Saving {_failedActions.Count} failed actions.");
            File.WriteAllText(FilePath, Encryptor.Encrypt(_encryptionKey, string.Join(
                Environment.NewLine,
                _failedActions.Select(x => x.Value).Select(JsonConvert.SerializeObject))));
        }

        private static void OnClientFailed(BaseAction action, System.Exception e)
        {
            if (_failedActions.TryAdd(action.MessageId, action))
            {
                Debug.Log($"The action of type {action.GetType()} is stored.");
            }
        }

        private static void OnClientSucceeded(BaseAction action)
        {
            if (_failedActions.TryRemove(action.MessageId, out _))
            {
                Debug.Log($"Action of type {action.GetType()} is successfully resent.");
            }
        }

        private static void ResendFailedActions()
        {
            while (_running)
            {
                Thread.Sleep(_timeBetweenAttempts * 1000);
                if (!_running) return; // in case the class was stopped during sleep

                foreach (var pair in _failedActions)
                {
                    Debug.Log("Trying to resend!");

                    RSAnalytics.Client.Enqueue(pair.Value);
                }
            }
        }
    }
}