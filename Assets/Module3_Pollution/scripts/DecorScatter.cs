using UnityEngine;

public class DecorScatter : MonoBehaviour
{
    public Terrain terrain;
    public GameObject[] prefabs;

    [Header("Rangée alignée")]
    public int count = 6;
    public float startX = 2f;
    public float rowZ = 44f;
    public float spacing = 3f;
    public float rotationY = 20f;
    public float yOffset = 0.1f;

    [Header("Petite variation naturelle")]
    public float zJitter = 0.15f;
    public float rotationJitter = 3f;
    public int seed = 7;

    void Start()
    {
        Random.InitState(seed);

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * spacing;
            float z = rowZ + Random.Range(-zJitter, zJitter);

            float y = terrain.SampleHeight(new Vector3(x, 0, z))
                    + terrain.transform.position.y + yOffset;

            GameObject prefab = prefabs[i % prefabs.Length];
            float rotY = rotationY + Random.Range(-rotationJitter, rotationJitter);

            Instantiate(prefab, new Vector3(x, y, z),
                Quaternion.Euler(0f, rotY, 0f), transform);
        }
    }
}