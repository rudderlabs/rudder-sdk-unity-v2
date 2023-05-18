using System;
using RudderStack.Model;

namespace RudderStack.Unity
{
    public class RSOptions
    {
        public string        AnonymousId  { get; private set; }
        public Dict          Integrations { get; private set; }
        public DateTime?     Timestamp    { get; private set; }
        public RudderContext Context      { get; private set; }

        public RudderOptions Inner
        {
            get
            {
                var inner = new RudderOptions()
                            .SetAnonymousId(AnonymousId)
                            .SetTimestamp(Timestamp)
                            .SetContext(Context);
                foreach (var integration in Integrations) 
                    inner.Integrations.Add(integration.Key, integration.Value);

                return inner;
            }
        }

        /// <summary>
        /// Options object that allows the specification of a timestamp,
        /// an anonymousId, a context, or target integrations.
        /// </summary>
        public RSOptions()
        {
            this.Integrations = new Dict();
            this.Context      = new RudderContext();
        }

        /// <summary>
        /// Sets the anonymousId of the user. This is typically a cookie session id that identifies
        /// a visitor before they have logged in.
        /// </summary>
        /// <returns>This Options object for chaining.</returns>
        /// <param name="anonymousId">The visitor's anonymousId.</param>
        public RSOptions SetAnonymousId(string anonymousId)
        {
            this.AnonymousId = anonymousId;
            return this;
        }

        /// <summary>
        /// Determines which integrations this messages goes to.
        ///   new Options()
        ///     .Integration("All", false)
        ///     .Integration("Mixpanel", true)
        /// will send a message to only Mixpanel.
        /// </summary>
        /// <param name="integration">The integration name.</param>
        /// <param name="enabled">If set to <c>true</c>, then the integration is enabled.</param>
        public RSOptions SetIntegration(string integration, bool enabled)
        {
            this.Integrations.Add(integration, enabled);
            return this;
        }

        /// <summary>
        /// Enable destination specific options for integration.
        ///   new Options()
        ///     .Integration("Vero", new Model.Dict() {
        ///         "tags", new Model.Dict() {
        ///             { "id", "235FAG" },
        ///             { "action", "add" },
        ///             { "values", new string[] {"warriors", "giants", "niners"} }
        ///         }
        ///     });
        /// </summary>
        /// <param name="integration">The integration name.</param>
        /// <param name="value">Dict value</param>
        public RSOptions SetIntegration(string integration, Dict value)
        {
            this.Integrations.Add(integration, value);
            return this;
        }

    }
}