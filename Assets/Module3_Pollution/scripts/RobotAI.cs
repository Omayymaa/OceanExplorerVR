using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RobotAI : MonoBehaviour
{
    public Transform[] pointsDepot;   // glisser le PointDepot de chaque poubelle
    public Transform holdPoint;
    public float pickupDistance = 1.2f;
    public float dropDistance = 2.5f;
    public float maxHeightAboveRobot = 3f;
    public float samplingRange = 3f;
    public float blacklistTime = 15f;

    enum State { Seek, Carry }
    State state = State.Seek;

    NavMeshAgent agent;
    Transform target;
    Transform depotChoisi;
    Collider[] myColliders;
    float repathTimer;
    float stuckTimer;
    Dictionary<Transform, float> ignored = new Dictionary<Transform, float>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0f;
        myColliders = GetComponentsInChildren<Collider>();

        if (!agent.isOnNavMesh)
            Debug.LogWarning("Le robot n'est pas sur le NavMesh : refais le Bake.");

        if (pointsDepot == null || pointsDepot.Length == 0)
            Debug.LogWarning("RobotAI : aucun PointDepot assigné.");
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (state == State.Seek) SeekUpdate();
        else CarryUpdate();
    }

    void SeekUpdate()
    {
        if (!IsValid(target))
        {
            target = ChooseTarget();
            stuckTimer = 0f;
            repathTimer = 0f;
            if (target == null) return;
            IgnorePhysics(target, true);
        }

        if (DistanceToEdge(target) < pickupDistance)
        {
            PickUp();
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.3f;
            if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, samplingRange, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
            {
                Ignore(target);
                target = null;
                return;
            }
        }

        bool immobile = !agent.pathPending && agent.velocity.magnitude < 0.1f;
        if (immobile) stuckTimer += Time.deltaTime;
        else stuckTimer = 0f;

        if (stuckTimer > 1.2f)
        {
            stuckTimer = 0f;
            if (DistanceToEdge(target) < pickupDistance * 2.5f) PickUp();
            else { Ignore(target); target = null; }
        }
    }

    void CarryUpdate()
    {
        if (depotChoisi == null)
        {
            depotChoisi = DepotLePlusProche();
            if (depotChoisi == null) return;
        }

        float flat = FlatDistance(transform.position, depotChoisi.position);

        if (flat < dropDistance)
        {
            Drop();
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.5f;
            if (NavMesh.SamplePosition(depotChoisi.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(depotChoisi.position);
        }

        bool immobile = !agent.pathPending && agent.velocity.magnitude < 0.1f;
        if (immobile) stuckTimer += Time.deltaTime;
        else stuckTimer = 0f;

        if (stuckTimer > 1.5f && flat < dropDistance * 2f)
            Drop();
    }

    Transform DepotLePlusProche()
    {
        Transform best = null;
        float min = Mathf.Infinity;

        if (pointsDepot == null) return null;

        foreach (Transform p in pointsDepot)
        {
            if (p == null) continue;
            float d = FlatDistance(transform.position, p.position);
            if (d < min) { min = d; best = p; }
        }
        return best;
    }

    Transform ChooseTarget()
    {
        List<Transform> dead = new List<Transform>();
        foreach (Transform k in ignored.Keys)
            if (k == null) dead.Add(k);
        foreach (Transform k in dead)
            ignored.Remove(k);

        float min = Mathf.Infinity;
        Transform best = null;

        foreach (GameObject t in GameObject.FindGameObjectsWithTag("Trash"))
        {
            if (IsIgnored(t.transform)) continue;
            if (t.transform.position.y > transform.position.y + maxHeightAboveRobot) continue;

            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic) continue;

            if (!NavMesh.SamplePosition(t.transform.position, out NavMeshHit hit, samplingRange, NavMesh.AllAreas))
                continue;

            float d = Vector3.Distance(transform.position, hit.position);
            if (d < min) { min = d; best = t.transform; }
        }
        return best;
    }

    float DistanceToEdge(Transform t)
    {
        Collider col = t.GetComponent<Collider>();
        Vector3 p = col != null ? col.ClosestPoint(transform.position) : t.position;
        return FlatDistance(transform.position, p);
    }

    void IgnorePhysics(Transform t, bool ignore)
    {
        foreach (Collider tc in t.GetComponentsInChildren<Collider>())
            foreach (Collider mc in myColliders)
                if (tc != null && mc != null)
                    Physics.IgnoreCollision(tc, mc, ignore);
    }

    bool IsValid(Transform t)
    {
        if (t == null) return false;
        if (!t.CompareTag("Trash")) return false;
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic) return false;
        return true;
    }

    void Ignore(Transform t)
    {
        if (t != null) ignored[t] = Time.time + blacklistTime;
    }

    bool IsIgnored(Transform t)
    {
        return ignored.TryGetValue(t, out float until) && Time.time < until;
    }

    void PickUp()
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        foreach (Collider c in target.GetComponentsInChildren<Collider>()) c.enabled = false;

        target.tag = "Untagged";
        target.SetParent(holdPoint);
        target.localPosition = Vector3.zero;

        depotChoisi = DepotLePlusProche();   // choix de la poubelle la plus proche

        state = State.Carry;
        stuckTimer = 0f;
        repathTimer = 0f;
    }

    void Drop()
    {
        if (target != null)
        {
            Destroy(target.gameObject);
            if (Affiche.Instance != null) Affiche.Instance.DechetDepose();
        }
        target = null;
        depotChoisi = null;
        state = State.Seek;
        stuckTimer = 0f;
    }

    float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.Distance(a, b);
    }
}