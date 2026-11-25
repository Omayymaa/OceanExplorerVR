using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BoatDashboard : MonoBehaviour
{
    public TMP_Text text;
    public Image tankBar;
    public int capacity = 30;
    public float nearbyRadius = 12f;
    public int clawMaxNearby = 3;
    public KeyCode clawKey = KeyCode.Alpha1;
    public KeyCode conveyorKey = KeyCode.Alpha2;

    CanvasGroup group;
    int remaining;
    int nearby;
    float nextScan;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        if (text == null) text = GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.textStyle = TMP_Style.NormalStyle;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            text.richText = true;
        }
    }

    void Start()
    {
        MarineStats.Capacity = capacity;
    }

    void Update()
    {
        float target = BoatController.Driving ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, Time.deltaTime * 4f);

        if (!BoatController.Driving) return;

        if (Input.GetKeyDown(clawKey)) MarineStats.Active = CollectSystem.Claw;
        if (Input.GetKeyDown(conveyorKey)) MarineStats.Active = CollectSystem.Conveyor;

        if (Time.time >= nextScan)
        {
            nextScan = Time.time + 0.5f;
            Scan();
        }

        Refresh();
    }

    void Scan()
    {
        remaining = 0;
        nearby = 0;

        BoatController boat = BoatController.Instance;
        Vector3 origin = boat != null ? boat.transform.position : Vector3.zero;

        foreach (WasteItem w in Object.FindObjectsByType<WasteItem>(FindObjectsSortMode.None))
        {
            if (!w.inSea || w.sorted) continue;
            remaining++;

            Vector3 d = w.transform.position - origin;
            d.y = 0f;
            if (d.magnitude <= nearbyRadius) nearby++;
        }
    }

    static string SystemName(CollectSystem s)
    {
        switch (s)
        {
            case CollectSystem.Claw: return "Pince";
            case CollectSystem.Conveyor: return "Convoyeur";
            default: return "Aucun";
        }
    }

    void Refresh()
    {
        BoatController boat = BoatController.Instance;
        int kmh = Mathf.RoundToInt((boat != null ? boat.Speed : 0f) * 3.6f);

        string advice;
        if (nearby == 0) advice = "Aucun déchet à proximité";
        else if (nearby <= clawMaxNearby) advice = "Pince recommandée";
        else advice = "Convoyeur recommandé";

        if (tankBar != null)
            tankBar.fillAmount = MarineStats.Capacity > 0
                ? (float)MarineStats.Collected / MarineStats.Capacity : 0f;

        if (text == null) return;

        text.text =
            "<size=120%><b>Tableau de bord</b></size>\n" +
            "Vitesse   <b>" + kmh + " km/h</b>\n" +
            "Système   <b>" + SystemName(MarineStats.Active) + "</b>   <size=75%>(1 pince, 2 convoyeur)</size>\n" +
            "Collectés   <b>" + MarineStats.Collected + "</b>\n" +
            "Réservoir   <b>" + MarineStats.Collected + " / " + MarineStats.Capacity + "</b>\n" +
            "Restants en mer   <b>" + remaining + "</b>\n" +
            "<size=85%>" + advice + "</size>";
    }
}