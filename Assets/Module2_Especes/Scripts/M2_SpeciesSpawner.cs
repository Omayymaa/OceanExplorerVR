using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    public class M2_SpeciesSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] fishPrefabs = new GameObject[2];
        public GameObject[] crabPrefabs = new GameObject[2];
        public GameObject[] turtlePrefabs = new GameObject[2];

        [Header("Nombre d'animaux")]
        public int fishCount = 8;
        public int crabCount = 3;
        public int turtleCount = 2;

        [Header("Zone des poissons")]
        public Vector3 areaMin = new Vector3(10, 11, 10);
        public Vector3 areaMax = new Vector3(90, 20, 90);

        [Header("Crabes et tortues")]
        public bool wholeMap = true;
        public float mapMin = 10f;
        public float mapMax = 90f;
        public Transform center;
        public float spawnRadius = 25f;
        public float minSpacing = 3f;
        public NavMeshSurface surface;

        [Header("Éviter les chevauchements")]
        public float crabClearRadius = 0.6f;     // Place libre exigée autour d'un crabe (m)
        public float turtleClearRadius = 1.5f;   // Place libre exigée autour d'une tortue (m)
        public LayerMask obstacleMask = ~0;      // Couches considérées comme obstacles (tout par défaut)

        void Start()
        {
            Respawn();
        }

        public void Respawn()
        {
            SpawnFish();
            SpawnOnSeabed(crabPrefabs, crabCount, crabClearRadius);
            SpawnOnSeabed(turtlePrefabs, turtleCount, turtleClearRadius);
        }

        List<GameObject> GetValidPrefabs(GameObject[] prefabs)
        {
            List<GameObject> valid = new List<GameObject>();
            if (prefabs != null)
                foreach (GameObject p in prefabs)
                    if (p != null) valid.Add(p);
            return valid;
        }

        List<GameObject> CreateSpawnSequence(GameObject[] prefabs, int count)
        {
            List<GameObject> available = GetValidPrefabs(prefabs);
            List<GameObject> sequence = new List<GameObject>();
            if (available.Count == 0 || count <= 0) return sequence;

            while (sequence.Count < count)
            {
                List<GameObject> round = new List<GameObject>(available);
                for (int i = round.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    GameObject t = round[i]; round[i] = round[j]; round[j] = t;
                }
                foreach (GameObject p in round)
                {
                    if (sequence.Count >= count) break;
                    sequence.Add(p);
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
                {
                    swim.speed = Random.Range(1f, 2.5f);               // Vitesse calme et réaliste
                    if (prefab.name == "Seahorse") swim.speed = 0.4f;  // L'hippocampe nage très lentement
                    swim.areaMin = areaMin;                            // Zone de nage = zone de départ
                    swim.areaMax = areaMax;
                }
            }
        }

        // Hauteur du sol à (x, z) en cherchant le terrain qui contient ce point
        bool TryGetGroundHeight(float x, float z, out float y)
        {
            foreach (Terrain t in Terrain.activeTerrains)
            {
                Vector3 p = t.transform.position;
                Vector3 s = t.terrainData.size;
                if (x >= p.x && x <= p.x + s.x && z >= p.z && z <= p.z + s.z)
                {
                    y = t.SampleHeight(new Vector3(x, 0, z)) + p.y;
                    return true;
                }
            }
            y = 10f;
            return false;
        }

        void SpawnOnSeabed(GameObject[] prefabs, int count, float clearRadius)
        {
            if (count <= 0) return;

            List<GameObject> sequence = CreateSpawnSequence(prefabs, count);
            if (sequence.Count == 0)
            {
                Debug.LogError("Aucun prefab assigné pour les crabes ou tortues !");
                return;
            }

            Vector3 c = center != null ? center.position : new Vector3(50, 10, 25);
            List<Vector3> placed = new List<Vector3>();
            int spawned = 0;

            for (int attempt = 0; attempt < 1000 && spawned < sequence.Count; attempt++)
            {
                float x, z;
                if (wholeMap)
                {
                    x = Random.Range(mapMin, mapMax);
                    z = Random.Range(mapMin, mapMax);
                }
                else
                {
                    Vector2 r = Random.insideUnitCircle * spawnRadius;
                    x = c.x + r.x;
                    z = c.z + r.y;
                }

                TryGetGroundHeight(x, z, out float groundY);

                if (!NavMesh.SamplePosition(new Vector3(x, groundY, z), out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    continue;

                // Distance minimale avec les animaux déjà placés
                bool tooClose = false;
                foreach (Vector3 p in placed)
                {
                    if (Vector3.Distance(p, hit.position) < minSpacing) { tooClose = true; break; }
                }
                if (tooClose) continue;

                // Rejette l'endroit si un objet solide (rocher, corail...) occupe déjà la place
                Vector3 checkPos = hit.position + Vector3.up * (clearRadius + 0.2f);
                if (Physics.CheckSphere(checkPos, clearRadius, obstacleMask, QueryTriggerInteraction.Ignore))
                    continue;

                GameObject animal = Instantiate(sequence[spawned], hit.position, Quaternion.identity, transform);
                NavMeshAgent agent = animal.GetComponent<NavMeshAgent>();
                if (agent != null) agent.Warp(hit.position);

                placed.Add(hit.position);
                spawned++;
            }

            if (spawned < sequence.Count)
                Debug.LogWarning("Placement incomplet : " + spawned + "/" + sequence.Count + " animaux.");
        }
    }
}