using System;
using System.Text;
using RudderStack.Model;
using UnityEngine;

namespace RudderStack.Unity
{
    [ExecuteAlways]
    [HelpURL("https://app.rudderstack.com/")]
    public class RudderStackInitializer : MonoBehaviour
    {
        [Tooltip("You can find DataPlane on your RudderStack personal page")]
        [SerializeField] private string dataPlaneUrl;

        [Tooltip("You can find DataPlane in Source's setup")]
        [SerializeField] private string writeKey;

        [Space]
        [Tooltip("Log automatically if a new scene is loaded.")]
        [SerializeField] private bool enableAutoSceneLogs;

        [Tooltip("UserId for automatic scene logging.")]
        [SerializeField] private string userId;


        private static bool _workerCreated;

        public void Start()
        {
            if (Application.isPlaying)
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

                    RSAnalytics.Initialize(writeKey,
                        new RSConfig(dataPlaneUrl: dataPlaneUrl).SetAutoCollectAdvertId(true));
                    

                    StartCoroutine(RSAnalytics.FetchConfig(writeKey, print));
                }
            }
            else
            {
#if UNITY_EDITOR
                if (Resources.Load<RsEncryptionKey>("rudderStackEncryptionKey") == null)
                {
                    const string path =
                        "Assets/RudderStack/RudderAnalytics SDK/Resources/rudderStackEncryptionKey.asset";

                    var example = ScriptableObject.CreateInstance<RsEncryptionKey>();
                    example.GenerateKey();
                    
                    UnityEditor.AssetDatabase.CreateAsset(example, path);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                }
#endif
            }
        }

        private void OnDestroy()
        {
            if (Application.isPlaying) RSAnalytics.Dispose();
        }
    }
}