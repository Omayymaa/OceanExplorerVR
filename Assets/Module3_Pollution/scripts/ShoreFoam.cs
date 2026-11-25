using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShoreFoam : MonoBehaviour
{
    public float length = 100f;
    public float centerX = 0f;
    public float step = 0.5f;

    public float seaEdgeZ = 50f;
    public float shoreZ = 50.3f;
    public float height = 0.2f;
    public float wetHeight = 0.05f;

    public float wetWidth = 1.5f;
    public float foamWidth = 1.0f;
    public float shallowWidth = 4f;

    public float swash = 0.3f;
    public float waveAmp = 0.4f;
    public float speed = 1f;

    public Color wetColor = new Color(0.45f, 0.38f, 0.28f, 0.35f);
    public Color foamColor = new Color(1f, 1f, 1f, 0.85f);
    public Color shallowColor = new Color(0.45f, 0.85f, 0.8f, 0.5f);

    const int Rows = 5;

    Mesh mesh;
    Vector3[] verts;
    Color[] cols;
    int count;

    void Start()
    {
        count = Mathf.Max(2, Mathf.RoundToInt(length / step));

        verts = new Vector3[(count + 1) * Rows];
        cols = new Color[verts.Length];

        int[] tris = new int[count * (Rows - 1) * 6];
        int t = 0;

        for (int c = 0; c < count; c++)
            for (int r = 0; r < Rows - 1; r++)
            {
                int a = c * Rows + r;
                int b = a + 1;
                int d = a + Rows;
                int e = d + 1;

                tris[t++] = a; tris[t++] = d; tris[t++] = b;
                tris[t++] = b; tris[t++] = d; tris[t++] = e;
            }

        mesh = new Mesh();
        mesh.name = "ShoreFoam";
        mesh.MarkDynamic();
        mesh.vertices = verts;
        mesh.colors = cols;
        mesh.triangles = tris;
        mesh.bounds = new Bounds(new Vector3(centerX, height, shoreZ), new Vector3(length + 10f, 4f, 30f));

        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material m = new Material(Shader.Find("Sprites/Default"));
        m.renderQueue = 3500;
        mr.sharedMaterial = m;
        mr.sortingOrder = 100;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        Debug.Log("ShoreFoam : centre en " + transform.TransformPoint(new Vector3(centerX, height, shoreZ)));
    }

    void Update()
    {
        if (mesh == null) return;

        float time = Time.time * speed;
        float breath = swash * Mathf.Sin(time * 0.7f);

        Color wet0 = wetColor; wet0.a = 0f;
        Color shallow1 = Color.Lerp(foamColor, shallowColor, 0.7f);
        Color shallow2 = shallowColor; shallow2.a = 0f;

        float landLimit = seaEdgeZ - 0.02f;

        for (int c = 0; c <= count; c++)
        {
            float x = -length * 0.5f + c * (length / count);
            float wx = centerX + x;

            float o = breath + waveAmp * (0.6f * Mathf.Sin(wx * 0.7f + time * 0.8f)
                                        + 0.4f * Mathf.Sin(wx * 1.6f - time * 1.3f));

            float patch = 0.65f + 0.35f * Mathf.Sin(wx * 1.9f + time * 1.2f) * Mathf.Sin(wx * 0.7f - time * 0.5f);

            Color foam = foamColor;
            foam.a *= Mathf.Clamp01(patch);

            float z0 = shoreZ + o;
            int i = c * Rows;

            verts[i + 0] = new Vector3(wx, wetHeight, Mathf.Min(z0 - wetWidth, landLimit));
            verts[i + 1] = new Vector3(wx, wetHeight, Mathf.Min(z0 - 0.1f, landLimit));
            verts[i + 2] = new Vector3(wx, height, z0);
            verts[i + 3] = new Vector3(wx, height, z0 + foamWidth);
            verts[i + 4] = new Vector3(wx, height, z0 + foamWidth + shallowWidth);

            cols[i + 0] = wet0;
            cols[i + 1] = wetColor;
            cols[i + 2] = foam;
            cols[i + 3] = shallow1;
            cols[i + 4] = shallow2;
        }

        mesh.vertices = verts;
        mesh.colors = cols;
    }
}