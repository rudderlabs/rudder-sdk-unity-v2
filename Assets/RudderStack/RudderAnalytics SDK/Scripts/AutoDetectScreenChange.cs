using UnityEngine;
using UnityEngine.SceneManagement;

namespace RudderStack.Unity
{
    public class AutoDetectScreenChange : MonoBehaviour
    {
        public string userId;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                RSAnalytics.Client.Screen(userId, scene.name);
            }
        }
    }
}