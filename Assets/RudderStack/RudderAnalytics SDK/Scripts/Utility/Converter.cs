using System;
using RudderStack.Model;
namespace RudderStack.Unity.Utility
{
    public static class Converter
    {
        public static RudderOptions ToOptions(this Track @this)
        {
            var options = new RudderOptions();
            options.SetContext(@this.Context);
            options.SetTimestamp(DateTime.Parse(@this.Timestamp));
            options.SetAnonymousId(@this.AnonymousId);
            // TODO: Solve problem with Integrations
            return options;
        }
    }
}
