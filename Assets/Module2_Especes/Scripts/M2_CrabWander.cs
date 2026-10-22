using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class M2_CrabWander : MonoBehaviour
    {
        public float wanderRadius = 12f;
        public float waitTime = 2f;
        public Vector2 minXZ = new Vector2(5, 5);
        public Vector2 maxXZ = new Vector2(95, 95);

        NavMeshAgent agent;
        Terrain terrain;
        float timer;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            terrain = Terrain.activeTerrain;
            timer = waitTime;
        }

        void Update()
        {
            if (!agent.isOnNavMesh) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                timer += Time.deltaTime;
                if (timer >= waitTime)
                {
                    PickDestination();
                    timer = 0f;
                }
            }
        }

        void PickDestination()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 p = transform.position + Random.insideUnitSphere * wanderRadius;
                p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);
                p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);
                p.y = terrain != null
                    ? terrain.SampleHeight(p) + terrain.transform.position.y
                    : 10f;

                if (NavMesh.SamplePosition(p, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }
    }
}