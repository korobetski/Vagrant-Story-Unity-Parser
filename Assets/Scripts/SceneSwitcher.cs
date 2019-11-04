using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static void GoToScene(string sceneName)
    {
        Scene activeSc = SceneManager.GetActiveScene();
        GameObject[] GOs = activeSc.GetRootGameObjects();
        foreach (GameObject go in GOs)
        {
            if (go.isStatic == false)
            {
                go.SetActive(false);
            }
        }

        SceneManager.UnloadSceneAsync(activeSc);
        SceneManager.LoadSceneAsync(sceneName);
    }
    public void ToScene(string sceneName)
    {
        GoToScene(sceneName);
    }

    public static void ReturnToMainMenu()
    {
        GoToScene("MainMenu");
    }


    public static void Quit()
    {
        Application.Quit();
    }
}
