using System.Collections.Generic;
using RudderStack;
using RudderStack.Model;
using RudderStack.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = RudderStack.Logger;

namespace Examples.RudderStack
{

    public class TestController : MonoBehaviour
    {
        [Header("Initialize")]
        [SerializeField] private TMP_InputField dataPlaneUrl;

        [SerializeField] private TMP_InputField writeKey;

        [Header("Track")]
        [SerializeField] private TMP_InputField userID;

        [SerializeField] private TMP_InputField eventName;
        [SerializeField] private TMP_InputField propertyType;
        [SerializeField] private TMP_InputField propertyValue;
        [SerializeField] private TMP_InputField deviceToken;
        [SerializeField] private TMP_InputField advertisementId;


        public void Initialize()
        {
            RsAnalytics.Initialize(writeKey.text, new RsConfig(dataPlaneUrl.text));
            CustomLogger.LoggingHandler(Logger.Level.INFO, "RudderAnalytics Initialized", null);
        }

        public void Track()
        {
            RsAnalytics.Client.Track(
                userID.text,
                eventName.text,
                new Dictionary<string, object> { { propertyType.text, propertyValue.text }, }
            );
        }

        public void SetCredentials()
        {
            var token = deviceToken.text;
            var id    = advertisementId.text;
            
            RsAnalytics.Client.PutAdvertisingId(id);
            RsAnalytics.Client.PutDeviceToken(token);
        }
    }
}