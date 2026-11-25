using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SortingBin : MonoBehaviour
{
    public WasteType acceptedType;
    public bool autoTypeFromName = true;
    public KeyCode sortKey = KeyCode.F;
    public float reach = 3f;
    public float sideMargin = 0.6f;

    public float signHeightRatio = 0.55f;
    public float signOffset = 0.03f;
    public Vector2 signSize = new Vector2(0.6f, 0.32f);
    public float maxSignGrow = 2.2f;

    public float zoneWidthFactor = 1.1f;
    public float zoneHeight = 1f;
    public float rejectSpeed = 3f;

    public static int Correct;
    public static int Wrong;
    public static SortingBin Current;

    static float currentDist;
    static int stamp = -1;

    PlayerGrab player;
    Camera cam;
    Bounds binBounds;
    Transform signTf;
    readonly Dictionary<WasteItem, float> rejected = new Dictionary<WasteItem, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Correct = 0;
        Wrong = 0;
        Current = null;
        stamp = -1;
    }

    static string ShortLabel(WasteType t)
    {
        switch (t)
        {
            case WasteType.Plastique: return "PLASTIQUE";
            case WasteType.Verre: return "VERRE";
            case WasteType.Metal: return "MÉTAL";
            default: return "PAPIER";
        }
    }

    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerGrab>();
        cam = Camera.main;

        if (autoTypeFromName)
        {
            string n = name.ToLower();
            if (n.Contains("plastique")) acceptedType = WasteType.Plastique;
            else if (n.Contains("verre")) acceptedType = WasteType.Verre;
            else if (n.Contains("metal")) acceptedType = WasteType.Metal;
            else if (n.Contains("papier")) acceptedType = WasteType.Papier;
        }

        if (player == null) Debug.LogWarning("SortingBin : aucun PlayerGrab dans la scène.");

        ComputeBounds();
        BuildSign();
        BuildZone();

        Debug.Log("Poubelle prête : " + name + " | type " + acceptedType + " | taille " + binBounds.size);
    }

    void ComputeBounds()
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();

        if (rs.Length == 0)
        {
            binBounds = new Bounds(transform.position + Vector3.up * 0.5f, new Vector3(0.8f, 1f, 0.8f));
            return;
        }

        binBounds = rs[0].bounds;
        foreach (Renderer r in rs) binBounds.Encapsulate(r.bounds);
    }

    void BuildSign()
    {
        Color c = WasteInfo.GetColor(acceptedType);

        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(sign.GetComponent<Collider>());
        sign.name = "Ticket";
        sign.transform.SetParent(transform, true);
        sign.transform.localScale = new Vector3(signSize.x, signSize.y, 1f);
        sign.GetComponent<Renderer>().material.color = c;
        signTf = sign.transform;

        GameObject tg = new GameObject("Mot");
        tg.transform.SetParent(sign.transform, false);
        tg.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        tg.transform.localRotation = Quaternion.identity;
        tg.transform.localScale = Vector3.one;

        TextMeshPro t = tg.AddComponent<TextMeshPro>();
        t.text = ShortLabel(acceptedType);
        t.alignment = TextAlignmentOptions.Center;
        t.enableAutoSizing = true;
        t.fontSizeMin = 0.3f;
        t.fontSizeMax = 1.6f;
        t.fontStyle = FontStyles.Bold;
        t.rectTransform.sizeDelta = new Vector2(0.95f, 0.8f);

        float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        t.color = lum > 0.55f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
    }

    void LateUpdate()
    {
        if (signTf == null) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 toCam = cam.transform.position - binBounds.center;
        float dist = new Vector2(toCam.x, toCam.z).magnitude;
        toCam.y = 0f;
        if (toCam.sqrMagnitude < 0.0001f) return;
        toCam.Normalize();

        float tx = Mathf.Abs(toCam.x) > 0.0001f ? binBounds.extents.x / Mathf.Abs(toCam.x) : float.MaxValue;
        float tz = Mathf.Abs(toCam.z) > 0.0001f ? binBounds.extents.z / Mathf.Abs(toCam.z) : float.MaxValue;
        float t = Mathf.Min(tx, tz);

        Vector3 pos = binBounds.center + toCam * (t + signOffset);
        pos.y = binBounds.min.y + binBounds.size.y * signHeightRatio;

        signTf.position = pos;
        signTf.rotation = Quaternion.LookRotation(-toCam, Vector3.up);

        float grow = Mathf.Clamp(dist / 4f, 1f, maxSignGrow);
        signTf.localScale = new Vector3(signSize.x * grow, signSize.y * grow, 1f);
    }

    void BuildZone()
    {
        GameObject z = new GameObject("ZoneDepot");
        z.transform.SetParent(transform, true);
        z.transform.position = new Vector3(binBounds.center.x, binBounds.max.y, binBounds.center.z);
        z.transform.rotation = Quaternion.identity;

        Vector3 s = z.transform.lossyScale;
        BoxCollider col = z.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(
            binBounds.size.x * zoneWidthFactor / Mathf.Max(0.0001f, s.x),
            zoneHeight / Mathf.Max(0.0001f, s.y),
            binBounds.size.z * zoneWidthFactor / Mathf.Max(0.0001f, s.z));

        BinTrigger bt = z.AddComponent<BinTrigger>();
        bt.bin = this;
    }

    bool IsFacing(out float dist)
    {
        dist = 0f;

        Bounds b = binBounds;
        b.Expand(new Vector3(sideMargin * 2f, 0f, sideMargin * 2f));

        Vector3 o = cam.transform.position;
        o.y = b.center.y;

        Vector3 d = cam.transform.forward;
        d.y = 0f;
        if (d.sqrMagnitude < 0.001f) return false;
        d.Normalize();

        float t;
        if (!b.IntersectRay(new Ray(o, d), out t)) return false;

        dist = t;
        return t <= reach;
    }

    void Update()
    {
        if (player == null || cam == null) return;

        float d = 0f;
        bool facing = player.Bag.Count > 0 && IsFacing(out d);

        if (facing)
        {
            if (stamp != Time.frameCount)
            {
                stamp = Time.frameCount;
                Current = this;
                currentDist = d;
            }
            else if (d < currentDist)
            {
                Current = this;
                currentDist = d;
            }
        }
        else if (Current == this)
        {
            Current = null;
        }

        if (facing && Current == this && Input.GetKeyDown(sortKey))
            Deposit();
    }

    void Deposit()
    {
        Debug.Log("Deposit appelé sur " + name + " | sac " + player.Bag.Count);

        List<WasteItem> good = new List<WasteItem>();
        foreach (WasteItem w in player.Bag)
            if (w.type == acceptedType) good.Add(w);

        if (good.Count == 0)
        {
            Wrong++;
            SoundFX.Wrong();
            SortPanel.ShowWrong(player.Held, acceptedType, player.Bag.Count, player.BagSize);
            return;
        }

        string what = good.Count == 1
            ? "<b>" + good[0].displayName + "</b>"
            : "<b>" + good.Count + " déchets</b>";

        foreach (WasteItem w in good)
        {
            w.sorted = true;
            player.RemoveFromBag(w);
            Destroy(w.gameObject);
            Correct++;
        }

        SoundFX.Correct();
        SortPanel.ShowCorrect(what, acceptedType, player.Bag.Count, player.BagSize);
    }

    public void OnItemEntered(WasteItem w)
    {
        if (w == null || w.sorted) return;

        Debug.Log("Déchet entré dans " + name + " : " + w.displayName + " (" + w.type + ")");

        float until;
        if (rejected.TryGetValue(w, out until) && Time.time < until) return;

        SoundFX.BinHit();

        int bagCount = player != null ? player.Bag.Count : 0;
        int bagSize = player != null ? player.BagSize : 0;

        if (w.type == acceptedType)
        {
            w.sorted = true;
            Correct++;
            SoundFX.Correct();
            SortPanel.ShowCorrect("<b>" + w.displayName + "</b>", acceptedType, bagCount, bagSize);
            Destroy(w.gameObject);
        }
        else
        {
            Wrong++;
            SoundFX.Wrong();
            rejected[w] = Time.time + 1.2f;

            Rigidbody rb = w.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 away = w.transform.position - binBounds.center;
                away.y = 0f;
                if (away.sqrMagnitude < 0.01f) away = Vector3.back;
                away.Normalize();
                rb.linearVelocity = away * rejectSpeed + Vector3.up * (rejectSpeed + 0.5f);
            }

            SortPanel.ShowWrong(w, acceptedType, bagCount, bagSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Bounds b = Application.isPlaying ? binBounds : new Bounds(transform.position + Vector3.up * 0.5f, Vector3.one);
        Gizmos.DrawWireCube(b.center, b.size);
    }
}