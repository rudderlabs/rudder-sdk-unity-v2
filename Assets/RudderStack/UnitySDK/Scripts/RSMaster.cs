using System.Collections;
using RudderStack.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RudderStack.Unity
{
    public class RSMaster : MonoBehaviour
    {
        public RSPlayerSettings settings;
        
        private const string lastSavedVersionKey = "rudderstack-last-version";
        private const string installedKey        = "rudderstack-install-reported";

        private static bool opened;

        public static RSMaster Instance
        {
            get;
            private set;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => RSAnalytics.Client != null);

            if (opened) yield break;

            opened = true;

            if (RSAnalytics.Client.Config.GetTrackLifeCycleEvents())
            {
                if (!PlayerPrefs.HasKey(installedKey))
                {
                    PlayerPrefs.SetInt(installedKey, 1);
                    RSAnalytics.Client.Track("Application Installed", new Dict { { "version", Application.version } });

                    PlayerPrefs.SetString(lastSavedVersionKey, Application.version);
                }
                else
                {
                    var prevVersion = PlayerPrefs.GetString(lastSavedVersionKey);
                    if (!string.IsNullOrEmpty(prevVersion) && prevVersion != Application.version)
                    {
                        RSAnalytics.Client.Track("Application Updated",
                            new Dict { { "previous_version", prevVersion }, { "version", Application.version } });

                        PlayerPrefs.SetString(lastSavedVersionKey, Application.version);
                    }
                }


                RSAnalytics.Client.Track("Application Opened",
                    new Dict { { "version", Application.version }, { "from_background", false } });
            }

            if (RSAnalytics.Client.Config.GetRecordScreenViews())
                RSAnalytics.Client.Screen(SceneManager.GetActiveScene().name);

        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (RSAnalytics.Client == null || !RSAnalytics.Client.Config.GetTrackLifeCycleEvents()) return;
            
            if (pauseStatus)
            {
                RSAnalytics.Client.Track("Application Backgrounded");
            }
            else
            {
                RSAnalytics.Client.Track("Application Opened",
                    new Dict { { "version", Application.version }, { "from_background", true } });
            }
        }

        private void OnApplicationQuit()
        {
            if (RSAnalytics.Client != null) 
                RSAnalytics.Dispose();
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!settings)
            {
                settings = UnityEditor.AssetDatabase.LoadAssetAtPath<RSPlayerSettings>(
                    "Assets/RudderStack/UnitySDK/Settings.asset");
            }
        }
        
        [UnityEditor.MenuItem("GameObject/RudderStack object")]
        public static void CreateInstance()
        {
            var instance = UnityEditor.PrefabUtility.InstantiatePrefab(
                UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/RudderStack/UnitySDK/Prefabs/RudderStack.prefab"));
            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Create RS Object");
        }
        
#endif
    }
}