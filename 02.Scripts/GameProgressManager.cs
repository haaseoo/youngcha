using UnityEngine;
using TMPro;

public class GameProgressManager : MonoBehaviour
{
    [Header("Level Setting")]
    public int level = 1;
    public int exp = 0;
    public int maxExp = 30;
    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text rightPanelText;

    [Header("Badge")]
    public BadgeManager badgeManager;

    private void Start()
    {
        UpdateUI();
    }
    public void AddReceiptExp()
    {
        exp += 10;

        if (exp >= maxExp)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        exp = 0;
        level++;

        if (level > 3)
        {
            level = 3;
            exp = maxExp;
            return;
        }

        if (level == 2)
        {
            maxExp = 60;
        }
        else if (level == 3)
        {
            maxExp = 100;
        }

        Debug.Log("Level Up! 현재 레벨: " + level);
    }

    public string GetRightPanelText()
    {
        return
            "Lv. " + level + "\n" +
            "Exp: " + exp + " / " + maxExp;
    }

    private void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }

        if (expText != null)
        {
            expText.text = exp + " / " + maxExp;
        }

        if (rightPanelText != null)
        {
            rightPanelText.text = GetRightPanelText();
        }

        if (badgeManager != null)
        {
            badgeManager.UpdateBadge(level);
        }
    }
}