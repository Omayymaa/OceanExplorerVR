using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    [Header("Références")]
    public Camera cam;
    public ArmIK arm;

    [Header("Touches")]
    public KeyCode grabKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.G;

    [Header("Ramassage")]
    public int bagSize = 6;
    public float reach = 2.5f;
    public float reachTime = 0.25f;

    [Header("Objet tenu")]
    public Vector3 handOffset = new Vector3(0f, 0.05f, 0f);
    public Vector3 heldEuler = Vector3.zero;
    public float heldScale = 0.6f;

    [Header("Lancer")]
    public float throwSpeed = 9f;
    public float upBoost = 1.5f;
    public float dropForce = 2f;

    Transform hand;
    CharacterController controller;
    LineRenderer arc;
    readonly List<WasteItem> bag = new List<WasteItem>();
    readonly Dictionary<WasteItem, Vector3> originalScale = new Dictionary<WasteItem, Vector3>();
    WasteItem aimed;
    bool busy;
    bool wasDriving;

    public IReadOnlyList<WasteItem> Bag { get { return bag; } }
    public int BagSize { get { return bagSize; } }
    public WasteItem Held { get { return bag.Count > 0 ? bag[bag.Count - 1] : null; } }

    void Start()
    {
        if (cam == null) cam = Camera.main;
        controller = GetComponent<CharacterController>();

        Animator anim = arm != null ? arm.animator : GetComponentInChildren<Animator>();
        if (anim != null && anim.isHuman)
            hand = anim.GetBoneTransform(HumanBodyBones.RightHand);

        if (hand == null)
            Debug.LogWarning("PlayerGrab : main droite introuvable. Avatar Humanoïde ?");

        if (arm != null)
        {
            arm.goal = arm.carryPoint;
            arm.active = true;
        }

        BuildArc();
        HUDMessage.SetBase("");
    }

    void BuildArc()
    {
        GameObject g = new GameObject("ArcVisee");
        arc = g.AddComponent<LineRenderer>();
        arc.useWorldSpace = true;
        arc.startWidth = 0.035f;
        arc.endWidth = 0.035f;
        arc.material = new Material(Shader.Find("Sprites/Default"));
        arc.startColor = new Color(1f, 1f, 1f, 0.9f);
        arc.endColor = new Color(1f, 1f, 1f, 0.2f);
        arc.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        arc.enabled = false;
    }

    Vector3 ThrowOrigin()
    {
        return cam.transform.position + cam.transform.forward * 0.6f + Vector3.down * 0.2f;
    }

    Vector3 ThrowVelocity()
    {
        return cam.transform.forward * throwSpeed + Vector3.up * upBoost;
    }

    void Update()
    {
        if (BoatController.Driving)
        {
            wasDriving = true;

            if (arm != null) arm.active = false;
            HUDMessage.SetBase("");
            if (arc != null) arc.enabled = false;
            return;
        }

        if (wasDriving)
        {
            wasDriving = false;

            if (arm != null)
            {
                arm.goal = arm.carryPoint;
                arm.active = true;
            }
        }

        if (busy || hand == null)
        {
            if (arc != null) arc.enabled = false;
            return;
        }

        bool locked = Cursor.lockState == CursorLockMode.Locked;

        aimed = FindAimed();

        string sac = bag.Count > 0
            ? "<size=80%>Sac " + bag.Count + "/" + bagSize + "</size>"
            : "";

        if (SortingBin.Current != null)
        {
            HUDMessage.SetBase("Trier dans <b>" +
                WasteInfo.Label(SortingBin.Current.acceptedType) + "</b>\n" + sac);
        }
        else if (aimed != null && bag.Count < bagSize)
            HUDMessage.SetBase("Ramasser <b>" + aimed.displayName + "</b>\n" + sac);
        else if (aimed != null)
            HUDMessage.SetBase("Sac plein\n" + sac);
        else
            HUDMessage.SetBase(sac);

        UpdateArc(locked && bag.Count > 0 && Input.GetMouseButton(1));

        if (Input.GetKeyDown(grabKey) && aimed != null && bag.Count < bagSize)
            StartCoroutine(Grab(aimed));

        if (locked && Input.GetMouseButtonDown(0) && bag.Count > 0)
            Throw();

        if (Input.GetKeyDown(dropKey) && bag.Count > 0)
            Drop();
    }

    void UpdateArc(bool show)
    {
        if (arc == null) return;

        arc.enabled = show;
        if (!show) return;

        List<Vector3> pts = new List<Vector3>();
        Vector3 p = ThrowOrigin();
        Vector3 v = ThrowVelocity();
        pts.Add(p);

        for (int i = 0; i < 40; i++)
        {
            Vector3 next = p + v * 0.05f;
            v += Physics.gravity * 0.05f;

            RaycastHit hit;
            if (Physics.Linecast(p, next, out hit, ~0, QueryTriggerInteraction.Ignore))
            {
                pts.Add(hit.point);
                break;
            }

            pts.Add(next);
            p = next;
        }

        arc.positionCount = pts.Count;
        arc.SetPositions(pts.ToArray());
    }

    WasteItem FindAimed()
    {
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0f;
        fwd.Normalize();

        WasteItem best = null;
        float bestDist = reach;

        foreach (WasteItem w in FindObjectsByType<WasteItem>(FindObjectsSortMode.None))
        {
            if (w.sorted || bag.Contains(w)) continue;
            if (w.GetComponent<FloatBob>() != null) continue;

            Vector3 to = w.transform.position - transform.position;
            to.y = 0f;
            float d = to.magnitude;

            if (d > bestDist) continue;
            if (d > 0.5f && Vector3.Dot(fwd, to.normalized) < 0.5f) continue;

            bestDist = d;
            best = w;
        }
        return best;
    }

    IEnumerator Grab(WasteItem target)
    {
        busy = true;
        if (arm != null && arm.animator != null)
            arm.animator.SetTrigger("Grab");

        arm.goal = target.transform;
        arm.active = true;
        yield return new WaitForSeconds(reachTime);

        if (target != null)
        {
            WasteItem previous = Held;
            if (previous != null) previous.gameObject.SetActive(false);

            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            foreach (Collider c in target.GetComponentsInChildren<Collider>()) c.enabled = false;

            target.transform.SetParent(hand, true);
            ShrinkInHand(target);
            target.transform.position = hand.TransformPoint(handOffset);
            target.transform.localRotation = Quaternion.Euler(heldEuler);
            bag.Add(target);
        }

        arm.goal = arm.carryPoint;
        busy = false;
    }

    void ShrinkInHand(WasteItem w)
    {
        Vector3 world = w.transform.lossyScale;
        originalScale[w] = world;

        Vector3 target = world * heldScale;
        Vector3 p = w.transform.parent != null ? w.transform.parent.lossyScale : Vector3.one;

        w.transform.localScale = new Vector3(
            target.x / Mathf.Max(0.0001f, p.x),
            target.y / Mathf.Max(0.0001f, p.y),
            target.z / Mathf.Max(0.0001f, p.z));
    }

    void RestoreSize(WasteItem w)
    {
        Vector3 s;
        if (originalScale.TryGetValue(w, out s))
        {
            w.transform.localScale = s;
            originalScale.Remove(w);
        }
    }

    void Throw()
    {
        WasteItem w = Held;
        RemoveFromBag(w);

        w.transform.SetParent(null, true);
        RestoreSize(w);
        w.transform.position = ThrowOrigin();

        Collider[] cols = w.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols) c.enabled = true;

        Rigidbody rb = w.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = ThrowVelocity();
            rb.angularVelocity = Random.insideUnitSphere * 5f;
        }

        if (controller != null) StartCoroutine(IgnorePlayer(cols));
    }

    IEnumerator IgnorePlayer(Collider[] cols)
    {
        foreach (Collider c in cols)
            if (c != null) Physics.IgnoreCollision(c, controller, true);

        yield return new WaitForSeconds(0.5f);

        foreach (Collider c in cols)
            if (c != null) Physics.IgnoreCollision(c, controller, false);
    }

    void Drop()
    {
        WasteItem w = Held;
        RemoveFromBag(w);

        w.transform.SetParent(null, true);
        RestoreSize(w);
        foreach (Collider c in w.GetComponentsInChildren<Collider>()) c.enabled = true;

        Rigidbody rb = w.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(cam.transform.forward * dropForce, ForceMode.VelocityChange);
        }
    }

    public void RemoveFromBag(WasteItem w)
    {
        if (w == null) return;

        bag.Remove(w);
        originalScale.Remove(w);
        if (bag.Count > 0) bag[bag.Count - 1].gameObject.SetActive(true);
        if (arm != null) arm.goal = arm.carryPoint;
    }
}