using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySceneSwitch : MonoBehaviour
{
    public void ChangeToPlayScene()
    {
        // "PlayScene" 이라는 이름의 씬을 화면에 로드합니다.
        // ⚠️ 이동할 실제 씬의 이름과 따옴표 안의 글자가 대소문자까지 완벽히 일치해야 합니다!
        SceneManager.LoadScene("PlayScene");
    }
}