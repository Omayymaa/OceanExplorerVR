using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    public class M2_SpeciesSpawner : MonoBehaviour
    {
        [Header("Prefabs des poissons")]
        public GameObject[] fishPrefabs = new GameObject[2];

        [Header("Prefabs des crabes")]
        public GameObject[] crabPrefabs = new GameObject[2];

        [Header("Prefabs des tortues")]
        public GameObject[] turtlePrefabs = new GameObject[2];

        [Header("Nombre d'animaux")]
        public int fishCount = 8;
        public int crabCount = 3;
        public int turtleCount = 2;

        [Header("Zone des poissons")]
        public Vector3 areaMin = new Vector3(30, 11, 5);
        public Vector3 areaMax = new Vector3(70, 18, 40);

        [Header("Zone du fond marin")]
        public Transform center;
        public float spawnRadius = 25f;
        public float minSpacing = 3f;
        public NavMeshSurface surface;

        void Start()
        {
            // Décommentez la ligne suivante si le NavMesh n'est pas trouvé :
            // if (surface != null) surface.BuildNavMesh();

            Respawn();
        }

        public void Respawn()
        {
            SpawnFish();
            SpawnOnSeabed(crabPrefabs, crabCount);
            SpawnOnSeabed(turtlePrefabs, turtleCount);
        }

        List<GameObject> GetValidPrefabs(GameObject[] prefabs)
        {
            List<GameObject> valid = new List<GameObject>();

            if (prefabs != null)
            {
                foreach (GameObject prefab in prefabs)
                {
                    if (prefab != null)
                        valid.Add(prefab);
                }
            }

            return valid;
        }

        List<GameObject> CreateSpawnSequence(GameObject[] prefabs, int count)
        {
            List<GameObject> available = GetValidPrefabs(prefabs);
            List<GameObject> sequence = new List<GameObject>();

            if (available.Count == 0 || count <= 0)
                return sequence;

            // Chaque prefab est utilisé avant de recommencer
            while (sequence.Count < count)
            {
                List<GameObject> round = new List<GameObject>(available);

                for (int i = round.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    GameObject temp = round[i];
                    round[i] = round[j];
                    round[j] = temp;
                }

                foreach (GameObject prefab in round)
                {
                    if (sequence.Count >= count)
                        break;

                    sequence.Add(prefab);
                }
            }

            return sequence;
        }

        void SpawnFish()
        {
            List<GameObject> sequence = CreateSpawnSequence(fishPrefabs, fishCount);

            if (sequence.Count == 0 && fishCount > 0)
            {
                Debug.LogError("Assigne les prefabs dans Fish Prefabs !");
                return;
            }

            foreach (GameObject prefab in sequence)
            {
                Vector3 pos = new Vector3(
                    Random.Range(areaMin.x, areaMax.x),
                    Random.Range(areaMin.y, areaMax.y),
                    Random.Range(areaMin.z, areaMax.z));

                GameObject fish = Instantiate(prefab, pos, Quaternion.identity, transform);

                M2_FishSwim swim = fish.GetComponent<M2_FishSwim>();

                if (swim != null)
                    swim.speed = Random.Range(2f, 5f);
                else
                    Debug.LogWarning(prefab.name + " ne possède pas le script M2_FishSwim.");
            }
        }

        void SpawnOnSeabed(GameObject[] prefabs, int count)
        {
            if (count <= 0)
                return;

            List<GameObject> sequence = CreateSpawnSequence(prefabs, count);

            if (sequence.Count == 0)
            {
                Debug.LogError("Aucun prefab assigné pour les crabes ou tortues !");
                return;
            }

            Vector3 c = center != null ? center.position : new Vector3(50, 10, 25);
            Terrain terrain = Terrain.activeTerrain;
            List<Vector3> placed = new List<Vector3>();
            int spawned = 0;

            for (int attempt = 0; attempt < 300 && spawned < sequence.Count; attempt++)
            {
                Vector2 r = Random.insideUnitCircle * spawnRadius;
                float x = Mathf.Clamp(c.x + r.x, 5f, 95f);
                float z = Mathf.Clamp(c.z + r.y, 5f, 95f);

                float groundY = terrain != null
                    ? terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y
                    : 10f;

                if (!NavMesh.SamplePosition(new Vector3(x, groundY, z), out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    continue;

                // Trop près d'un animal déjà placé : on réessaie ailleurs
                bool tooClose = false;
                foreach (Vector3 p in placed)
                {
                    if (Vector3.Distance(p, hit.position) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose)
                    continue;

                GameObject animal = Instantiate(sequence[spawned], hit.position, Quaternion.identity, transform);

                NavMeshAgent agent = animal.GetComponent<NavMeshAgent>();
                if (agent != null)
                    agent.Warp(hit.position);

                placed.Add(hit.position);
                spawned++;
            }

            if (spawned < sequence.Count)
                Debug.LogWarning("Placement incomplet : " + spawned + "/" + sequence.Count + " animaux. Vérifie le NavMesh.");
        }
    }
}