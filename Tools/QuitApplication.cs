using UnityEditor;
using UnityEngine;

namespace ItsBaptiste.Menu_system.Scripts {
    public class QuitApplication : MonoBehaviour {
        public void Quit() {
            StaticQuit();
        }

        public static void StaticQuit() {
            //If we are running in a standalone build of the game
#if UNITY_STANDALONE
            //Quit the application
            Application.Quit();
#endif

            //If we are running in the editor
#if UNITY_EDITOR
            //Stop playing the scene
            EditorApplication.isPlaying = false;
#endif
        }
    }
}