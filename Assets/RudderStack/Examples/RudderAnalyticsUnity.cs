using RudderStack;

public class RudderAnalyticsUnity : RudderClient
{
    public RudderAnalyticsUnity(string writeKey) : base(writeKey)
    {
    }

    public RudderAnalyticsUnity(string writeKey, RudderConfig config) : base(writeKey, config)
    {
    }
}
