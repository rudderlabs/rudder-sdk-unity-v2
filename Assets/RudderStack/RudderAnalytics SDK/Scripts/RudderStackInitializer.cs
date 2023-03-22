using System;
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
        [SerializeField] private string encryptionKey;
        [Space]
        [Tooltip("Log automatically if a new scene is loaded.")]
        [SerializeField] private bool enableAutoSceneLogs;
        [Tooltip("UserId for automatic scene logging.")]
        [SerializeField] private string userId;

        private RSFailureRequestManager _failureRequestManager;

        private static bool _workerCreated;

        private void Start()
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
            }

            RSAnalytics.Initialize(writeKey, new RSConfig(dataPlaneUrl: dataPlaneUrl).SetAutoCollectAdvertId(true));
            _failureRequestManager = new RSFailureRequestManager(RSAnalytics.Client, encryptionKey);
        }

        private void OnDestroy()
        {
            _failureRequestManager.Dispose();
        }
    }
}