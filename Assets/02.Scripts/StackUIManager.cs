using System.Collections;
using TMPro;
using UnityEngine;

public class StackUIManager : MonoBehaviour
{
    [Header("Game UI")]
    public TMP_Text scoreText;
    public TMP_Text countText;

    [Header("Start UI")]
    public TMP_Text startGuideText;
    public float blinkInterval = 0.45f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalCountText;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        HideGameOverPanel();
    }

    public void UpdateGameUI(int score, int count)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score}";
        }

        if (countText != null)
        {
            countText.text = $"Count : {count}";
        }
    }

    public void ShowStartGuide()
    {
        if (startGuideText == null) return;

        startGuideText.gameObject.SetActive(true);
        blinkCoroutine = StartCoroutine(BlinkStartText());
    }

    public void HideStartGuide()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (startGuideText != null)
        {
            startGuideText.gameObject.SetActive(false);
        }
    }

    private IEnumerator BlinkStartText()
    {
        while (true)
        {
            if (startGuideText != null)
            {
                startGuideText.enabled = true;
            }

            yield return new WaitForSeconds(blinkInterval);

            if (startGuideText != null)
            {
                startGuideText.enabled = false;
            }

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void ShowGameOverPanel(int score, int count)
    {
        HideStartGuide();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score : {score}";
        }

        if (finalCountText != null)
        {
            finalCountText.text = $"Count : {count}";
        }
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}