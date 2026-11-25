using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class SandTerrain : MonoBehaviour
{
    public float duneHeight = 0.12f;
    public float scale = 8f;
    public int seed = 42;
    public float flatZoneStart = 0.7f;
    public float shoreHeight = 0.02f;

    [ContextMenu("Générer les dunes")]
    public void Generate()
    {
        TerrainData data = GetComponent<Terrain>().terrainData;
        int res = data.heightmapResolution;
        float[,] h = new float[res, res];

        for (int x = 0; x < res; x++)
            for (int z = 0; z < res; z++)
            {
                float nx = (float)x / res * scale + seed;
                float nz = (float)z / res * scale + seed;

                float dunes = Mathf.PerlinNoise(nx, nz) * duneHeight
                            + Mathf.PerlinNoise(nx * 5f, nz * 5f) * duneHeight * 0.12f;

                float t = Mathf.InverseLerp(flatZoneStart, 1f, (float)z / res);
                t = t * t * (3f - 2f * t);

                h[z, x] = Mathf.Lerp(dunes, shoreHeight, t);
            }
        data.SetHeights(0, 0, h);
    }
}