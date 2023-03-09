namespace RudderStack.Unity
{
    public class RSAnalytics
    {
        public static string VERSION
        {
            get => RudderAnalytics.VERSION;
        }

        public static RSClient Client
        {
            get => new RSClient(RudderAnalytics.Client);
        }

        public static void Initialize(string writeKey)
        {
            RudderAnalytics.Initialize(writeKey);
        }

        public static void Initialize(string writeKey, RSConfig config)
        {
            RudderAnalytics.Initialize(writeKey, config);
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
        }
    }
}