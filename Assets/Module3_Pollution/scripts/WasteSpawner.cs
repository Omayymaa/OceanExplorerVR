using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WasteEntry
{
    public string displayName = "Déchet";
    public WasteType type;
    public GameObject prefab;
    public float scale = 1f;
}

public class WasteSpawner : MonoBehaviour
{
    public Terrain terrain;
    public List<WasteEntry> entries = new List<WasteEntry>();

    [Header("Centre de la zone (vide = le joueur)")]
    public Transform center;

    [Header("Anneau autour du centre")]
    public int count = 24;
    public float minRadius = 4f;
    public float maxRadius = 22f;

    [Header("Limites de la plage (X, Z)")]
    public Vector2 minXZ = new Vector2(-30f, 5f);
    public Vector2 maxXZ = new Vector2(30f, 42f);

    [Header("Placement")]
    public float seaLevel = 0.15f;
    public float minSpacing = 2f;
    public float clearance = 0.5f;
    public float dropHeight = 0.3f;
    public int seed = 11;

    [Header("Déchets de test (si Entries est vide)")]
    public float placeholderSize = 1.6f;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogWarning("WasteSpawner : Terrain manquant.");
            return;
        }

        if (center == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) center = p.transform;
        }

        Vector3 c = center != null ? center.position : transform.position;

        if (entries.Count == 0) BuildDefaultEntries();

        UnityEngine.Random.InitState(seed);
        List<Vector2> placed = new List<Vector2>();
        int attempts = 0;
        int index = 0;

        while (placed.Count < count && attempts < count * 60)
        {
            attempts++;

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float dist = Mathf.Sqrt(UnityEngine.Random.Range(minRadius * minRadius,
                                                              maxRadius * maxRadius));
            Vector2 p = new Vector2(c.x + Mathf.Cos(angle) * dist,
                                    c.z + Mathf.Sin(angle) * dist);

            // Dans les limites de la plage
            if (p.x < minXZ.x || p.x > maxXZ.x || p.y < minXZ.y || p.y > maxXZ.y) continue;

            // Pas trop près d'un autre déchet
            bool tooClose = false;
            foreach (Vector2 q in placed)
                if (Vector2.Distance(p, q) < minSpacing) { tooClose = true; break; }
            if (tooClose) continue;

            // Hors de l'eau
            float groundY = terrain.SampleHeight(new Vector3(p.x, 0f, p.y))
                          + terrain.transform.position.y;
            if (groundY < seaLevel + 0.02f) continue;

            // Pas dans un objet (décor, poubelle, pont, palmier...)
            Vector3 probe = new Vector3(p.x, groundY + clearance + 0.1f, p.y);
            if (Physics.CheckSphere(probe, clearance, ~0, QueryTriggerInteraction.Ignore)) continue;

            WasteEntry e = entries[index % entries.Count];
            index++;

            Create(e, new Vector3(p.x, groundY + dropHeight, p.y),
                   Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
            placed.Add(p);
        }

        if (placed.Count < count)
            Debug.Log("WasteSpawner : " + placed.Count + " déchets placés sur " + count
                      + " demandés (zone trop petite ou trop encombrée).");
    }

    void BuildDefaultEntries()
    {
        entries.Add(new WasteEntry { displayName = "Bouteille en plastique", type = WasteType.Plastique });
        entries.Add(new WasteEntry { displayName = "Bouteille en verre", type = WasteType.Verre });
        entries.Add(new WasteEntry { displayName = "Canette métallique", type = WasteType.Metal });
        entries.Add(new WasteEntry { displayName = "Boîte en carton", type = WasteType.Papier });
    }

    GameObject Create(WasteEntry e, Vector3 pos, Quaternion rot)
    {
        GameObject o;

        if (e.prefab != null)
        {
            o = Instantiate(e.prefab, pos, rot, transform);
            o.transform.localScale *= e.scale;

            foreach (MeshCollider mc in o.GetComponentsInChildren<MeshCollider>())
                mc.convex = true;

            foreach (MeshFilter mf in o.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.GetComponent<Collider>() == null)
                {
                    MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }
        }
        else
        {
            o = CreatePlaceholder(e.type, pos, rot);
        }

        o.name = e.displayName;

        WasteItem item = o.AddComponent<WasteItem>();
        item.type = e.type;
        item.displayName = e.displayName;

        o.tag = "Trash";

        if (o.GetComponent<Rigidbody>() == null)
            o.AddComponent<Rigidbody>();

        return o;
    }

    GameObject CreatePlaceholder(WasteType t, Vector3 pos, Quaternion rot)
    {
        PrimitiveType shape = (t == WasteType.Papier) ? PrimitiveType.Cube : PrimitiveType.Cylinder;
        GameObject o = GameObject.CreatePrimitive(shape);
        o.transform.SetParent(transform);
        o.transform.SetPositionAndRotation(pos, rot);

        float k = placeholderSize;
        switch (t)
        {
            case WasteType.Plastique:
                o.transform.localScale = new Vector3(0.08f, 0.13f, 0.08f) * k;
                WasteInfo.Paint(o, new Color(0.65f, 0.85f, 1f));
                break;
            case WasteType.Verre:
                o.transform.localScale = new Vector3(0.075f, 0.14f, 0.075f) * k;
                WasteInfo.Paint(o, new Color(0.1f, 0.45f, 0.2f));
                break;
            case WasteType.Metal:
                o.transform.localScale = new Vector3(0.07f, 0.065f, 0.07f) * k;
                WasteInfo.Paint(o, new Color(0.8f, 0.8f, 0.85f));
                break;
            default:
                o.transform.localScale = new Vector3(0.28f, 0.2f, 0.22f) * k;
                WasteInfo.Paint(o, new Color(0.6f, 0.42f, 0.22f));
                break;
        }
        return o;
    }

    // Visualisation de la zone dans la Scene view (objet Waste sélectionné)
    void OnDrawGizmosSelected()
    {
        Vector3 c = center != null ? center.position : transform.position;

        Gizmos.color = new Color(1f, 0.9f, 0.2f);
        DrawCircle(c, minRadius);
        Gizmos.color = new Color(0.2f, 1f, 0.4f);
        DrawCircle(c, maxRadius);

        Gizmos.color = new Color(0.3f, 0.7f, 1f);
        Vector3 a = new Vector3(minXZ.x, c.y, minXZ.y);
        Vector3 b = new Vector3(maxXZ.x, c.y, minXZ.y);
        Vector3 d = new Vector3(maxXZ.x, c.y, maxXZ.y);
        Vector3 e = new Vector3(minXZ.x, c.y, maxXZ.y);
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, d);
        Gizmos.DrawLine(d, e); Gizmos.DrawLine(e, a);
    }

    void DrawCircle(Vector3 c, float r)
    {
        Vector3 prev = c + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= 48; i++)
        {
            float a = i / 48f * Mathf.PI * 2f;
            Vector3 next = c + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}