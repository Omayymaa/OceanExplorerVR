using System.Collections.Generic;
using UnityEngine;

namespace OceanExplorer.Module2
{
    // Banc de poissons : un chef invisible se déplace, les poissons le suivent en groupe
    public class M2_FishSchool : MonoBehaviour
    {
        [Header("Banc")]
        public GameObject fishPrefab;          // Prefab du poisson du banc
        public int count = 12;                 // Nombre de poissons dans le banc
        public float spread = 3f;              // Dispersion autour du chef (mètres)

        [Header("Mouvement")]
        public float leaderSpeed = 2f;         // Vitesse du chef
        public float followSpeed = 4f;         // Vitesse de rattrapage des poissons
        public float turnSpeed = 4f;           // Vitesse de rotation des poissons
        public Vector3 areaMin = new Vector3(10, 12, 10);   // Zone de nage (coin min)
        public Vector3 areaMax = new Vector3(90, 22, 90);   // Zone de nage (coin max)

        Transform leader;                      // Le chef (objet invisible)
        Vector3 target;                        // Destination actuelle du chef
        readonly List<Transform> fish = new List<Transform>();
        readonly List<Vector3> offsets = new List<Vector3>();   // Place de chaque poisson

        void Start()
        {
            // Crée le chef au centre de la zone
            leader = new GameObject(name + "_Leader").transform;
            leader.SetParent(transform);
            leader.position = RandomPoint();
            target = RandomPoint();

            // Crée les poissons autour du chef
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Random.insideUnitSphere * spread;
                var go = Instantiate(fishPrefab, leader.position + offset, leader.rotation, transform);

                // On retire la nage aléatoire individuelle : le poisson suit le banc
                var swim = go.GetComponent<M2_FishSwim>();
                if (swim != null) Destroy(swim);

                fish.Add(go.transform);
                offsets.Add(offset);
            }
        }

        // Point aléatoire dans la zone de nage
        Vector3 RandomPoint()
        {
            return new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                Random.Range(areaMin.z, areaMax.z));
        }

        void Update()
        {
            // Le chef avance vers sa destination, puis en choisit une autre
            leader.position = Vector3.MoveTowards(leader.position, target, leaderSpeed * Time.deltaTime);
            if ((target - leader.position).sqrMagnitude < 1f) target = RandomPoint();

            Vector3 leaderDir = (target - leader.position).normalized;

            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i] == null) continue;   // Poisson pêché : on l'ignore

                // Position voulue = chef + place du poisson dans le banc
                Vector3 wanted = leader.position + offsets[i];
                Vector3 pos = fish[i].position;
                Vector3 move = (wanted - pos);

                // Avance en douceur vers sa place
                fish[i].position = Vector3.MoveTowards(pos, wanted, followSpeed * Time.deltaTime);

                // Se tourne dans le sens du déplacement (sinon dans le sens du chef)
                Vector3 dir = move.sqrMagnitude > 0.05f ? move.normalized : leaderDir;
                if (dir != Vector3.zero)
                {
                    Quaternion look = Quaternion.LookRotation(dir);
                    fish[i].rotation = Quaternion.Slerp(fish[i].rotation, look, turnSpeed * Time.deltaTime);
                }
            }
        }
    }
}