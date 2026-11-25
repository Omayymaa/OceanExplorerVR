using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class HandClaw : MonoBehaviour
{
    [Header("Perche")]
    public float handleBack = 0.35f;
    public float poleThickness = 0.04f;
    public Color poleColor = new Color(0.9f, 0.9f, 0.92f);
    public Vector3 restLocalDir = new Vector3(0.12f, -0.4f, 1f);
    public float restLength = 0.9f;
    public float maxLength = 5.5f;

    [Header("Mâchoires")]
    public Color jawColor = new Color(0.95f, 0.75f, 0.1f);
    public float jawOpenAngle = 28f;
    public float jawClosedAngle = 4f;

    [Header("Bac de dépôt (repère local du bateau)")]
    public Vector3 tankLocal = new Vector3(0.4f, 0.4f, -0.5f);
    public bool buildTankVisual = true;

    [Header("Saisie")]
    public float grabRadius = 1f;
    public float seaLevel = 0.15f;
    public float extendTime = 0.45f;
    public float retractTime = 0.35f;
    public float swingTime = 0.5f;

    Transform hand, pole, tongs, jawL, jawR, marker;
    Renderer markerRenderer;
    Camera cam;
    GameObject visuals;

    Vector3 curDir = Vector3.forward;
    float curLen;
    bool busy;
    bool hasTarget;
    bool inReach;
    Vector3 targetPoint;
    WasteItem targetWaste;

    void Start()
    {
        cam = Camera.main;

        ArmIK arm = Object.FindAnyObjectByType<ArmIK>();
        Animator anim = arm != null ? arm.animator : null;
        if (anim != null && anim.isHuman)
            hand = anim.GetBoneTransform(HumanBodyBones.RightHand);

        if (hand == null)
            Debug.LogWarning("HandClaw : main droite introuvable (Avatar Humanoïde ?).");

        BuildVisuals();
        BuildTankVisual();

        curLen = restLength;
    }

    Vector3 RestDir()
    {
        return cam.transform.TransformDirection(restLocalDir.normalized);
    }

    Vector3 TankWorld()
    {
        BoatController b = BoatController.Instance;
        return b != null ? b.transform.TransformPoint(tankLocal) : hand.position + Vector3.down;
    }

    void BuildVisuals()
    {
        visuals = new GameObject("PinceMain");

        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(p.GetComponent<Collider>());
        p.name = "Perche";
        p.transform.SetParent(visuals.transform, false);
        p.GetComponent<Renderer>().material.color = poleColor;
        pole = p.transform;

        GameObject t = new GameObject("Pince");
        t.transform.SetParent(visuals.transform, false);
        tongs = t.transform;

        jawL = MakeJaw("MachoireG", -0.06f);
        jawR = MakeJaw("MachoireD", 0.06f);

        GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(m.GetComponent<Collider>());
        m.name = "AnneauVisee";
        marker = m.transform;
        marker.localScale = new Vector3(grabRadius * 2f, 0.01f, grabRadius * 2f);
        markerRenderer = m.GetComponent<Renderer>();
        markerRenderer.material = new Material(Shader.Find("Sprites/Default"));
        markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m.SetActive(false);

        SetClose(0f);
        visuals.SetActive(false);
    }

    Transform MakeJaw(string n, float x)
    {
        GameObject pivot = new GameObject(n);
        pivot.transform.SetParent(tongs, false);
        pivot.transform.localPosition = new Vector3(x, 0f, 0f);

        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(blade.GetComponent<Collider>());
        blade.transform.SetParent(pivot.transform, false);
        blade.transform.localPosition = new Vector3(0f, -0.11f, 0f);
        blade.transform.localScale = new Vector3(0.03f, 0.22f, 0.06f);
        blade.GetComponent<Renderer>().material.color = jawColor;

        return pivot.transform;
    }

    void BuildTankVisual()
    {
        if (!buildTankVisual) return;

        BoatController b = BoatController.Instance;
        if (b == null) return;

        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(c.GetComponent<Collider>());
        c.name = "BacCollecte";
        c.transform.SetParent(b.transform, false);
        c.transform.localPosition = tankLocal + Vector3.down * 0.25f;
        c.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
        c.GetComponent<Renderer>().material.color = new Color(0.25f, 0.45f, 0.7f);
    }

    void SetClose(float k)
    {
        float a = Mathf.Lerp(jawOpenAngle, jawClosedAngle, k);
        jawL.localRotation = Quaternion.Euler(0f, 0f, a);
        jawR.localRotation = Quaternion.Euler(0f, 0f, -a);
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

    IEnumerator GoTo(System.Func<Vector3> dirF, System.Func<float> lenF, float time)
    {
        Vector3 d0 = curDir;
        float l0 = curLen;

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / time);
            curDir = Vector3.Slerp(d0, dirF(), k).normalized;
            curLen = Mathf.Lerp(l0, lenF(), k);
            yield return null;
        }

        curDir = dirF();
        curLen = lenF();
    }

    WasteItem NearestWaste(Vector3 p, float r)
    {
        WasteItem best = null;
        float bestD = r;

        foreach (WasteItem w in FindObjectsByType<WasteItem>(FindObjectsSortMode.None))
        {
            if (!w.inSea || w.sorted) continue;

            Vector3 d = w.transform.position - p;
            d.y = 0f;
            float m = d.magnitude;

            if (m < bestD)
            {
                bestD = m;
                best = w;
            }
        }
        return best;
    }

    bool Active()
    {
        return BoatController.Driving && MarineStats.Active == CollectSystem.Claw;
    }

    void Update()
    {
        if (hand == null || cam == null) return;

        if (!Active() || busy)
        {
            if (marker != null) marker.gameObject.SetActive(false);
            return;
        }

        Aim();
        if (!hasTarget) return;

        if (MarineStats.Full)
            HUDMessage.ShowTimed("Réservoir plein", 0.15f);
        else if (!inReach)
            HUDMessage.ShowTimed("Trop loin pour la pince", 0.15f);
        else if (targetWaste != null)
            HUDMessage.ShowTimed("Saisir <b>" + targetWaste.displayName + "</b>", 0.15f);

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.Locked
            && inReach && !MarineStats.Full)
        {
            StartCoroutine(Collect(targetPoint));
        }
    }

    void Aim()
    {
        Plane sea = new Plane(Vector3.up, new Vector3(0f, seaLevel, 0f));
        Ray r = new Ray(cam.transform.position, cam.transform.forward);

        float t;
        hasTarget = sea.Raycast(r, out t) && t < 60f;
        marker.gameObject.SetActive(hasTarget);
        if (!hasTarget) return;

        targetPoint = r.GetPoint(t);
        inReach = Vector3.Distance(hand.position, targetPoint) <= maxLength;
        targetWaste = NearestWaste(targetPoint, grabRadius);

        marker.position = new Vector3(targetPoint.x, seaLevel + 0.03f, targetPoint.z);

        Color col;
        if (!inReach) col = new Color(1f, 0.3f, 0.3f, 0.45f);
        else if (targetWaste != null) col = new Color(0.3f, 1f, 0.5f, 0.6f);
        else col = new Color(1f, 1f, 1f, 0.3f);

        markerRenderer.material.color = col;
    }

    IEnumerator Collect(Vector3 target)
    {
        busy = true;
        marker.gameObject.SetActive(false);

        Vector3 tp = new Vector3(target.x, seaLevel + 0.05f, target.z);

        yield return GoTo(
            () => (tp - hand.position).normalized,
            () => Mathf.Max(0.5f, Vector3.Distance(hand.position, tp)),
            extendTime);

        yield return Close(0f, 1f, 0.25f);

        Vector3 tipPos = hand.position + curDir * curLen;
        WasteItem w = NearestWaste(tipPos, grabRadius);

        if (w != null)
        {
            FloatBob fb = w.GetComponent<FloatBob>();
            if (fb != null) fb.enabled = false;

            foreach (Collider c in w.GetComponentsInChildren<Collider>()) c.enabled = false;

            w.transform.position = tipPos;
            w.transform.SetParent(tongs, true);
        }

        yield return GoTo(RestDir, () => restLength, retractTime);

        if (w != null)
        {
            Vector3 tank = TankWorld();

            yield return GoTo(
                () => (tank - hand.position).normalized,
                () => Mathf.Max(0.4f, Vector3.Distance(hand.position, tank)),
                swingTime);

            yield return Close(1f, 0f, 0.2f);

            if (w != null)
            {
                w.transform.SetParent(null, true);
                MarineStats.TryCollect(w);
                SoundFX.Correct();
            }

            yield return GoTo(RestDir, () => restLength, retractTime);
        }
        else
        {
            HUDMessage.ShowTimed("Rien à saisir ici", 1.5f);
            yield return Close(1f, 0f, 0.2f);
        }

        busy = false;
    }

    void LateUpdate()
    {
        if (visuals == null || hand == null || cam == null) return;

        bool show = Active() || busy;
        if (visuals.activeSelf != show) visuals.SetActive(show);
        if (!show) return;

        if (!busy)
        {
            curDir = RestDir();
            curLen = restLength;
            SetClose(0f);
        }

        Vector3 basePos = hand.position - curDir * handleBack;
        float total = curLen + handleBack;

        pole.position = basePos + curDir * total * 0.5f;
        pole.rotation = Quaternion.FromToRotation(Vector3.up, curDir);
        pole.localScale = new Vector3(poleThickness, total * 0.5f, poleThickness);

        tongs.position = hand.position + curDir * curLen;
        tongs.rotation = Quaternion.FromToRotation(Vector3.down, curDir);
    }

    void OnDrawGizmosSelected()
    {
        BoatController b = Object.FindAnyObjectByType<BoatController>();
        if (b == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(b.transform.TransformPoint(tankLocal), 0.25f);
    }
}