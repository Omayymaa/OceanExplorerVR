using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(300)]
public class CraneGrapple : MonoBehaviour
{
    [Header("Pince (ton modèle 3D)")]
    public GameObject grapplePrefab;
    public float grappleScale = 0.12f;
    public Vector3 grappleEuler = Vector3.zero;
    public Vector3 grappleOffset = Vector3.zero;
    [Range(0f, 0.5f)] public float squeeze = 0.2f;

    [Header("Grue (repère local du bateau)")]
    public Vector2 mastXZ = new Vector2(0f, 0.3f);
    public Vector3 baseLocal = new Vector3(0f, 1.9f, 1.0f);

    [Header("Pont du bateau (là où tombent les déchets)")]
    public Vector3 deckCenter = new Vector3(0f, 0.3f, 0.2f);

    [Header("Commandes")]
    public KeyCode deployKey = KeyCode.Alpha1;
    public KeyCode grabKey = KeyCode.E;
    public float deployTime = 1.6f;

    [Header("Saisie automatique")]
    public float autoRange = 2f;
    public int maxPerGrab = 4;
    public float grabRadius = 1.6f;
    public float seaLevel = 0.15f;

    [Header("Mouvements")]
    public float hoverDrop = 1.5f;
    public float grabHeight = 0.25f;
    public float speed = 7f;
    public float verticalSpeed = 3f;

    Transform head, model, mastT, armT, pulleyT;
    GameObject craneRoot;
    Vector3 modelBaseScale;
    LineRenderer cable;

    bool busy, wanted;
    float deployK;

    void Start()
    {
        BuildVisuals();
        SetClose(0f);
        ApplyDeploy();
    }

    Vector3 BaseWorld() { return transform.TransformPoint(baseLocal); }
    Vector3 DeckWorld() { return transform.TransformPoint(deckCenter); }
    Vector3 HoverPos(Vector3 xz) { return new Vector3(xz.x, BaseWorld().y - hoverDrop, xz.z); }
    Vector3 RestWorld() { return HoverPos(BaseWorld()); }

    Transform Piece(PrimitiveType type, string n, Vector3 scale, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        Destroy(g.GetComponent<Collider>());
        g.name = n;
        g.transform.SetParent(craneRoot.transform, false);
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().material.color = c;
        return g.transform;
    }

