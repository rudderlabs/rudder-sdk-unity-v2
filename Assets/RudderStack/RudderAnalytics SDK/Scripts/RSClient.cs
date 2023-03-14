using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using RudderStack.Model;
using RudderStack.Stats;
using RudderStack.Unity.Utility;

namespace RudderStack.Unity
{
    public class RSClient : IRudderAnalyticsClient
    {
        private string _advertisingId;
        private string _deviceToken;
        private RSFailureRequestManager _storageManager;
        public bool IsConnected { get; private set; }

        public RudderClient Inner { get; }
        
        public Statistics Statistics
        {
            get => Inner.Statistics;
            set => Inner.Statistics = value;
        }

        public event FailedHandler Failed
        {
            add => Inner.Failed += value;
            remove => Inner.Failed -= value;
        }

        public event SucceededHandler Succeeded
        {
            add => Inner.Succeeded += value;
            remove => Inner.Succeeded -= value;
        }

        public RSClient(RudderClient innerClient)
        {
            Inner = innerClient;
            _storageManager = new RSFailureRequestManager(this);
        }
        
        public RSClient(string writeKey)
        {
            Inner = new RudderClient(writeKey, new RSConfig());
            _storageManager = new RSFailureRequestManager(this);
        }

        public RSClient(string writeKey, RSConfig config)
        {
            Inner = new RudderClient(writeKey, config);
            _storageManager = new RSFailureRequestManager(this);
        }

        public string WriteKey
        {
            get => Inner.WriteKey;
        }

        public RSConfig Config
        {
            get => Inner.Config as RSConfig;
        }

        RudderConfig IRudderAnalyticsClient.Config
        {
            get => Inner.Config;
        }

        public void Identify(string userId, IDictionary<string, object> traits) =>
            Identify(userId, traits, new RudderOptions());

        public void Identify(string userId, IDictionary<string, object> traits, RudderOptions options)
        {
            SetDeviceValues(null);
            Inner.Identify(userId, traits, options);
        }


        public void Group(string userId, string groupId, RudderOptions options) =>
            Group(userId, groupId, null, options);

        public void Group(string userId, string groupId, IDictionary<string, object> traits) =>
            Group(userId, groupId, traits, new RudderOptions());

        public void Group(string userId, string groupId, IDictionary<string, object> traits, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Group(userId, groupId, traits, options);
        }
        public void Track(Track track) =>
            Track(track.UserId, track.EventName, track.Properties, track.ToOptions());
        
        public void Track(string userId, string eventName) =>
            Track(userId, eventName, null, new RudderOptions());

        public void Track(string userId, string eventName, IDictionary<string, object> properties) =>
            Track(userId, eventName, properties, new RudderOptions());

        public void Track(string userId, string eventName, RudderOptions options) =>
            Track(userId, eventName, null, new RudderOptions());

        public void Track(
            string                      userId,
            string                      eventName,
            IDictionary<string, object> properties,
            RudderOptions               options)
        {
            SetDeviceValues(options);
            Inner.Track(userId, eventName, properties, options);
        }

        public void Alias(string previousId, string userId) =>
            Alias(previousId, userId, new RudderOptions());

        public void Alias(string previousId, string userId, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Alias(previousId, userId, options);
        }

        public void Page(string userId, string name) =>
            Page(userId, name, null, null, new RudderOptions());

        public void Page(string userId, string name, RudderOptions options) =>
            Page(userId, name, null, null, options);

        public void Page(string userId, string name, string category) =>
            Page(userId, name, category, null, new RudderOptions());

        public void Page(string userId, string name, IDictionary<string, object> properties) =>
            Page(userId, name, null, properties, new RudderOptions());

        public void Page(string userId, string name, IDictionary<string, object> properties, RudderOptions options) =>
            Page(userId, name, null, properties, options);

        public void Page(
            string                      userId,
            string                      name,
            string                      category,
            IDictionary<string, object> properties,
            RudderOptions               options)
        {
            SetDeviceValues(options);
            Inner.Page(userId, name, category, properties, options);
        }

        public void Screen(string userId, string name) =>
            Screen(userId, name, null, null, new RudderOptions());

        public void Screen(string userId, string name, RudderOptions options) =>
            Screen(userId, name, null, null, options);

        public void Screen(string userId, string name, string category) =>
            Screen(userId, name, category, null, new RudderOptions());

        public void Screen(string userId, string name, IDictionary<string, object> properties) =>
            Screen(userId, name, null, properties, new RudderOptions());

        public void Screen(string userId, string name, IDictionary<string, object> properties, RudderOptions options) =>
            Screen(userId, name, null, properties, options);

        public void Screen(
            string                      userId,
            string                      name,
            string                      category,
            IDictionary<string, object> properties,
            RudderOptions               options)
        {
            SetDeviceValues(options);
            Inner.Screen(userId, name, category, properties, options);
        }

        private void SetDeviceValues(RudderOptions options)
        {
            if (Config.GetAutoCollectAdvertId())
                options.Context.Add("device", new Dict
                {
                    { "token", _deviceToken },
                    { "adTrackingEnabled", true },
                    { "advertisingId", _advertisingId },
                });
        }

        public void Flush() => Inner.Flush();

        public Task FlushAsync() => Inner.FlushAsync();

        public void Dispose() => Inner.Dispose();

        /// <summary>
        /// Set the AdvertisingId yourself. If set, SDK will not capture idfa automatically
        /// <b>Call this method before initializing the RudderClient</b>
        /// </summary>
        /// <param name="advertisingId">IDFA for the device</param>
        public void PutAdvertisingId(string advertisingId)
        {
            _advertisingId = advertisingId;
        }
        
        /// <summary>
        /// Set the push token for the device to be passed to the downstream destinations
        /// </summary>
        /// <param name="deviceToken">Push Token from FCM</param>
        public void PutDeviceToken(string deviceToken)
        {
            _deviceToken = deviceToken;
        }
    }
}