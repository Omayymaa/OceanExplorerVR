using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BeachProgress : MonoBehaviour
{
    [Range(0.1f, 1f)] public float requiredRatio = 0.8f;
    public TMP_Text message;
    public float showSeconds = 8f;
    public float fadeSeconds = 1.5f;
    public UnityEvent onBeachClean;

    public static int Total;
    public static int Needed;
    public static bool Clean;

    bool measured;
    float shownAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Total = 0;
        Needed = 0;
        Clean = false;
    }

    void Start()
    {
        if (message != null)
        {
            message.textStyle = TMP_Style.NormalStyle;
            message.color = Color.white;
            message.richText = true;
            message.raycastTarget = false;
            message.alignment = TextAlignmentOptions.Center;
            message.text = "";
        }

        Invoke(nameof(Measure), 1.5f);
    }

    void Measure()
    {
        Total = 0;
        foreach (WasteItem w in Object.FindObjectsByType<WasteItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!w.inSea) Total++;

        Needed = Mathf.Max(1, Mathf.CeilToInt(Total * requiredRatio));
        measured = true;
    }

    void Update()
    {
        if (!measured) return;

        if (!Clean)
        {
            if (SortingBin.Correct >= Needed)
            {
                Clean = true;
                shownAt = Time.time;

                if (message != null)
                    message.text = "<b>Plage propre !</b>  Objectif suivant : nettoyer la mer";

                onBeachClean.Invoke();
            }
            return;
        }

        if (message == null || showSeconds <= 0f) return;

        float t = Time.time - shownAt;
        if (t > showSeconds)
        {
            float k = Mathf.Clamp01((t - showSeconds) / Mathf.Max(0.01f, fadeSeconds));
            message.alpha = 1f - k;
        }
    }
}