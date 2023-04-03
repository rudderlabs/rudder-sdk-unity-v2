using System;
using System.Text;
using UnityEngine;

namespace RudderStack.Unity
{
    [HelpURL("https://app.rudderstack.com/")]
    public class RudderStackInitializer : MonoBehaviour
    {
        [Tooltip("You can find DataPlane on your RudderStack personal page")]
        [SerializeField] private string dataPlaneUrl;
        [Tooltip("You can find DataPlane in Source's setup")]
        [SerializeField] private string writeKey;
        [Space] 
        [Tooltip("Any string")]
        public string encryptionKey;
        [Space]
        [Tooltip("Log automatically if a new scene is loaded.")]
        [SerializeField] private bool enableAutoSceneLogs;
        [Tooltip("UserId for automatic scene logging.")]
        [SerializeField] private string userId;


        private static bool _workerCreated;

        public void Start()
        {
            if (string.IsNullOrEmpty(dataPlaneUrl)) 
                throw new ArgumentException(nameof(dataPlaneUrl));
            if (string.IsNullOrEmpty(writeKey)) 
                throw new ArgumentException(nameof(writeKey));

            if (!_workerCreated)
            {
                _workerCreated = true;
                var worker = new GameObject("RudderStack Worker");
                DontDestroyOnLoad(worker);
                worker.AddComponent<UnityMainThread>();
                if (enableAutoSceneLogs)
                {
                    worker.AddComponent<AutoDetectScreenChange>().userId = userId;
                }

                RSAnalytics.Initialize(writeKey, encryptionKey, new RSConfig(dataPlaneUrl: dataPlaneUrl).SetAutoCollectAdvertId(true));
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(encryptionKey))
            {
                using var aes = System.Security.Cryptography.Aes.Create();
                encryptionKey = Convert.ToBase64String(aes.Key);
            }
        }

        private void OnDestroy()
        {
            RSAnalytics.Dispose();
        }
    }
}