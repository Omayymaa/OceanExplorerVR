using UnityEngine;

public class PollutionManager : MonoBehaviour
{
    public static PollutionManager Instance;
    public float maxPollution = 100f;
    public float currentPollution = 0f;
    public Material waterMaterial;
    public Color cleanColor = new Color(0.2f, 0.6f, 1f, 0.3f);
    public Color dirtyColor = new Color(0.3f, 0.25f, 0.1f, 0.7f);

    void Awake()
    {
        Instance = this;
    }

    public void AddPollution(float v)
    {
        currentPollution = Mathf.Clamp(currentPollution + v, 0, maxPollution);
    }

    public void RemovePollution(float v)
    {
        currentPollution = Mathf.Clamp(currentPollution - v, 0, maxPollution);
    }

    public float Ratio => currentPollution / maxPollution;

    void Update()
    {
        if (waterMaterial != null)
        {
            // L'eau devient sale visuellement en fonction du ratio de pollution
            waterMaterial.color = Color.Lerp(cleanColor, dirtyColor, Ratio);
            RenderSettings.fogColor = waterMaterial.color;
        }
    }
}