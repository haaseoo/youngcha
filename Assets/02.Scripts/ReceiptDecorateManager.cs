using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReceiptDecorateManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject decorateButton;
    public GameObject decoratePanel;
    public TMP_InputField memoInputField;

    [Header("Receipt Preview UI")]
    public Image receiptPreviewImage;
    public TMP_Text receiptPreviewText;

    [Header("Receipt Background Materials")]
    public Material[] receiptBackgroundMaterials;

    [Header("Diary Save")]
    public DiarySaveManager diarySaveManager;

    private GameObject currentReceipt;
    private string savedMemo = "";

    private string currentStoreName = "상호명 없음";
    private string currentDate = "날짜 없음";
    private string currentAmount = "금액 없음";
    private string currentCategory = "분류 없음";

    private Material currentSelectedMaterial;
    private int currentBackgroundIndex = 0;

    private void Start()
    {
        if (decorateButton != null)
        {
            decorateButton.SetActive(false);
        }

        if (decoratePanel != null)
        {
            decoratePanel.SetActive(false);
        }

        if (memoInputField != null)
        {
            memoInputField.contentType = TMP_InputField.ContentType.Standard;
            memoInputField.inputType = TMP_InputField.InputType.Standard;
            memoInputField.characterValidation = TMP_InputField.CharacterValidation.None;
            memoInputField.lineType = TMP_InputField.LineType.MultiLineNewline;

            memoInputField.onValueChanged.RemoveListener(OnMemoChanged);
            memoInputField.onValueChanged.AddListener(OnMemoChanged);
        }

        ClearMemoInput();
    }

    public void SetCurrentReceipt(GameObject receiptObject)
    {
        currentReceipt = receiptObject;

        // 새 영수증이 들어오면 이전 메모 완전 초기화
        savedMemo = "";
        ClearMemoInput();

        ReceiptMemo receiptMemo = currentReceipt.GetComponent<ReceiptMemo>();
        if (receiptMemo != null)
        {
            receiptMemo.memoText = "";
        }

        if (decorateButton != null)
        {
            decorateButton.SetActive(true);
        }

        MeshRenderer meshRenderer = currentReceipt.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null)
        {
            currentSelectedMaterial = meshRenderer.material;
            ApplyMaterialToPreview(currentSelectedMaterial);
            currentBackgroundIndex = FindMaterialIndex(currentSelectedMaterial);
        }

        RefreshReceiptPreview();

        Debug.Log("꾸미기 대상 영수증 등록 완료: " + receiptObject.name);
    }

    public void SetReceiptInfo(string storeName, string date, string amount, string category)
    {
        currentStoreName = storeName;
        currentDate = date;
        currentAmount = amount;
        currentCategory = category;

        RefreshReceiptPreview();
    }

    public void OpenDecoratePanel()
    {
        if (currentReceipt == null)
        {
            Debug.LogWarning("꾸밀 영수증이 아직 없습니다.");
            return;
        }

        if (decoratePanel != null)
        {
            decoratePanel.SetActive(true);
            decoratePanel.transform.SetAsLastSibling();
        }

        // 꾸미기 창을 열 때마다 메모 입력창은 무조건 빈칸으로 시작
        savedMemo = "";
        ClearMemoInput();

        ReceiptMemo receiptMemo = currentReceipt.GetComponent<ReceiptMemo>();
        if (receiptMemo != null)
        {
            receiptMemo.memoText = "";
        }

        MeshRenderer meshRenderer = currentReceipt.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null)
        {
            currentSelectedMaterial = meshRenderer.material;
            ApplyMaterialToPreview(currentSelectedMaterial);
            currentBackgroundIndex = FindMaterialIndex(currentSelectedMaterial);
        }

        RefreshReceiptPreview();
    }

    public void CloseDecoratePanel()
    {
        if (decoratePanel != null)
        {
            decoratePanel.SetActive(false);
        }

        savedMemo = "";
        ClearMemoInput();
        RefreshReceiptPreview();
    }

    public void ChangeReceiptBackgroundRandom()
    {
        if (currentReceipt == null)
        {
            Debug.LogWarning("배경을 바꿀 영수증이 없습니다.");
            return;
        }

        if (receiptBackgroundMaterials == null || receiptBackgroundMaterials.Length == 0)
        {
            Debug.LogWarning("영수증 배경 머터리얼 배열이 비어 있습니다.");
            return;
        }

        int randomIndex = Random.Range(0, receiptBackgroundMaterials.Length);
        Material selectedMaterial = receiptBackgroundMaterials[randomIndex];

        MeshRenderer meshRenderer = currentReceipt.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null)
        {
            Debug.LogWarning("현재 영수증에 MeshRenderer가 없습니다.");
            return;
        }

        meshRenderer.material = selectedMaterial;

        currentSelectedMaterial = selectedMaterial;
        currentBackgroundIndex = randomIndex;

        ApplyMaterialToPreview(selectedMaterial);
        RefreshReceiptPreview();

        Debug.Log("영수증 배경 변경 완료: " + selectedMaterial.name);
    }

    public void SaveMemo()
    {
        if (currentReceipt == null)
        {
            Debug.LogWarning("메모를 저장할 영수증이 없습니다.");
            return;
        }

        if (memoInputField != null)
        {
            savedMemo = CleanMemoText(memoInputField.text);
        }

        ReceiptMemo receiptMemo = currentReceipt.GetComponent<ReceiptMemo>();

        if (receiptMemo == null)
        {
            receiptMemo = currentReceipt.AddComponent<ReceiptMemo>();
        }

        receiptMemo.memoText = savedMemo;

        if (diarySaveManager != null)
        {
            diarySaveManager.SaveDiary(
                currentStoreName,
                currentDate,
                currentAmount,
                currentCategory,
                savedMemo,
                currentBackgroundIndex
            );
        }
        else
        {
            Debug.LogWarning("DiarySaveManager가 연결되지 않았습니다.");
        }

        Debug.Log("영수증 메모 저장 완료: " + savedMemo);
        Debug.Log("저장된 배경 번호: " + currentBackgroundIndex);

        // 다이어리에 저장한 뒤 현재 영수증 오브젝트에는 메모를 남기지 않음
        receiptMemo.memoText = "";

        CloseDecoratePanel();
    }

    private void OnMemoChanged(string value)
    {
        RefreshReceiptPreview();
    }

    private void RefreshReceiptPreview()
    {
        if (receiptPreviewText == null) return;

        string memo = "";

        if (memoInputField != null)
        {
            memo = CleanMemoText(memoInputField.text);
        }

        receiptPreviewText.text =
            $"<size=45><b>{currentStoreName}</b></size>\n" +
            $"\n" +
            $"--------------------\n" +
            $"\n" +
            $"날짜  {currentDate}\n" +
            $"금액  {currentAmount}\n" +
            $"분류  {currentCategory}\n" +
            $"\n" +
            $"--------------------\n" +
            $"\n" +
            $"MEMO\n" +
            $"\n" +
            $"{memo}";
    }

    private void ClearMemoInput()
    {
        if (memoInputField == null) return;

        memoInputField.text = "";
        memoInputField.SetTextWithoutNotify("");

        memoInputField.caretPosition = 0;
        memoInputField.selectionAnchorPosition = 0;
        memoInputField.selectionFocusPosition = 0;

        memoInputField.ForceLabelUpdate();
    }

    private string CleanMemoText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string cleaned = text;

        cleaned = cleaned.Replace("<ㅂ>", "");
        cleaned = cleaned.Replace("</ㅂ>", "");
        cleaned = cleaned.Replace("<b>", "");
        cleaned = cleaned.Replace("</b>", "");
        cleaned = cleaned.Replace("<u>", "");
        cleaned = cleaned.Replace("</u>", "");
        cleaned = cleaned.Replace("•", "");

        return cleaned.Trim();
    }

    private void ApplyMaterialToPreview(Material material)
    {
        if (receiptPreviewImage == null)
        {
            Debug.LogWarning("Receipt Preview Image가 연결되지 않았습니다.");
            return;
        }

        if (material == null)
        {
            return;
        }

        Texture texture = material.mainTexture;

        if (texture == null)
        {
            receiptPreviewImage.sprite = null;
            receiptPreviewImage.color = material.color;
            return;
        }

        Texture2D texture2D = texture as Texture2D;

        if (texture2D == null)
        {
            Debug.LogWarning("미리보기로 변환할 수 없는 Texture 타입입니다.");
            return;
        }

        Sprite previewSprite = Sprite.Create(
            texture2D,
            new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f)
        );

        receiptPreviewImage.sprite = previewSprite;
        receiptPreviewImage.color = Color.white;
        receiptPreviewImage.type = Image.Type.Simple;
        receiptPreviewImage.preserveAspect = false;
    }

    private int FindMaterialIndex(Material material)
    {
        if (material == null || receiptBackgroundMaterials == null)
        {
            return 0;
        }

        for (int i = 0; i < receiptBackgroundMaterials.Length; i++)
        {
            if (receiptBackgroundMaterials[i] == null) continue;

            string currentName = material.name.Replace(" (Instance)", "");
            string arrayName = receiptBackgroundMaterials[i].name.Replace(" (Instance)", "");

            if (currentName == arrayName)
            {
                return i;
            }
        }

        return 0;
    }
}