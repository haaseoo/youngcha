using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BadgeManager : MonoBehaviour
{
    [Header("Badge UI")]
    public Image badgeImage;

    [Header("Badge Sprites")]
    public Sprite level1Badge;
    public Sprite level2Badge;
    public Sprite level3Badge;

    [Header("Animation Setting")]
    public float popScale = 1.35f;
    public float popSpeed = 0.12f;
    public float returnSpeed = 0.15f;

    private int currentBadgeLevel = 0;
    private Coroutine badgeEffectCoroutine;

    public void UpdateBadge(int level)
    {
        if (badgeImage == null)
        {
            Debug.LogWarning("Badge Image가 연결되지 않았습니다.");
            return;
        }

        // 같은 레벨이면 효과 반복 실행 X
        if (currentBadgeLevel == level)
        {
            return;
        }

        currentBadgeLevel = level;

        if (level == 1)
        {
            badgeImage.sprite = level1Badge;
        }
        else if (level == 2)
        {
            badgeImage.sprite = level2Badge;
        }
        else if (level == 3)
        {
            badgeImage.sprite = level3Badge;
        }

        PlayBadgeEffect();
    }

    private void PlayBadgeEffect()
    {
        if (badgeEffectCoroutine != null)
        {
            StopCoroutine(badgeEffectCoroutine);
        }

        badgeEffectCoroutine = StartCoroutine(BadgePopEffect());
    }

    private IEnumerator BadgePopEffect()
    {
        RectTransform rect = badgeImage.rectTransform;

        Vector3 normalScale = Vector3.one;
        Vector3 bigScale = Vector3.one * popScale;

        rect.localScale = normalScale;

        float time = 0f;

        // 커지는 구간
        while (time < popSpeed)
        {
            time += Time.deltaTime;
            float t = time / popSpeed;
            rect.localScale = Vector3.Lerp(normalScale, bigScale, t);
            yield return null;
        }

        rect.localScale = bigScale;

        time = 0f;

        // 다시 작아지는 구간
        while (time < returnSpeed)
        {
            time += Time.deltaTime;
            float t = time / returnSpeed;
            rect.localScale = Vector3.Lerp(bigScale, normalScale, t);
            yield return null;
        }

        rect.localScale = normalScale;
    }
}