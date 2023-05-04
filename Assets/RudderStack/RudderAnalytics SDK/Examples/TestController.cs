using System.Collections.Generic;
using RudderStack.Model;
using RudderStack.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = RudderStack.Logger;

namespace Examples.RudderStack.Unity
{

    public class TestController : MonoBehaviour
    {
        [Header("Initialization")]
        [SerializeField] private InputField dataPlane;
        [SerializeField] private InputField writeKey;
        [Space]
        [SerializeField] private InputField deviceToken;
        [SerializeField] private InputField advertisementId;
        
        [Header("Identification")]
        [SerializeField] private InputField userID;
        [SerializeField] private InputField newUserId;
        [SerializeField] private Text       userDisplay;

        [Header("Track & Page")]
        [SerializeField] private InputField eventName;
        [SerializeField] private InputField propertyType;
        [SerializeField] private InputField propertyValue;

        private void Update()
        {
            if (userDisplay)
            {
                var id = RSAnalytics.Client?.UserId;
                userDisplay.text = string.IsNullOrEmpty(id) ? "NULL" : id;
            }
        }

        public void PutAdvertisingId() => RSClient.PutAdvertisingId(advertisementId.text);

        public void PutDeviceToken() => RSClient.PutDeviceToken(deviceToken.text);
        
        public void Initialize() =>
            StartCoroutine(RSAnalytics.Initialize(writeKey.text,
                new RSConfig(dataPlaneUrl: dataPlane.text)
                    .SetAutoCollectAdvertId(true)
                    .SetGzip(true)
                    .SetLogLevel(Logger.Level.DEBUG)
                    .SetControlPlaneUrl("https://api.rudderlabs.com")
                    .SetRecordScreenViews(true)));

        public void IdentifyUser() => RSAnalytics.Client.Identify(userID.text, new Dict());
        public void AliasUser()    => RSAnalytics.Client.Alias(newUserId.text);
        public void ResetUser()    => RSAnalytics.Client.Reset();

        public void Track() =>
            RSAnalytics.Client.Track(
                eventName.text,
                new Dictionary<string, object> { { propertyType.text, propertyValue.text }, } );

        public void Page() =>
            RSAnalytics.Client.Page(
                eventName.text,
                new Dictionary<string, object> { { propertyType.text, propertyValue.text }, } );


        public void Group() =>
            RSAnalytics.Client.Group(propertyValue.text, new Dict());

        public void SwitchScene() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example 1");

        public void SwitchBack() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example");
    }
}