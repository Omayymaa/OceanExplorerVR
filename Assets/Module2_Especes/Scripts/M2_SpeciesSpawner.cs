using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    public class M2_SpeciesSpawner : MonoBehaviour
    {
        public GameObject fishPrefab;
        public GameObject crabPrefab;
        public GameObject turtlePrefab;
        public int fishCount = 8;
        public int crabCount = 3;
        public int turtleCount = 2;
        public Vector3 areaMin = new Vector3(30, 13, 5);
        public Vector3 areaMax = new Vector3(70, 20, 40);

        void Start()
        {
            Respawn();
        }

        public void Respawn()
        {
            for (int i = 0; i < fishCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(areaMin.x, areaMax.x),
                    Random.Range(areaMin.y, areaMax.y),
                    Random.Range(areaMin.z, areaMax.z));
                GameObject fish = Instantiate(fishPrefab, pos, Quaternion.identity, transform);
                fish.GetComponent<M2_FishSwim>().speed = Random.Range(2f, 5f);
            }
            SpawnOnSeabed(crabPrefab, crabCount);
            SpawnOnSeabed(turtlePrefab, turtleCount);
        }

        void SpawnOnSeabed(GameObject prefab, int count)
        {
            int spawned = 0;
            for (int attempt = 0; attempt < 200 && spawned < count; attempt++)
            {
                Vector3 point = new Vector3(Random.Range(10f, 90f), 10f, Random.Range(10f, 90f));
                if (NavMesh.SamplePosition(point, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                {
                    Instantiate(prefab, hit.position, Quaternion.identity, transform);
                    spawned++;
                }
            }
        }
    }
}