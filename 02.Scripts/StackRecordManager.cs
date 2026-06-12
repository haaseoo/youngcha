using TMPro;
using UnityEngine;

public class StackRecordManager : MonoBehaviour
{
    [Header("Record UI")]
    public GameObject recordPanel;
    public TMP_Text recordText;

    private const string RECORD_KEY = "YoungchaStackGameRecords";
    private const string RECORD_COUNT_KEY = "YoungchaStackGameRecordCount";

    private void Start()
    {
        CloseRecord();
    }

    public void SaveRecord(int score, int count)
    {
        int recordCount = PlayerPrefs.GetInt(RECORD_COUNT_KEY, 0);
        recordCount++;

        string newRecord = $"{recordCount}. Count : {count} / Score : {score}";

        string oldRecords = PlayerPrefs.GetString(RECORD_KEY, "");

        string savedRecords;

        if (string.IsNullOrEmpty(oldRecords))
        {
            savedRecords = newRecord;
        }
        else
        {
            // 최신 기록이 위에 오도록 저장
            savedRecords = newRecord + "\n" + oldRecords;
        }

        PlayerPrefs.SetString(RECORD_KEY, savedRecords);
        PlayerPrefs.SetInt(RECORD_COUNT_KEY, recordCount);
        PlayerPrefs.Save();

        Debug.Log("스택 게임 기록 저장 완료: " + newRecord);
    }

    public void ShowRecord()
    {
        if (recordPanel != null)
        {
            recordPanel.SetActive(true);
        }

        RefreshRecordText();
    }

    public void CloseRecord()
    {
        if (recordPanel != null)
        {
            recordPanel.SetActive(false);
        }
    }

    public void ClearRecord()
    {
        PlayerPrefs.DeleteKey(RECORD_KEY);
        PlayerPrefs.DeleteKey(RECORD_COUNT_KEY);
        PlayerPrefs.Save();

        if (recordText != null)
        {
            recordText.text = "기록이 삭제되었습니다.";
        }

        Debug.Log("스택 게임 기록 삭제 완료");
    }

    private void RefreshRecordText()
    {
        if (recordText == null) return;

        string records = PlayerPrefs.GetString(RECORD_KEY, "");

        if (string.IsNullOrEmpty(records))
        {
            recordText.text = "아직 저장된 기록이 없습니다.";
        }
        else
        {
            recordText.text = records;
        }
    }
}