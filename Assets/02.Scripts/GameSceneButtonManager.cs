using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneButtonManager : MonoBehaviour
{
    [Header("Scene Name")]
    public string mainSceneName = "PlayScene";

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}