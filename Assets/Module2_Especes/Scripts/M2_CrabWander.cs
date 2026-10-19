using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class M2_CrabWander : MonoBehaviour
    {
        public float wanderRadius = 15f;
        public float waitTime = 2f;

        NavMeshAgent agent;
        float timer;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            timer = waitTime;
        }

        void Update()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                timer += Time.deltaTime;
                if (timer >= waitTime)
                {
                    Vector3 random = transform.position + Random.insideUnitSphere * wanderRadius;
                    if (NavMesh.SamplePosition(random, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                        agent.SetDestination(hit.position);
                    timer = 0f;
                }
            }
        }
    }
}