    void BuildVisuals()
    {
        Color steel = new Color(0.3f, 0.33f, 0.37f);

        craneRoot = new GameObject("Grue");
        craneRoot.transform.SetParent(transform, false);

        mastT = Piece(PrimitiveType.Cylinder, "MatGrue", new Vector3(0.1f, 0.01f, 0.1f), steel);
        armT = Piece(PrimitiveType.Cylinder, "BrasGrue", new Vector3(0.08f, 0.01f, 0.08f), steel);
        pulleyT = Piece(PrimitiveType.Sphere, "PoulieGrue", Vector3.one * 0.22f, steel);

        head = new GameObject("TetePince").transform;
        head.position = RestWorld();

        GameObject m;
        if (grapplePrefab != null)
        {
            m = Instantiate(grapplePrefab);
            foreach (Collider c in m.GetComponentsInChildren<Collider>(true)) Destroy(c);
            foreach (Camera cm in m.GetComponentsInChildren<Camera>(true)) Destroy(cm);
            foreach (AudioListener al in m.GetComponentsInChildren<AudioListener>(true)) Destroy(al);
            foreach (Light lt in m.GetComponentsInChildren<Light>(true)) Destroy(lt);
        }
        else
        {
            m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(m.GetComponent<Collider>());
            m.GetComponent<Renderer>().material.color = new Color(0.95f, 0.75f, 0.1f);
        }

        m.name = "ModelePince";
        m.transform.SetParent(head, false);
        m.transform.localPosition = grappleOffset;
        m.transform.localRotation = Quaternion.Euler(grappleEuler);
        m.transform.localScale = Vector3.one * grappleScale;
        model = m.transform;
        modelBaseScale = m.transform.localScale;

        GameObject cb = new GameObject("CableGrue");
        cable = cb.AddComponent<LineRenderer>();
        cable.useWorldSpace = true;
        cable.positionCount = 2;
        cable.startWidth = 0.015f;
        cable.endWidth = 0.015f;
        cable.material = new Material(Shader.Find("Sprites/Default"));
        cable.startColor = new Color(0.1f, 0.1f, 0.1f);
        cable.endColor = new Color(0.1f, 0.1f, 0.1f);
        cable.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void SetClose(float k)
    {
        float s = 1f - squeeze * k;
        model.localScale = new Vector3(modelBaseScale.x * s, modelBaseScale.y, modelBaseScale.z * s);
    }

    IEnumerator Close(float from, float to, float time)
    {
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            SetClose(Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        SetClose(to);
    }

    IEnumerator MoveTo(System.Func<Vector3> dest, float spd)
    {
        while (true)
        {
            Vector3 d = dest();
            if (Vector3.Distance(head.position, d) < 0.05f)
            {
                head.position = d;
                yield break;
            }
            head.position = Vector3.MoveTowards(head.position, d, spd * Time.deltaTime);
            yield return null;
        }
    }

    int FreeSpace()
    {
        return Mathf.Max(0, MarineStats.Capacity - MarineStats.Collected);
    }

    List<WasteItem> Nearby(Vector3 p, float r, int max)
    {
        List<WasteItem> list = new List<WasteItem>();

        foreach (WasteItem w in FindObjectsByType<WasteItem>(FindObjectsSortMode.None))
        {
            if (!w.inSea || w.sorted) continue;

            Vector3 d = w.transform.position - p;
            d.y = 0f;
            if (d.magnitude <= r) list.Add(w);
        }

        list.Sort((a, b) =>
        {
            Vector3 da = a.transform.position - p; da.y = 0f;
            Vector3 db = b.transform.position - p; db.y = 0f;
            return da.sqrMagnitude.CompareTo(db.sqrMagnitude);
        });

        if (list.Count > max) list.RemoveRange(max, list.Count - max);
        return list;
    }

    bool Ready()
    {
        return BoatController.Driving && MarineStats.Active == CollectSystem.Claw && deployK >= 0.99f;
    }

    void Update()
    {
        if (!BoatController.Driving)
        {
            wanted = false;
            deployK = 0f;
            return;
        }

        if (Input.GetKeyDown(deployKey) && !busy)
        {
            wanted = !wanted;
            MarineStats.Active = wanted ? CollectSystem.Claw : CollectSystem.None;
        }

        if (MarineStats.Active != CollectSystem.Claw && !busy) wanted = false;

        float before = deployK;
        deployK = Mathf.MoveTowards(deployK, wanted ? 1f : 0f, Time.deltaTime / Mathf.Max(0.1f, deployTime));

        if (busy) return;

        List<WasteItem> near = Nearby(BaseWorld(), autoRange, Mathf.Min(maxPerGrab, FreeSpace()));

        if (!Ready())
        {
            if (!wanted && near.Count > 0 && !MarineStats.Full)
                HUDMessage.ShowTimed("<b>1</b> : sortir la pince  (" + near.Count + " déchet(s) à proximité)", 0.15f);
            return;
        }

        if (before < 0.99f && deployK >= 0.99f && near.Count == 0)
            HUDMessage.ShowTimed("Pince prête : approche-toi des déchets (moins de " + autoRange + " m)", 2.5f);

        if (MarineStats.Full)
        {
            HUDMessage.ShowTimed("Bateau plein", 0.15f);
            return;
        }

        if (near.Count > 0)
            HUDMessage.ShowTimed("<b>E</b> : saisir " + (near.Count == 1
                ? "<b>" + near[0].displayName + "</b>"
                : "<b>" + near.Count + " déchets</b>"), 0.15f);

        if (Input.GetKeyDown(grabKey))
        {
            if (near.Count > 0) StartCoroutine(Collect(near[0].transform.position));
            else HUDMessage.ShowTimed("Aucun déchet à moins de " + autoRange + " m", 1.5f);
        }
    }

    IEnumerator Collect(Vector3 target)
    {
        busy = true;
        SetClose(0f);

        yield return MoveTo(() => HoverPos(target), speed);
        yield return MoveTo(() => new Vector3(target.x, seaLevel + grabHeight, target.z), verticalSpeed);
        yield return Close(0f, 1f, 0.3f);

        int take = Mathf.Min(maxPerGrab, FreeSpace());
        List<WasteItem> held = Nearby(head.position, grabRadius, take);

        for (int i = 0; i < held.Count; i++)
        {
            WasteItem w = held[i];
            if (w == null) continue;

            FloatBob fb = w.GetComponent<FloatBob>();
            if (fb != null) fb.enabled = false;
            foreach (Collider c in w.GetComponentsInChildren<Collider>()) c.enabled = false;

            float a = i * Mathf.PI * 0.5f;
            Vector3 off = new Vector3(Mathf.Cos(a) * 0.18f, -0.25f - 0.05f * i, Mathf.Sin(a) * 0.18f);

            w.transform.position = head.position + off;
            w.transform.SetParent(head, true);
        }

        yield return MoveTo(() => HoverPos(head.position), verticalSpeed);

        if (held.Count > 0)
        {
            Vector3 above = DeckWorld() + Vector3.up * 0.9f;
            yield return MoveTo(() => above, speed);
            yield return Close(1f, 0f, 0.25f);

            List<WasteItem> dropped = new List<WasteItem>();
            foreach (WasteItem w in held)
            {
                if (w == null) continue;
                w.transform.SetParent(null, true);
                if (MarineStats.Register(w)) dropped.Add(w);
            }

            Vector3 floor = DeckWorld();
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                foreach (WasteItem w in dropped)
                {
                    if (w == null) continue;
                    w.transform.position = Vector3.Lerp(w.transform.position, floor, k);
                    w.transform.localScale *= 1f - Time.deltaTime * 2.5f;
                }
                yield return null;
            }

            int count = dropped.Count;
            foreach (WasteItem w in dropped)
                if (w != null) Destroy(w.gameObject);

            SoundFX.Correct();
            if (count > 0)
                HUDMessage.ShowTimed("<b>+" + count + "</b> dans le réservoir  (" +
                    MarineStats.Collected + " / " + MarineStats.Capacity + ")", 2.5f);
        }
        else
        {
            HUDMessage.ShowTimed("Rien à saisir ici", 1.5f);
            yield return Close(1f, 0f, 0.2f);
        }

        yield return MoveTo(() => RestWorld(), speed);
        busy = false;
    }

    void ApplyDeploy()
    {
        float mastK = Mathf.Clamp01(deployK / 0.5f);
        float armK = Mathf.Clamp01((deployK - 0.4f) / 0.3f);

        bool visible = deployK > 0.01f;
        if (craneRoot.activeSelf != visible) craneRoot.SetActive(visible);
        if (!visible) return;

        float h = Mathf.Max(0.001f, baseLocal.y * mastK);
        mastT.localPosition = new Vector3(mastXZ.x, h * 0.5f, mastXZ.y);
        mastT.localScale = new Vector3(0.1f, h * 0.5f, 0.1f);

        Vector3 a0 = new Vector3(mastXZ.x, baseLocal.y, mastXZ.y);
        Vector3 a1 = Vector3.Lerp(a0, baseLocal, armK);
        Vector3 d = a1 - a0;
        bool armShow = armK > 0.01f && d.sqrMagnitude > 0.0001f;

        armT.gameObject.SetActive(armShow);
        if (armShow)
        {
            armT.localPosition = (a0 + a1) * 0.5f;
            armT.localRotation = Quaternion.FromToRotation(Vector3.up, d);
            armT.localScale = new Vector3(0.08f, d.magnitude * 0.5f, 0.08f);
        }

        pulleyT.gameObject.SetActive(mastK >= 0.99f);
        pulleyT.localPosition = armShow ? a1 : a0;
    }

    void LateUpdate()
    {
        if (head == null) return;

        ApplyDeploy();

        float dropK = Mathf.Clamp01((deployK - 0.7f) / 0.3f);
        bool headShow = dropK > 0.01f || busy;

        if (head.gameObject.activeSelf != headShow) head.gameObject.SetActive(headShow);
        cable.enabled = headShow;
        if (!headShow) return;

        if (!busy)
        {
            head.position = Vector3.Lerp(pulleyT.position, RestWorld(), dropK);
            SetClose(0f);
        }

        head.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        cable.SetPosition(0, pulleyT.position);
        cable.SetPosition(1, head.position + Vector3.up * 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(baseLocal), 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(deckCenter), 0.25f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(BaseWorld(), autoRange);
    }
}