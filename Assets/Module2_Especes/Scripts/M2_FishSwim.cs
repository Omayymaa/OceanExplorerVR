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

            // Calcule la rotation nécessaire pour orienter l'objet
            // dans la direction du vecteur dir.
            Quaternion look = Quaternion.LookRotation(dir);

            // Effectue une rotation progressive et fluide de l'objet
            // depuis sa rotation actuelle vers la rotation cible look.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                look,
                turnSpeed * Time.deltaTime
            );

            // Déplace l'objet vers l'avant selon son orientation actuelle,
            // à une vitesse speed, indépendamment du nombre de FPS.
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