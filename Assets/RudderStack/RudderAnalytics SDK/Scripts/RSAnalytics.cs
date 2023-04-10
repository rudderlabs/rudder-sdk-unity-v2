using System;
using System.Net.Http;
using RudderStack.Request;

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

        public static void Initialize(string writeKey, string storageEncryptionKey)
        {
            Initialize(writeKey, storageEncryptionKey, new RSConfig());
        }

        public static void Initialize(string writeKey, string storageEncryptionKey, RSConfig config)
        {
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
            
            RudderAnalytics.Initialize(writeKey, storageEncryptionKey, config, requestHandler);
            Client         = new RSClient(RudderAnalytics.Client);
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
    }
}