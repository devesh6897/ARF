using UnityEngine;
using UnityEngine.SceneManagement; // Import to manage scenes

public class SceneChanger : MonoBehaviour
{
    // Load a scene by name
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Load a scene by index (set in Build Settings)
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Reload the current scene
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Exit the application
    public void ExitApplication()
    {
        Debug.Log("Exiting Application..."); // Log for debugging

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else
            Application.Quit(); // Exit the application in a built game
#endif
    }
}
