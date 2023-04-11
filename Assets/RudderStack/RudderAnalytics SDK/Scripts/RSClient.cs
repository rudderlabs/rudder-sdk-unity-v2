using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RudderStack.Model;
using RudderStack.Stats;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSClient
    {
        private const string AnonIdKey = "rudder-stack-anon-id";
        
        private string _advertisingId;
        private string _deviceToken;

        public string UserId
        {
            get;
            private set;
        }

        public IDictionary<string, object> UserTraits
        {
            get;
            private set;
        }

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
        }

        public string AnonymousId
        {
            get
            {
                if (PlayerPrefs.HasKey(AnonIdKey))
                {
                    return PlayerPrefs.GetString(AnonIdKey);
                }
                else
                {
                    var id = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString(AnonIdKey, id);
                    return id;
                }
            }
        }
        
        /*
        public RSClient(string writeKey)
        {
            Inner = new RudderClient(writeKey, new RSConfig(), null);
        }

        public RSClient(string writeKey, RSConfig config)
        {
            Inner = new RudderClient(writeKey, config, null);
        }
        */

        public string WriteKey
        {
            get => Inner.WriteKey;
        }

        public RSConfig Config
        {
            get => Inner.Config as RSConfig;
        }

        public void Identify(string userId) =>
            Identify(userId, null, new RudderOptions());
        
        public void Identify(string userId, IDictionary<string, object> traits) =>
            Identify(userId, traits, new RudderOptions());

        public void Identify(string userId, IDictionary<string, object> traits, RudderOptions options)
        {
            UserId     = userId;
            UserTraits = traits;
            SetDeviceValues(options);
            Inner.Identify(userId, traits, options);
        }


        public void Group(string groupId, RudderOptions options) =>
            Group(groupId, null, options);

        public void Group(string groupId, IDictionary<string, object> traits) =>
            Group(groupId, traits, new RudderOptions());

        public void Group(string groupId, IDictionary<string, object> traits, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Group(UserId, groupId, traits, options);
        }

        public void Track(string eventName) =>
            Track(eventName, null, new RudderOptions());

        public void Track(string eventName, IDictionary<string, object> properties) =>
            Track(eventName, properties, new RudderOptions());

        public void Track(string eventName, RudderOptions options) =>
            Track(eventName, null, new RudderOptions());

        public void Track(string eventName, IDictionary<string, object> properties, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Track(UserId, eventName, properties, options);
        }

        public void Alias(string userId) =>
            Alias(userId, new RudderOptions());

        public void Alias(string userId, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Alias(UserId, userId, options);
            UserId = userId;
        }

        public void Page(string name) =>
            Page(name, null, null, new RudderOptions());

        public void Page(string name, RudderOptions options) =>
            Page(name, null, null, options);

        public void Page(string name, string category) =>
            Page(name, category, null, new RudderOptions());

        public void Page(string name, IDictionary<string, object> properties) =>
            Page(name, null, properties, new RudderOptions());

        public void Page(string name, IDictionary<string, object> properties, RudderOptions options) =>
            Page(name, null, properties, options);

        public void Page(string name, string category, IDictionary<string, object> properties, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Page(UserId, name, category, properties, options);
        }

        public void Screen(string name) =>
            Screen(name, null, null, new RudderOptions());

        public void Screen(string name, RudderOptions options) =>
            Screen(name, null, null, options);

        public void Screen(string name, string category) =>
            Screen(name, category, null, new RudderOptions());

        public void Screen(string name, IDictionary<string, object> properties) =>
            Screen(name, null, properties, new RudderOptions());

        public void Screen(string name, IDictionary<string, object> properties, RudderOptions options) =>
            Screen(name, null, properties, options);

        public void Screen(string name, string category, IDictionary<string, object> properties, RudderOptions options)
        {
            SetDeviceValues(options);
            Inner.Screen(UserId, name, category, properties, options);
        }

        private void SetDeviceValues(RudderOptions options)
        {
            if (Config.GetAutoCollectAdvertId())
            {
                var value = new Dict {
                    { "token", _deviceToken },
                    { "adTrackingEnabled", true },
                    { "advertisingId", _advertisingId },
                };
                
                if (options.Context.ContainsKey("device"))
                    options.Context["device"] = value;
                else
                    options.Context.Add("device", value);
            }

            options.Context.Add("traits", UserTraits);
            options.SetAnonymousId(AnonIdKey);
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

        public void Reset()
        {
            Flush();
            PlayerPrefs.DeleteKey(AnonIdKey);
            UserId     = null;
            UserTraits = null;

        }
    }
}