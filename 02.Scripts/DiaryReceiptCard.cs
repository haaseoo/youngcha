using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryReceiptCard : MonoBehaviour
{
    [Header("Card UI")]
    public Image receiptImage;
    public TMP_Text receiptText;

    public void SetCard(Material backgroundMaterial, string storeName, string date, string amount, string category, string memo)
    {
        ApplyMaterialToImage(backgroundMaterial);

        if (receiptText != null)
        {
            receiptText.text =
                $"<size=42><b>{storeName}</b></size>\n" +
                $"\n" +
                $"--------------------\n" +
                $"\n" +
                $"날짜  {date}\n" +
                $"금액  {amount}\n" +
                $"분류  {category}\n" +
                $"\n" +
                $"--------------------\n" +
                $"\n" +
                $"MEMO\n" +
                $"\n" +
                $"{memo}";
        }
    }

    private void ApplyMaterialToImage(Material material)
    {
        if (receiptImage == null)
        {
            Debug.LogWarning("DiaryReceiptCard의 Receipt Image가 연결되지 않았습니다.");
            return;
        }

        if (material == null)
        {
            receiptImage.color = Color.white;
            return;
        }

        Texture texture = material.mainTexture;

        if (texture == null)
        {
            receiptImage.sprite = null;
            receiptImage.color = material.color;
            return;
        }

        Texture2D texture2D = texture as Texture2D;

        if (texture2D == null)
        {
            Debug.LogWarning("Material의 텍스처를 Texture2D로 변환할 수 없습니다.");
            return;
        }

        Sprite sprite = Sprite.Create(
            texture2D,
            new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f)
        );

        receiptImage.sprite = sprite;
        receiptImage.color = Color.white;
        receiptImage.type = Image.Type.Simple;
        receiptImage.preserveAspect = false;
    }
}