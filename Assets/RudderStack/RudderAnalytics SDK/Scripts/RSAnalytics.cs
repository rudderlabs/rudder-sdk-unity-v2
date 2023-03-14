namespace RudderStack.Unity
{
    public class RSAnalytics
    {
        private static RSClient _client;
        public static string VERSION => RudderAnalytics.VERSION;

        public static RSClient Client
        {
            get => _client;
            private set 
            {
                if(_client != null) return;
                _client = value;
            }
        }

        public static void Initialize(string writeKey)
        {
            RudderAnalytics.Initialize(writeKey);
            Client = new RSClient(RudderAnalytics.Client);
        }

        public static void Initialize(string writeKey, RSConfig config)
        {
            RudderAnalytics.Initialize(writeKey, config);
            Client = new RSClient(RudderAnalytics.Client);
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
            Client = new RSClient(RudderAnalytics.Client);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
        }
    }
}