using UnityEngine;

namespace RudderStack.Unity
{
    public class RSMaster : MonoBehaviour
    {
        private static bool _alreadyExists;
        
        private void Awake()
        {
            if (_alreadyExists)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _alreadyExists = true;
            }
        }

        private void OnApplicationQuit()
        {
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