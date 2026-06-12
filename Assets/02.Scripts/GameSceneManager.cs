using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public void GoToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}