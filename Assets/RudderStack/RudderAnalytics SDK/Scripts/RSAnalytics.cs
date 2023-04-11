using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using RudderStack.Flush;
using RudderStack.Request;
using UnityEngine;
using UnityEngine.Networking;

namespace RudderStack.Unity
{
    public class RSAnalytics
    {
        private static RSClient _client;
        public static  string   VERSION => RudderAnalytics.VERSION;

        public static RSClient Client
        {
            get => _client;
            private set => _client ??= value;
        }

        public static void Initialize(string writeKey, RSConfig config)
        {
            if (_client != null)
                return;

            IUnityRequestHandler requestHandler;

            if (config.Send)
            {
                if (config.MaxRetryTime.HasValue)
                {
                    requestHandler = new UnityRequestHandler(config.Timeout,
                        new Backo(max: (Convert.ToInt32(config.MaxRetryTime.Value.TotalSeconds) * 1000), jitter: 5000));
                }
                else
                {
                    requestHandler = new UnityRequestHandler(config.Timeout);
                }
            }
            else
            {
                requestHandler = new UnityFakeRequestHandler();
            }

            IBatchFactory batchFactory = new SimpleBatchFactory(writeKey);

            IAsyncFlushHandler flushHandler;

            var storageEncryptionKey = Resources.Load<RsEncryptionKey>("rudderStackEncryptionKey").key;
            if (config.Async)
            {
                flushHandler = new UnityFlushHandler(batchFactory, requestHandler, config.MaxQueueSize,
                    config.FlushAt, config.FlushIntervalInMillis, storageEncryptionKey);
            }
            else
                flushHandler = new BlockingFlushHandler(batchFactory, requestHandler);

            RudderAnalytics.Initialize(writeKey, config, flushHandler);
            Client = new RSClient(RudderAnalytics.Client);
            requestHandler.Init(RudderAnalytics.Client, new HttpClient());
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
            Client = new RSClient(RudderAnalytics.Client);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
            Client.Dispose();
        }

        public static IEnumerator FetchConfig(string writeKey, Action<string> callback)
        {
            var uri = $"https://api.rudderlabs.com/sourceConfig?p=unity&v={VERSION}&w={writeKey}";

            using var webRequest = UnityWebRequest.Get(uri);

            webRequest.SetRequestHeader("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{writeKey}:"))}");
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"ERROR: {webRequest.error}\n Message: {webRequest.downloadHandler.text}");
                    break;
                case UnityWebRequest.Result.Success:
                    callback.Invoke(webRequest.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.InProgress:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}