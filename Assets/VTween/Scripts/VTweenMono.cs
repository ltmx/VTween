using UnityEngine;
using UnityEngine.SceneManagement;

namespace VTWeen
{
    public class VTweenMono : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(this);
            SceneManager.activeSceneChanged += ChangedActiveScene;
        }
        private void ChangedActiveScene(Scene current, Scene next)
        {
            VTweenManager.AbortVTweenWorker();
        }
        void OnApplicationQuit()
        {
            VTweenManager.AbortVTweenWorker();
        }
    }
}
