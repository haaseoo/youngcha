using UnityEngine;

public class GameBackgroundManager : MonoBehaviour
{
    [Header("Skybox Background")]
    public Material[] skyboxMaterials;

    private void Start()
    {
        ApplySavedBackground();
    }

    private void ApplySavedBackground()
    {
        if (skyboxMaterials == null || skyboxMaterials.Length == 0)
        {
            Debug.LogWarning("GameScene의 Skybox Materials 배열이 비어 있습니다.");
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt("SelectedBackgroundIndex", 0);

        if (selectedIndex < 0 || selectedIndex >= skyboxMaterials.Length)
        {
            selectedIndex = 0;
        }

        RenderSettings.skybox = skyboxMaterials[selectedIndex];
        DynamicGI.UpdateEnvironment();

        Debug.Log("GameScene에 저장된 배경 적용: " + skyboxMaterials[selectedIndex].name);
    }
}