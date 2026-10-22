using UnityEngine;

namespace OceanExplorer.Module2
{
    public class M2_Sway : MonoBehaviour
    {
        public float angle = 8f;
        public float speed = 1f;
        float offset;
        Quaternion start;

        void Start()
        {
            start = transform.rotation;
            offset = Random.Range(0f, 10f);
        }

        void Update()
        {
            float a = Mathf.Sin(Time.time * speed + offset) * angle;
            transform.rotation = start * Quaternion.Euler(a, 0, a);
        }
    }
}