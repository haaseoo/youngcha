using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ReceiptManager : MonoBehaviour
{
    [Header("Spawn Setting")]
    public Transform spawnPoint;

    [Header("UI")]
    public TMP_Text receiptInfoText;
    public TMP_Text saveMessageText;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("Receipt Paper Materials")]
    public Material[] receiptPaperMaterials;

    [Header("Receipt Object Font")]
    public TMP_FontAsset receiptFont;

    [Header("Skybox Background")]
    public Material[] skyboxMaterials;

    [Header("Game Progress")]
    public GameProgressManager gameProgressManager;

    [Header("Decorate Manager")]
    public ReceiptDecorateManager receiptDecorateManager;

    [Header("API")]
    public string apiUrl = "http://127.0.0.1:5000/ocr";

    private int backgroundIndex = 0;
    private int sampleImageIndex = 0;

    private int receiptCount = 0;
    private int totalAmount = 0;

    private string[] sampleImageNames =
    {
        "sample_receipt_1.jpg",
        "sample_receipt_2.jpg",
        "sample_receipt_3.jpg",
        "sample_receipt_4.jpg",
        "sample_receipt_5.jpg",
        "sample_receipt_6.jpg"
    };

    [System.Serializable]
    public class ReceiptData
    {
        public string storeName;
        public string date;
        public int amount;
        public string category;
        public string feedback;
    }

    private Dictionary<string, string[]> categoryFeedbackMap = new Dictionary<string, string[]>()
    {
        {
            "교통",
            new string[]
            {
                "이동 기록이 추가되었습니다.\n오늘의 동선도 소비 아카이브에 저장되었어요.",
                "교통비가 기록되었습니다.\n작은 이동 비용도 모이면 생활 패턴이 됩니다."
            }
        },
        {
            "식비",
            new string[]
            {
                "식비 영수증이 추가되었습니다.\n오늘의 식사 기록이 보관되었어요.",
                "음식 소비가 기록되었습니다.\n나의 하루 식사 패턴을 확인할 수 있어요."
            }
        },
        {
            "의류",
            new string[]
            {
                "의류 소비가 추가되었습니다.\n나만의 스타일 기록이 쌓이고 있어요.",
                "패션 아이템 구매가 기록되었습니다.\n소비 아카이브에 옷장이 채워졌어요."
            }
        },
        {
            "화장품",
            new string[]
            {
                "화장품 소비가 추가되었습니다.\n뷰티 아이템 기록이 저장되었어요.",
                "뷰티 소비가 기록되었습니다.\n나만의 꾸미기 컬렉션이 늘어났어요."
            }
        },
        {
            "카페",
            new string[]
            {
                "카페 영수증이 추가되었습니다.\n오늘의 작은 휴식이 기록되었어요.",
                "카페 소비가 기록되었습니다.\n나의 커피 타임이 아카이브에 저장되었어요."
            }
        },
        {
            "문구",
            new string[]
            {
                "문구 소비가 추가되었습니다.\n공부와 기록을 위한 아이템이 저장되었어요.",
                "문구류 구매가 기록되었습니다.\n생산적인 소비 기록이 쌓이고 있어요."
            }
        },
        {
            "편의점",
            new string[]
            {
                "편의점 소비가 추가되었습니다.\n일상 속 작은 소비도 기록되었어요.",
                "간편 소비가 기록되었습니다.\n자주 쓰는 생활비 패턴을 확인할 수 있어요."
            }
        }
    };

    private void Start()
    {
        if (receiptInfoText != null)
        {
            receiptInfoText.text =
                "누적 영수증  0개\n" +
                "총 소비금액  0원";
        }

        if (saveMessageText != null)
        {
            saveMessageText.text = "영수증을 추가해보세요!";
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "영수증을 쌓아\n" +
                "나만의 소비 아카이브를 완성하세요!";
        }

        if (progressText != null && gameProgressManager != null)
        {
            progressText.text = gameProgressManager.GetRightPanelText();
        }

        ApplyFirstBackground();
    }

    public void RequestOCRFromServer()
    {
        StartCoroutine(SendOCRRequest());
    }

    private IEnumerator SendOCRRequest()
    {
        if (saveMessageText != null)
        {
            saveMessageText.text = "AI 분석 중...";
        }

        if (feedbackText != null)
        {
            feedbackText.text =
                "영수증 이미지를 읽는 중입니다...\n" +
                "AI가 소비 정보를 추출하고 있어요.";
        }

        yield return new WaitForSeconds(0.8f);

        string selectedImageName = sampleImageNames[sampleImageIndex];

        sampleImageIndex++;

        if (sampleImageIndex >= sampleImageNames.Length)
        {
            sampleImageIndex = 0;
        }

        string imagePath = Path.Combine(Application.streamingAssetsPath, selectedImageName);

        if (!File.Exists(imagePath))
        {
            Debug.LogError("샘플 영수증 이미지가 없습니다: " + imagePath);

            if (saveMessageText != null)
            {
                saveMessageText.text = "이미지 없음";
            }

            if (feedbackText != null)
            {
                feedbackText.text =
                    "Assets/StreamingAssets 폴더 안에\n" +
                    selectedImageName + " 파일을 넣어주세요.";
            }

            yield break;
        }

        byte[] imageBytes = File.ReadAllBytes(imagePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, selectedImageName, "image/jpeg");

        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
        {
            yield return request.SendWebRequest();

            Debug.Log("응답 코드: " + request.responseCode);
            Debug.Log("응답 내용: " + request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API 요청 실패: " + request.error);

                if (saveMessageText != null)
                {
                    saveMessageText.text = "API 연결 실패";
                }

                if (feedbackText != null)
                {
                    feedbackText.text =
                        "Flask 서버가 켜져 있는지 확인하세요.\n\n" +
                        "응답 코드: " + request.responseCode + "\n" +
                        request.error;
                }

                yield break;
            }

            if (saveMessageText != null)
            {
                saveMessageText.text = "AI 분석 완료";
            }

            if (feedbackText != null)
            {
                feedbackText.text = "분석 결과를 3D 아카이브에 저장하는 중입니다...";
            }

            yield return new WaitForSeconds(0.4f);

            string json = request.downloadHandler.text;
            Debug.Log("서버 응답 JSON: " + json);

            ReceiptData data = JsonUtility.FromJson<ReceiptData>(json);

            data.feedback = GetRandomCategoryFeedback(data.category);

            AddReceiptRecord(data);
            CreateReceiptObject(data);
            UpdateResultUI(data);
            UpdateProgressUI();
        }
    }

    private void AddReceiptRecord(ReceiptData data)
    {
        receiptCount++;
        totalAmount += data.amount;

        Debug.Log("영수증 기록 추가됨: " + data.storeName);

        if (gameProgressManager != null)
        {
            gameProgressManager.AddReceiptExp();
        }
        else
        {
            Debug.LogWarning("GameProgressManager가 연결되지 않았습니다.");
        }
    }

    private string GetRandomCategoryFeedback(string category)
    {
        if (categoryFeedbackMap.ContainsKey(category))
        {
            string[] feedbacks = categoryFeedbackMap[category];
            int randomIndex = Random.Range(0, feedbacks.Length);
            return feedbacks[randomIndex];
        }

        return "소비 기록이 추가되었습니다.\n영수증이 나만의 아카이브에 저장되었어요.";
    }

    private void CreateReceiptObject(ReceiptData data)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint가 연결되지 않았습니다.");
            return;
        }

        Vector3 randomOffset = new Vector3(
            Random.Range(-1.2f, 1.2f),
            0,
            Random.Range(-1.2f, 1.2f)
        );

        Vector3 spawnPosition = spawnPoint.position + randomOffset;

        float sizeMultiplier = Mathf.Clamp(data.amount / 10000f, 0.85f, 1.55f);

        GameObject receipt = new GameObject("Receipt_" + data.storeName);

        receipt.transform.position = spawnPosition;
        receipt.transform.rotation = Quaternion.Euler(
            Random.Range(-12f, 12f),
            Random.Range(0f, 360f),
            Random.Range(-12f, 12f)
        );

        MeshFilter meshFilter = receipt.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = receipt.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateSoftReceiptMesh(
            width: 1.25f * sizeMultiplier,
            height: 2.05f * sizeMultiplier,
            xSegments: 6,
            ySegments: 10
        );

        ApplyRandomPaperMaterial(meshRenderer);

        Rigidbody rb = receipt.AddComponent<Rigidbody>();
        rb.mass = 0.08f;
        rb.useGravity = true;
        rb.linearDamping = 1.2f;
        rb.angularDamping = 3.5f;

        BoxCollider boxCollider = receipt.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(1.25f * sizeMultiplier, 0.16f, 2.05f * sizeMultiplier);
        boxCollider.center = new Vector3(0f, 0.08f, 0f);

        CreateReceiptText(receipt.transform, data);

        if (receiptDecorateManager != null)
        {
            receiptDecorateManager.SetCurrentReceipt(receipt);

            receiptDecorateManager.SetReceiptInfo(
                data.storeName,
                data.date,
                data.amount.ToString("N0") + "원",
                data.category
            );
        }
        else
        {
            Debug.LogWarning("ReceiptDecorateManager가 연결되지 않았습니다.");
        }
    }

    private void ApplyRandomPaperMaterial(MeshRenderer meshRenderer)
    {
        if (receiptPaperMaterials != null && receiptPaperMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, receiptPaperMaterials.Length);
            meshRenderer.material = receiptPaperMaterials[randomIndex];
        }
        else
        {
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = new Color(0.97f, 0.95f, 0.88f);
        }
    }

    private Mesh CreateSoftReceiptMesh(float width, float height, int xSegments, int ySegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Soft Receipt Paper Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int y = 0; y <= ySegments; y++)
        {
            for (int x = 0; x <= xSegments; x++)
            {
                float xPercent = x / (float)xSegments;
                float yPercent = y / (float)ySegments;

                float px = (xPercent - 0.5f) * width;
                float pz = (yPercent - 0.5f) * height;

                float wave =
                    Mathf.Sin(yPercent * Mathf.PI * 2f) * 0.03f +
                    Mathf.Sin(xPercent * Mathf.PI * 3f) * 0.018f +
                    Random.Range(-0.012f, 0.012f);

                if (x == 0 || x == xSegments || y == 0 || y == ySegments)
                {
                    px += Random.Range(-0.018f, 0.018f);
                    pz += Random.Range(-0.018f, 0.018f);
                }

                vertices.Add(new Vector3(px, wave + 0.1f, pz));
                uvs.Add(new Vector2(xPercent, yPercent));
            }
        }

        for (int y = 0; y < ySegments; y++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                int i = y * (xSegments + 1) + x;

                triangles.Add(i);
                triangles.Add(i + xSegments + 1);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + xSegments + 1);
                triangles.Add(i + xSegments + 2);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void CreateReceiptText(Transform parent, ReceiptData data)
    {
        GameObject textObj = new GameObject("ReceiptText");
        textObj.transform.SetParent(parent);

        textObj.transform.localPosition = new Vector3(0f, 0.13f, 0f);
        textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        textObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

        TMP_Text text = textObj.AddComponent<TextMeshPro>();

        if (receiptFont != null)
        {
            text.font = receiptFont;
        }

        text.text =
            $"<b>{data.storeName}</b>\n" +
            $"----------------\n" +
            $"날짜  {data.date}\n" +
            $"금액  {data.amount:N0}원\n" +
            $"분류  {data.category}\n" +
            $"----------------";

        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 8.0f;
        text.color = Color.black;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private void UpdateResultUI(ReceiptData data)
    {
        if (receiptInfoText == null || saveMessageText == null || feedbackText == null)
        {
            Debug.LogWarning("UI Text가 연결되지 않았습니다.");
            return;
        }

        receiptInfoText.text =
            $"누적 영수증  {receiptCount}개\n" +
            $"총 소비금액  {totalAmount:N0}원\n\n" +
            $"상호명  {data.storeName}\n" +
            $"금액    {data.amount:N0}원\n" +
            $"분류    {data.category}\n";

        saveMessageText.text = "영차! 기록 저장 완료!";

        feedbackText.text = data.feedback;
    }

    private void UpdateProgressUI()
    {
        if (progressText == null)
        {
            Debug.LogWarning("Progress Text가 연결되지 않았습니다.");
            return;
        }

        if (gameProgressManager == null)
        {
            progressText.text = "GameProgressManager가 연결되지 않았습니다.";
            return;
        }

        progressText.text = gameProgressManager.GetRightPanelText();
    }

    public void ChangeBackgroundImage()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0)
        {
            Debug.LogWarning("Skybox Materials 배열이 비어 있습니다.");
            return;
        }

        backgroundIndex++;

        if (backgroundIndex >= skyboxMaterials.Length)
        {
            backgroundIndex = 0;
        }

        RenderSettings.skybox = skyboxMaterials[backgroundIndex];
        DynamicGI.UpdateEnvironment();
    }

    private void ApplyFirstBackground()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0)
        {
            return;
        }

        backgroundIndex = 0;

        RenderSettings.skybox = skyboxMaterials[backgroundIndex];
        DynamicGI.UpdateEnvironment();
    }
}