using UnityEngine;
using UnityEngine.SceneManagement;

namespace RudderStack.Unity
{
    [Tooltip("Log automatically if a new scene is loaded.")]
    public class RSScreenView : MonoBehaviour
    {
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
                RSAnalytics.Client.Screen(scene.name);
        }
    }
}