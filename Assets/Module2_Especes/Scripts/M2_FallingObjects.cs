using UnityEngine;

namespace OceanExplorer.Module2
{
    public class M2_FallingObjects : MonoBehaviour
    {
        public GameObject[] prefabs;
        public float spawnInterval = 0.7f;
        public float spawnHeight = 31f;
        public float mapMin = 5f;
        public float mapMax = 95f;
        public float fallSpeedMin = 0.6f;
        public float fallSpeedMax = 1.5f;
        public float baseScale = 1f;
        public float settledLifetime = 20f;
        public int maxObjects = 60;
        public Terrain terrain;

        float timer;

        void Update()
        {
            if (prefabs == null || prefabs.Length == 0) return;
            timer += Time.deltaTime;
            if (timer < spawnInterval || transform.childCount >= maxObjects) return;
            timer = 0f;

            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) return;
            Vector3 pos = new Vector3(Random.Range(mapMin, mapMax), spawnHeight,
                                      Random.Range(mapMin, mapMax));
            var go = Instantiate(prefab, pos, Random.rotation, transform);
            go.transform.localScale *= baseScale * Random.Range(0.7f, 1.3f);
            go.AddComponent<FallingItem>().Init(
                Random.Range(fallSpeedMin, fallSpeedMax), terrain, settledLifetime);
        }
    }

    public class FallingItem : MonoBehaviour
    {
        float speed, life, phase;
        Terrain terrain;
        Vector3 spin;
        bool landed;

        public void Init(float s, Terrain t, float settledLifetime)
        {
            speed = s; terrain = t; life = settledLifetime;
            phase = Random.value * 10f;
            spin = Random.insideUnitSphere * 40f;
        }

        void Update()
        {
            if (landed)
            {
                life -= Time.deltaTime;
                if (life <= 0f) Destroy(gameObject);
                return;
            }

            Vector3 p = transform.position;
            p.y -= speed * Time.deltaTime;
            p.x += Mathf.Sin(Time.time * 1.2f + phase) * 0.4f * Time.deltaTime;
            p.z += Mathf.Cos(Time.time * 1.0f + phase) * 0.4f * Time.deltaTime;

            float ground = terrain != null
                ? terrain.SampleHeight(p) + terrain.transform.position.y
                : 10f;

            if (p.y <= ground)
            {
                p.y = ground;
                landed = true;
            }
            else
            {
                transform.Rotate(spin * Time.deltaTime);
            }
            transform.position = p;
        }
    }
}