using UnityEngine;

namespace OceanExplorer.Module2
{
    public class M2_FishSwim : MonoBehaviour
    {
        public float speed = 3f;
        public float turnSpeed = 2f;
        public Vector3 areaMin = new Vector3(10, 15, 10);
        public Vector3 areaMax = new Vector3(90, 25, 90);

        Vector3 target;

        void Start()
        {
            PickTarget();
        }

        void Update()
        {
            Vector3 dir = target - transform.position;
            if (dir.magnitude < 1f)
            {
                PickTarget();
                return;
            }

            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        void PickTarget()
        {
            target = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                Random.Range(areaMin.z, areaMax.z));
        }
    }
}