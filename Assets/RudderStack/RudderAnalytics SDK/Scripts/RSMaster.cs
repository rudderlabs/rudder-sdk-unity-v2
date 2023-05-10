using System;
using System.Collections;
using System.Collections.Generic;
using RudderStack.Model;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSMaster : MonoBehaviour
    {
        private const  string lastSavedVersionKey = "rudderstack-last-version";
        private const  string installedKey        = "rudderstack-install-reported";

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

            if (RSAnalytics.Client.Config.GetTrackLifeCycleEvents() == false || opened) yield break;

            opened = true;
            
            if (!PlayerPrefs.HasKey(installedKey))
            {
                PlayerPrefs.SetInt(installedKey, 1);
                RSAnalytics.Client.Track("Application Installed", new Dict { { "version", Application.version } });
            }

            var prevVersion = PlayerPrefs.GetString(lastSavedVersionKey);
            if (!string.IsNullOrEmpty(prevVersion) && string.Compare(Application.version, prevVersion, StringComparison.Ordinal) > 0)
            {
                RSAnalytics.Client.Track("Application Updated",
                    new Dict { { "previous_version", prevVersion }, { "version", Application.version } });
                PlayerPrefs.SetString(lastSavedVersionKey, Application.version);
            }

            RSAnalytics.Client.Track("Application Opened",
                new Dict { { "version", Application.version }, { "from_background", false } });
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
        [UnityEditor.MenuItem("GameObject/RudderStack object")]
        public static void CreateInstance()
        {
            var instance = UnityEditor.PrefabUtility.InstantiatePrefab(
                UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/RudderStack/RudderAnalytics SDK/Prefabs/RudderStack.prefab"));
            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Create RS Object");
        }
#endif
    }
}