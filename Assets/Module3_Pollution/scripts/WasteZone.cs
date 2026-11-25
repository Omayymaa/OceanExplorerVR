using UnityEngine;

public class WasteZone : MonoBehaviour
{
    public enum Mode { Cluster, Line, Floating }

    [System.Serializable]
    public class Entry
    {
        public GameObject prefab;
        public WasteType type;
        public string displayName = "Déchet";
        public bool decorative;
    }

    public Terrain terrain;
    public Mode mode = Mode.Cluster;
    public Entry[] entries;
    public int count = 8;
    public float radius = 3f;
    public float lineLength = 40f;
    public float seaLevel = 0.15f;
    public float minScale = 1f;
    public float maxScale = 1.3f;
    public int seed = 1;

    void Start()
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        Random.InitState(seed);

        for (int i = 0; i < count; i++)
            SpawnOne(i);
    }

    Vector3 PickPosition(int i)
    {
        Vector3 c = transform.position;

        if (mode == Mode.Line)
        {
            float t = count > 1 ? (float)i / (count - 1) : 0.5f;
            float x = c.x + (t - 0.5f) * lineLength + Random.Range(-1f, 1f);
            float z = c.z + Mathf.Sin(x * 0.3f) * 1.2f + Random.Range(-0.7f, 0.7f);
            return new Vector3(x, 0, z);
        }

        if (mode == Mode.Floating)
        {
            float x = c.x + Random.Range(-lineLength / 2f, lineLength / 2f);
            float z = c.z + Random.Range(-radius, radius);
            return new Vector3(x, 0, z);
        }

        Vector2 p = Random.insideUnitCircle * radius;
        return new Vector3(c.x + p.x, 0, c.z + p.y);
    }

    float GroundY(float x, float z)
    {
        return terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
    }

    void SpawnOne(int i)
    {
        if (entries == null || entries.Length == 0) return;

        Entry e = entries[Random.Range(0, entries.Length)];
        if (e.prefab == null) return;

        Vector3 p = PickPosition(i);
        bool floating = mode == Mode.Floating;

        float y = floating ? seaLevel + 0.03f
                           : GroundY(p.x, p.z) + (e.decorative ? -0.2f : 0.3f);

        GameObject o = Instantiate(e.prefab, new Vector3(p.x, y, p.z),
            Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);

        o.transform.localScale *= Random.Range(minScale, maxScale);

        if (e.decorative)
        {
            if (floating)
            {
                foreach (Collider c in o.GetComponentsInChildren<Collider>())
                    Destroy(c);
                o.AddComponent<FloatBob>();
            }
            else
            {
                o.isStatic = true;
            }
            return;
        }

        o.tag = "Trash";

        foreach (MeshCollider mc in o.GetComponentsInChildren<MeshCollider>())
            mc.convex = true;

        if (o.GetComponentInChildren<Collider>() == null) FitBox(o);

        Rigidbody rb = o.GetComponent<Rigidbody>();
        if (rb == null) rb = o.AddComponent<Rigidbody>();
        rb.mass = 0.3f;

        WasteItem item = o.GetComponent<WasteItem>();
        if (item == null) item = o.AddComponent<WasteItem>();
        item.type = e.type;
        item.displayName = string.IsNullOrEmpty(e.displayName) ? e.prefab.name : e.displayName;
        item.inSea = floating;

        if (floating)
        {
            rb.isKinematic = true;
            o.AddComponent<FloatBob>();
        }
        else
        {
            o.transform.rotation = Quaternion.Euler(
                Random.value < 0.4f ? 90f : 0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
        }
    }

    void FitBox(GameObject o)
    {
        Renderer[] rs = o.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;

        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);

        BoxCollider col = o.AddComponent<BoxCollider>();
        Vector3 s = o.transform.lossyScale;
        col.center = o.transform.InverseTransformPoint(b.center);
        col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 c = transform.position;

        if (mode == Mode.Cluster)
            Gizmos.DrawWireSphere(c, radius);
        else
            Gizmos.DrawWireCube(c, new Vector3(lineLength, 0.5f, mode == Mode.Floating ? radius * 2f : 3f));
    }
}