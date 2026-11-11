using UnityEngine;
using UnityEngine.AI;

namespace OceanExplorer.Module2
{
    public class M2_BoneWiggle : MonoBehaviour
    {
        public Vector3 legAxis = new Vector3(0, 0, 1);
        public float legAngle = 20f;
        public Vector3 clawAxis = new Vector3(0, 0, 1);
        public float clawAngle = 12f;
        public float idleSpeed = 1.5f;
        public float walkSpeed = 9f;

        NavMeshAgent agent;
        Transform[] bones;
        Quaternion[] startRot;
        float[] phase;
        bool[] isLeg;
        float t;

        void Start()
        {
            agent = GetComponentInParent<NavMeshAgent>();
            var list = new System.Collections.Generic.List<Transform>();
            var ph = new System.Collections.Generic.List<float>();
            var leg = new System.Collections.Generic.List<bool>();

            foreach (Transform b in GetComponentsInChildren<Transform>())
            {
                bool isL = b.name.StartsWith("Leg_");
                bool isC = b.name.StartsWith("Claw");
                if (!isL && !isC) continue;

                float p = b.name.EndsWith(".L") ? 0f : Mathf.PI;
                if (b.name.Contains("Mid")) p += Mathf.PI / 2f;
                list.Add(b);
                ph.Add(p);
                leg.Add(isL);
            }

            bones = list.ToArray();
            phase = ph.ToArray();
            isLeg = leg.ToArray();
            startRot = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                startRot[i] = bones[i].localRotation;
        }

        void Update()
        {
            float v = agent != null ? agent.velocity.magnitude : 0f;
            float k = Mathf.Clamp01(v / 1.5f);
            t += Time.deltaTime * Mathf.Lerp(idleSpeed, walkSpeed, k);
            float amp = Mathf.Lerp(0.3f, 1f, k);

            for (int i = 0; i < bones.Length; i++)
            {
                float angle = Mathf.Sin(t + phase[i]) * (isLeg[i] ? legAngle : clawAngle) * amp;
                Vector3 axis = isLeg[i] ? legAxis : clawAxis;
                bones[i].localRotation = startRot[i] * Quaternion.AngleAxis(angle, axis);
            }
        }
    }
}