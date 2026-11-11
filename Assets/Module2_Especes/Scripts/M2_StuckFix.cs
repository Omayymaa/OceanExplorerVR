using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    // Débloque un animal NavMesh coincé (contre un rocher par exemple) :
    // s'il ne bouge presque plus pendant un moment, on lui donne une nouvelle destination.
    [RequireComponent(typeof(NavMeshAgent))]
    public class M2_StuckFix : MonoBehaviour
    {
        public float stuckTime = 2f;        // Durée d'immobilité avant de réagir (secondes)
        public float minSpeed = 0.05f;      // Vitesse en dessous de laquelle on est "bloqué"
        public float searchRadius = 15f;    // Distance de la nouvelle destination

        NavMeshAgent agent;                 // Agent de l'animal
        float timer;                        // Temps passé immobile

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (!agent.isOnNavMesh) return;

            // Vitesse réelle de l'agent ; si trop faible, on compte le temps
            if (agent.velocity.magnitude < minSpeed) timer += Time.deltaTime;
            else timer = 0f;

            // Bloqué trop longtemps : nouvelle destination
            if (timer >= stuckTime)
            {
                timer = 0f;
                Vector3 random = transform.position + Random.insideUnitSphere * searchRadius;
                if (NavMesh.SamplePosition(random, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
        }
    }
}