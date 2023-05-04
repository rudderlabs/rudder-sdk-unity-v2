using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static IEnumerator Initialize(string writeKey, RSConfig config)
        {
            if (_client != null)
                yield break;
            
            if(string.IsNullOrEmpty(writeKey) || config is null || string.IsNullOrEmpty(config.Inner.DataPlaneUrl))
                throw new InvalidOperationException("Please supply a valid writeKey and config to initialize.");

            yield return FetchConfig(config, writeKey, sourceConfig =>
            {
                if (sourceConfig.source.enabled == false)
                    throw new InvalidOperationException("The RudderStack source is disabled!");
                
                IRSRequestHandler requestHandler;
                if (config.Inner.Send)
                {
                    if (config.Inner.MaxRetryTime.HasValue)
                        requestHandler = new RSRequestHandler(config.Inner.Timeout,
                            new RSBacko(Convert.ToInt32(config.Inner.MaxRetryTime.Value.TotalSeconds) * 1000));
                    else
                        requestHandler = new RSRequestHandler(config.Inner.Timeout);
                }
                else
                {
                    requestHandler = new RSFakeRequestHandler();
                }

                IBatchFactory batchFactory = new SimpleBatchFactory(writeKey);

                IAsyncFlushHandler flushHandler = config.Inner.Async
                    ? new RSFlushHandler(batchFactory, requestHandler, config)
                    : new BlockingFlushHandler(batchFactory, requestHandler);

                RudderAnalytics.Initialize(writeKey, config.Inner, flushHandler);
                Client = new RSClient(RudderAnalytics.Client, config);
                requestHandler.Init(RudderAnalytics.Client, new HttpClient());
            });
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
            Client = new RSClient(RudderAnalytics.Client, client.Config);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
            Client.Dispose();
        }

        private static IEnumerator FetchConfig(RSConfig config, string writeKey, Action<RSSourceConfig> callback)
        {
            var uri = $"{config.GetControlPlaneUrl()}/sourceConfig?p=unity&v={VERSION}&w={writeKey}";

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
                    callback.Invoke(JsonConvert.DeserializeObject<RSSourceConfig>(webRequest.downloadHandler.text));
                    break;
                case UnityWebRequest.Result.InProgress:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}