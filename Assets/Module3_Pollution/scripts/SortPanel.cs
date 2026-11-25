using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class SortPanel : MonoBehaviour
{
    public static SortPanel Instance;

    public TMP_Text text;
    public Image accent;
    public float showTime = 4f;

    static readonly Color okColor = new Color(0.30f, 0.78f, 0.47f);
    static readonly Color badColor = new Color(0.92f, 0.36f, 0.36f);

    CanvasGroup group;
    float hideAt;

    void Awake()
    {
        Instance = this;
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        if (text == null) text = GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.enableVertexGradient = false;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            text.richText = true;
            text.characterSpacing = 0f;
            text.wordSpacing = 0f;

            Material m = text.fontMaterial;
            m.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
            m.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            m.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
            m.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.5f));
        }
    }

    void Update()
    {
        float target = Time.time < hideAt ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, Time.deltaTime * 4f);
    }

    static string Bin(WasteType t)
    {
        return "<b>" + WasteInfo.Label(t) + "</b>";
    }

    static string Clean(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
    }

    void Show(string s, Color stripe)
    {
        if (text != null) text.text = s;
        if (accent != null) accent.color = stripe;
        hideAt = Time.time + showTime;
    }

    public static void ShowCorrect(string what, WasteType bin, int bagCount, int bagSize)
    {
        if (Instance == null) return;

        Instance.Show(
            "<size=130%><b>Tri correct</b></size>\n" +
            Clean(what) + " dans " + Bin(bin) + "\n" +
            "<size=85%>Sac " + bagCount + " / " + bagSize + "</size>",
            okColor);
    }

    public static void ShowWrong(WasteItem item, WasteType triedBin, int bagCount, int bagSize)
    {
        if (Instance == null || item == null) return;

        Instance.Show(
            "<size=130%><b>Tri faux</b></size>\n" +
            "<b>" + Clean(item.displayName) + "</b> ne va pas dans " + Bin(triedBin) + "\n" +
            "Bonne poubelle : " + Bin(item.type) + "\n" +
            "<size=85%>Sac " + bagCount + " / " + bagSize + "</size>",
            badColor);
    }
}