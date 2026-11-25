using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Accès")]
    public Transform seat;
    public Transform exitPoint;
    public Transform modelRoot;
    public KeyCode boardKey = KeyCode.B;
    public float boardRadius = 4.5f;
    public float exitRadius = 8f;
    public bool requireBeachClean = true;

    [Header("Conduite")]
    public float enginePower = 9000f;
    public float reversePower = 4500f;
    public float turnPower = 3500f;
    public float maxSpeed = 7f;
    public float sideGrip = 2f;

    [Header("Mer")]
    public float seaLevel = 0.15f;
    public Vector2 limitX = new Vector2(-44f, 44f);
    public Vector2 limitZ = new Vector2(52f, 88f);

    [Header("Mouvement des vagues")]
    public float bobAmount = 0.06f;
    public float bobSpeed = 1.4f;
    public float rollAmount = 3f;

    public static bool Driving;
    public static BoatController Instance;

    Rigidbody rb;
    PlayerController pc;
    CharacterController cc;
    Transform playerTf;
    bool aboard;
    Vector3 modelPos;
    Quaternion modelRot;
    float phase;

    public float Speed { get { return rb != null ? rb.linearVelocity.magnitude : 0f; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Driving = false;
        Instance = null;
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 500f;
        rb.useGravity = false;
        rb.linearDamping = 1.2f;
        rb.angularDamping = 3f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        Vector3 p = transform.position;
        p.y = seaLevel;
        rb.position = p;

        pc = Object.FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            cc = pc.GetComponent<CharacterController>();
            playerTf = pc.transform;
        }
        else Debug.LogWarning("BoatController : aucun PlayerController dans la scène.");

        if (modelRoot != null)
        {
            modelPos = modelRoot.localPosition;
            modelRot = modelRoot.localRotation;
        }
        phase = Random.value * 10f;

        GameObject block = GameObject.Find("ZoneMerBloquee");
        if (block != null)
        {
            foreach (Collider a in GetComponentsInChildren<Collider>())
                foreach (Collider b in block.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(a, b, true);
        }
    }

    void Update()
    {
        if (playerTf == null || seat == null) return;

        if (!aboard)
        {
            Vector3 to = seat.position - playerTf.position;
            to.y = 0f;

            if (to.magnitude > boardRadius) return;

            if (requireBeachClean && !BeachProgress.Clean)
            {
                HUDMessage.ShowTimed("Bateau verrouillé\nTermine d'abord le nettoyage de la plage", 0.15f);
                return;
            }

            if (Input.GetKeyDown(boardKey)) Board();
        }
        else if (Input.GetKeyDown(boardKey))
        {
            TryLeave();
        }
    }

    void Board()
    {
        aboard = true;
        Driving = true;
        MarineStats.Active = CollectSystem.None;   // la grue se déploie avec la touche 1

        pc.canMove = false;
        cc.enabled = false;

        playerTf.SetParent(seat, false);
        playerTf.localPosition = Vector3.zero;
        playerTf.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    void TryLeave()
    {
        if (exitPoint != null)
        {
            Vector3 d = exitPoint.position - transform.position;
            d.y = 0f;

            if (d.magnitude > exitRadius)
            {
                HUDMessage.ShowTimed("Ramène le bateau au quai pour descendre", 2.5f);
                return;
            }
        }

        aboard = false;
        Driving = false;
        MarineStats.Active = CollectSystem.None;   // la grue se replie

        playerTf.SetParent(null, true);
        if (exitPoint != null) playerTf.position = exitPoint.position;
        playerTf.rotation = Quaternion.Euler(0f, playerTf.eulerAngles.y, 0f);

        cc.enabled = true;
        pc.canMove = true;
    }

    void FixedUpdate()
    {
        if (aboard)
        {
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            if (v > 0f) rb.AddForce(transform.forward * v * enginePower);
            else if (v < 0f) rb.AddForce(transform.forward * v * reversePower);

            float fs = Vector3.Dot(rb.linearVelocity, transform.forward);
            float turnFactor = Mathf.Clamp(fs / 3f, -1f, 1f);
            if (Mathf.Abs(turnFactor) < 0.35f) turnFactor = 0.35f;
            rb.AddTorque(Vector3.up * h * turnPower * turnFactor);
        }

        Vector3 lateral = Vector3.Project(rb.linearVelocity, transform.right);
        rb.AddForce(-lateral * rb.mass * sideGrip);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        Vector3 pos = rb.position;
        Vector3 vel = rb.linearVelocity;

        if (pos.x < limitX.x) { pos.x = limitX.x; vel.x = Mathf.Max(0f, vel.x); }
        if (pos.x > limitX.y) { pos.x = limitX.y; vel.x = Mathf.Min(0f, vel.x); }
        if (pos.z < limitZ.x) { pos.z = limitZ.x; vel.z = Mathf.Max(0f, vel.z); }
        if (pos.z > limitZ.y) { pos.z = limitZ.y; vel.z = Mathf.Min(0f, vel.z); }

        rb.position = pos;
        rb.linearVelocity = vel;
    }

    void LateUpdate()
    {
        if (modelRoot == null) return;

        float t = Time.time * bobSpeed + phase;
        modelRoot.localPosition = modelPos + Vector3.up * Mathf.Sin(t) * bobAmount;
        modelRoot.localRotation = modelRot * Quaternion.Euler(
            Mathf.Sin(t * 0.8f) * rollAmount, 0f, Mathf.Cos(t * 0.6f) * rollAmount);
    }
}