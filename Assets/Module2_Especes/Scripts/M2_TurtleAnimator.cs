using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    public class M2_TurtleAnimator : MonoBehaviour
    {
        public string speedParameter = "Speed";

        NavMeshAgent agent;
        Animator animator;

        void Start()
        {
            agent = GetComponentInParent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (agent == null || animator == null) return;
            animator.SetFloat(speedParameter, agent.velocity.magnitude);
        }
    }
}