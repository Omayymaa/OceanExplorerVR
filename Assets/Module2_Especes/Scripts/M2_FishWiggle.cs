using System.Collections.Generic;
using UnityEngine;

namespace OceanExplorer.Module2
{
    public class M2_FishWiggle : MonoBehaviour
    {
        public float bodyAngle = 12f;
        public float tailAngle = 25f;
        public float speed = 6f;
        public Vector3 axis = new Vector3(0, 1, 0);

        Transform[] tails;
        Quaternion[] tailStart;
        Quaternion bodyStart;
        float offset;

        void Start()
        {
            offset = Random.Range(0f, 10f);
            bodyStart = transform.localRotation;

            var list = new List<Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLower();
                if (n.Contains("tail") || n.Contains("fin"))
                    list.Add(t);
            }
            tails = list.ToArray();
            tailStart = new Quaternion[tails.Length];
            for (int i = 0; i < tails.Length; i++)
                tailStart[i] = tails[i].localRotation;
        }

        void Update()
        {
            float s = Mathf.Sin(Time.time * speed + offset);

            // Le corps ondule doucement
            transform.localRotation = bodyStart * Quaternion.AngleAxis(s * bodyAngle, axis);

            // La queue et les nageoires ondulent davantage (si le modèle en a)
            for (int i = 0; i < tails.Length; i++)
                tails[i].localRotation = tailStart[i] * Quaternion.AngleAxis(s * tailAngle, axis);
        }
    }
}