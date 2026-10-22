using UnityEngine;

namespace OceanExplorer.Module2
{
    public class M2_CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 6, -12);
        public float smooth = 5f;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 wanted = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, wanted, smooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up);
        }
    }
}