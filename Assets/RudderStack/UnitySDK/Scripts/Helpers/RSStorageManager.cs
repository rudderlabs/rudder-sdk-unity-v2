using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RudderStack.Model;
using UnityEngine;
using Application = UnityEngine.Device.Application;
using Page = RudderStack.Model.Page;
using Screen = RudderStack.Model.Screen;

namespace RudderStack.Unity.Utility
{
    public class RSStorageManager
    {
        private bool _running;
        
        private readonly string _encryptionKey;
        private readonly int    _timeBetweenAttempts;
        private readonly string _directoryPath;
        private readonly string _filePath;

        public RSStorageManager(string encryptionKey)
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new ArgumentException(nameof(encryptionKey));

            _encryptionKey = encryptionKey;
            
            _directoryPath = $"{Application.persistentDataPath}/RudderStack/";
            _filePath      = _directoryPath + "rs_persistence";
        }

        public void ClearFile()
        {
            File.Delete(_filePath);
        }

        public List<BaseAction> LoadFromFile()
        {
            var res = new List<BaseAction>();
            
            if (!File.Exists(_filePath)) return res;
            
            var entries = RSEncryptor
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

                res.Add(action);
            }

            return res;
        }

        public void SaveToFile(List<BaseAction> actions)
        {
            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);

            Logger.Debug($"{actions.Count} actions saved to file");
            File.WriteAllText(_filePath,
                RSEncryptor.Encrypt(_encryptionKey,
                    string.Join(Environment.NewLine, actions.Select(JsonConvert.SerializeObject))));
        }
    }
}