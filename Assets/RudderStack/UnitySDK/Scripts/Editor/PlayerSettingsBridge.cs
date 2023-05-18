using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RudderStack.Unity
{
    public class RSPlayerSettingsBridge : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }
        public void OnPreprocessBuild(BuildReport report)
        {
            const string path = "Assets/RudderStack/UnitySDK/Settings.asset";

            var settings = AssetDatabase.LoadAssetAtPath<RSPlayerSettings>(path);

            if (settings is null)
            {
                settings             = ScriptableObject.CreateInstance<RSPlayerSettings>();
                settings.androidCode = PlayerSettings.Android.bundleVersionCode;
                settings.iosCode     = PlayerSettings.iOS.buildNumber;
                settings.packageName = PlayerSettings.applicationIdentifier;
                AssetDatabase.CreateAsset(settings, path);
            }
            else
            {
                settings.androidCode = PlayerSettings.Android.bundleVersionCode;
                settings.iosCode     = PlayerSettings.iOS.buildNumber;
                settings.packageName = PlayerSettings.applicationIdentifier;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#if UNITY_WSA
            if (!PlayerSettings.WSA.GetCapability(PlayerSettings.WSACapability.InternetClient))
            {
                throw new BuildFailedException("RudderStack SDK: InternetClient capability required." +
                                               "\n Set true PlayerSettings -> Publishing Settings -> Capabilities -> InternetClient");
            }
#endif
        }
    }
}