namespace RudderStack.Unity
{
    public class RsAnalytics
    {
        public static string VERSION
        {
            get => RudderAnalytics.VERSION;
        }

        public static RsClient Client
        {
            get => new RsClient(RudderAnalytics.Client);
        }

        public static void Initialize(string writeKey)
        {
            RudderAnalytics.Initialize(writeKey);
        }

        public static void Initialize(string writeKey, RsConfig config)
        {
            RudderAnalytics.Initialize(writeKey, config);
        }

        public static void Initialize(RsClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
        }
    }
}