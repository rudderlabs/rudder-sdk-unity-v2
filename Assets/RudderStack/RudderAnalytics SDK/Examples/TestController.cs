using System.Collections.Generic;
using RudderStack;
using RudderStack.Model;
using RudderStack.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            RSAnalytics.Initialize(writeKey.text, FindObjectOfType<RudderStackInitializer>().encryptionKey, new RSConfig(dataPlaneUrl.text));
            RSLogger.LoggingHandler(Logger.Level.INFO, "RudderAnalytics Initialized", null);
        }

        public void Track()
        {
            RSAnalytics.Client.Track(
                userID.text,
                eventName.text,
                new Dictionary<string, object> { { propertyType.text, propertyValue.text }, }
            );
        }

        public void SetCredentials()
        {
            var token = deviceToken.text;
            var id    = advertisementId.text;
            
            RSAnalytics.Client.PutAdvertisingId(id);
            RSAnalytics.Client.PutDeviceToken(token);
        }

        public void SwitchScene()
        {
            SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example 1");
        }

        public void SwitchBack()
        {
            SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example");
        }
    }
}