using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiarySaveManager : MonoBehaviour
{
    [Header("Diary UI")]
    public GameObject diaryListPanel;

    [Header("Empty Text")]
    public TMP_Text emptyDiaryText;

    [Header("Card Spawn")]
    public Transform diaryContentParent;
    public GameObject diaryReceiptCardPrefab;

    [Header("Receipt Background Materials")]
    public Material[] receiptBackgroundMaterials;

    private const string DIARY_KEY = "YoungchaDiaryRecords";

    [Serializable]
    public class DiaryRecord
    {
        public string storeName;
        public string date;
        public string amount;
        public string category;
        public string memo;
        public int backgroundIndex;
    }

    [Serializable]
    public class DiaryRecordList
    {
        public List<DiaryRecord> records = new List<DiaryRecord>();
    }

    private void Start()
    {
        CloseDiaryList();
    }

    public void SaveDiary(string storeName, string date, string amount, string category, string memo, int backgroundIndex)
    {
        DiaryRecordList recordList = LoadRecordList();

        DiaryRecord newRecord = new DiaryRecord
        {
            storeName = storeName,
            date = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("yyyy.MM.dd HH:mm") : date,
            amount = amount,
            category = category,
            memo = memo,
            backgroundIndex = backgroundIndex
        };

        recordList.records.Insert(0, newRecord);

        string json = JsonUtility.ToJson(recordList);
        PlayerPrefs.SetString(DIARY_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("다이어리 저장 완료: " + newRecord.storeName);
        Debug.Log("저장된 배경 번호: " + backgroundIndex);
    }

    public void ShowDiaryList()
    {
        if (diaryListPanel != null)
        {
            diaryListPanel.SetActive(true);
            diaryListPanel.transform.SetAsLastSibling();
        }

        RefreshDiaryList();
    }

    public void CloseDiaryList()
    {
        if (diaryListPanel != null)
        {
            diaryListPanel.SetActive(false);
        }
    }

    public void ClearDiaryList()
    {
        PlayerPrefs.DeleteKey(DIARY_KEY);
        PlayerPrefs.Save();

        RefreshDiaryList();

        Debug.Log("다이어리 기록 삭제 완료");
    }

    private void RefreshDiaryList()
    {
        ClearSpawnedCards();

        DiaryRecordList recordList = LoadRecordList();

        if (recordList.records.Count == 0)
        {
            if (emptyDiaryText != null)
            {
                emptyDiaryText.gameObject.SetActive(true);
                emptyDiaryText.text = "아직 저장된 다이어리가 없습니다.";
            }

            return;
        }

        if (emptyDiaryText != null)
        {
            emptyDiaryText.gameObject.SetActive(false);
        }

        if (diaryReceiptCardPrefab == null)
        {
            Debug.LogError("Diary Receipt Card Prefab이 연결되지 않았습니다.");
            return;
        }

        if (diaryContentParent == null)
        {
            Debug.LogError("Diary Content Parent가 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < recordList.records.Count; i++)
        {
            DiaryRecord record = recordList.records[i];

            GameObject cardObj = Instantiate(diaryReceiptCardPrefab, diaryContentParent);

            DiaryReceiptCard card = cardObj.GetComponent<DiaryReceiptCard>();

            if (card == null)
            {
                Debug.LogWarning("DiaryReceiptCard 프리팹에 DiaryReceiptCard.cs가 붙어있지 않습니다.");
                continue;
            }

            Material selectedMaterial = GetBackgroundMaterial(record.backgroundIndex);

            card.SetCard(
                selectedMaterial,
                record.storeName,
                record.date,
                record.amount,
                record.category,
                record.memo
            );
        }
    }

    private DiaryRecordList LoadRecordList()
    {
        string json = PlayerPrefs.GetString(DIARY_KEY, "");

        if (string.IsNullOrEmpty(json))
        {
            return new DiaryRecordList();
        }

        try
        {
            DiaryRecordList recordList = JsonUtility.FromJson<DiaryRecordList>(json);

            if (recordList == null || recordList.records == null)
            {
                return new DiaryRecordList();
            }

            return recordList;
        }
        catch
        {
            Debug.LogWarning("기존 다이어리 저장 형식이 달라서 기록을 초기화합니다.");

            PlayerPrefs.DeleteKey(DIARY_KEY);
            PlayerPrefs.Save();

            return new DiaryRecordList();
        }
    }

    private void ClearSpawnedCards()
    {
        if (diaryContentParent == null) return;

        for (int i = diaryContentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(diaryContentParent.GetChild(i).gameObject);
        }
    }

    private Material GetBackgroundMaterial(int index)
    {
        if (receiptBackgroundMaterials == null || receiptBackgroundMaterials.Length == 0)
        {
            return null;
        }

        if (index < 0 || index >= receiptBackgroundMaterials.Length)
        {
            return receiptBackgroundMaterials[0];
        }

        return receiptBackgroundMaterials[index];
    }
}