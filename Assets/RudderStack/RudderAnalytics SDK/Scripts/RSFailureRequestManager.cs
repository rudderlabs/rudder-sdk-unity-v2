using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using RudderStack.Model;
using RudderStack.Unity.Utility;
using UnityEngine.Device;
using Screen = RudderStack.Model.Screen;

namespace RudderStack.Unity
{
    public class RSFailureRequestManager
    {
        private static readonly string DirectoryPath = $"{Application.persistentDataPath}/RudderStack/";
        private static string FilePath => DirectoryPath + "FailureRequests";
        private Thread _thread;
        
        public RSFailureRequestManager(RSClient client)
        {
            client.Failed += OnClientFailed;
        }
        
        private void OnClientFailed(BaseAction action, System.Exception e)
        {
            var json = JsonConvert.SerializeObject(action);
            var createText = json + Environment.NewLine;

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            
            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, createText);
            else
                File.AppendAllText(FilePath, createText);

            if (_thread is null or {IsAlive: false})
            {
                _thread = new Thread(TrySendRequests);
                _thread.Start();
            }
        }

        private void TrySendRequests()
        {
            Thread.Sleep(60000);

            var data = File.ReadLines(FilePath);
            File.WriteAllText(FilePath, string.Empty);

            var requests = data.Select(JsonConvert.DeserializeObject<IEnumerable<BaseAction>>).ToList();
            requests.OfType<Track>().ToList().ForEach(x => RSAnalytics.Client.Track(x));
            
            /*
            TODO: Solve different action types problem
            requests.OfType<Identify>().ToList().ForEach(x => RSAnalytics.Client.Identify(x));
            requests.OfType<Screen>().ToList().ForEach(x => RSAnalytics.Client.Screen(x));
            requests.OfType<Alias>().ToList().ForEach(x => RSAnalytics.Client.Alias(x));
            requests.OfType<Page>().ToList().ForEach(x => RSAnalytics.Client.Page(x));
            */
        }
    }
}
