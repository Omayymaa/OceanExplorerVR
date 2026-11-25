using UnityEngine;
using TMPro;

public class HUDMessage : MonoBehaviour
{
    public static HUDMessage Instance;

    public TMP_Text text;

    string baseMsg = "";
    string timedMsg = "";
    float timedUntil;

    void Awake()
    {
        Instance = this;
        if (text == null) text = GetComponent<TMP_Text>();

        if (text != null)
        {
            text.raycastTarget = false;
            text.outlineWidth = 0.25f;
            text.outlineColor = new Color32(0, 0, 0, 255);
        }
    }

    public static void SetBase(string s)
    {
        if (Instance != null) Instance.baseMsg = s;
    }

    public static void ShowTimed(string s, float duration)
    {
        if (Instance == null) return;
        Instance.timedMsg = s;
        Instance.timedUntil = Time.time + duration;
    }

    void Update()
    {
        string m = Time.time < timedUntil ? timedMsg : baseMsg;
        if (text != null && text.text != m) text.text = m;
    }
}