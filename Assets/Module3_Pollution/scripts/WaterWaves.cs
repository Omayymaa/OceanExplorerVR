using UnityEngine;

public class WaterWaves : MonoBehaviour
{
    public int size = 256;
    public float strength = 5f;
    public Vector2 tiling = new Vector2(10, 4);
    public Vector2 speed = new Vector2(0.01f, 0.006f);

    Material mat;
    Vector2 offset;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_NORMALMAP");
        mat.SetTexture("_BumpMap", BuildNormalMap());
        mat.SetTextureScale("_BumpMap", tiling);
    }

    void Update()
    {
        offset += speed * Time.deltaTime;
        mat.SetTextureOffset("_BumpMap", offset);
    }

    float Height(float u, float v)
    {
        float t = Mathf.PI * 2f;
        return Mathf.Sin(t * (2 * u + 1 * v)) * 0.5f
             + Mathf.Sin(t * (3 * u - 2 * v) + 1.3f) * 0.3f
             + Mathf.Sin(t * (5 * u + 4 * v) + 2.1f) * 0.2f;
    }

    Texture2D BuildNormalMap()
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
        tex.wrapMode = TextureWrapMode.Repeat;
        float d = 1f / size;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;
                float v = (float)y / size;

                float dx = Height(u + d, v) - Height(u - d, v);
                float dy = Height(u, v + d) - Height(u, v - d);

                Vector3 n = new Vector3(-dx * strength, -dy * strength, 1f).normalized;
                tex.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, n.x * 0.5f + 0.5f));
            }

        tex.Apply();
        return tex;
    }
}