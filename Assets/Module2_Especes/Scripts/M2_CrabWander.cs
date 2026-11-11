
using UnityEngine;
using UnityEngine.AI; // Permet d'utiliser le système de navigation NavMesh

namespace OceanExplorer.Module2
{
    // Ajoute automatiquement un NavMeshAgent au crabe
    // si ce composant n'existe pas déjà.
    [RequireComponent(typeof(NavMeshAgent))]
    public class M2_CrabWander : MonoBehaviour
    {
        // Distance maximale approximative pour choisir une destination autour du crabe.
        public float wanderRadius = 12f;

        // Temps d'attente avant de choisir une nouvelle destination.
        public float waitTime = 2f;

        // Limites minimales de déplacement sur les axes X et Z.
        public Vector2 minXZ = new Vector2(5, 5);

        // Limites maximales de déplacement sur les axes X et Z.
        public Vector2 maxXZ = new Vector2(95, 95);

        NavMeshAgent agent; // Gère le déplacement automatique du crabe.
        Terrain terrain;    // Référence au terrain de la scène.
        float timer;        // Compteur du temps d'attente.

        void Start()
        {
            // Récupère le NavMeshAgent attaché au crabe.
            agent = GetComponent<NavMeshAgent>();

            // Récupère le terrain actif de la scène.
            terrain = Terrain.activeTerrain;

            // Initialise le compteur avec le temps d'attente.
            timer = waitTime;
        }

        void Update()
        {
            // Vérifie que le crabe se trouve sur une surface NavMesh.
            if (!agent.isOnNavMesh) return;

            // Vérifie que le calcul du chemin est terminé
            // et que le crabe est arrivé à destination.
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                // Incrémente le compteur à chaque frame.
                timer += Time.deltaTime;

                // Si le temps d'attente est écoulé,
                // cherche une nouvelle destination.
                if (timer >= waitTime)
                {
                    PickDestination();

                    // Réinitialise le compteur.
                    timer = 0f;
                }
            }
        }

        void PickDestination()
        {
            // Essaie jusqu'à 10 positions aléatoires.
            for (int i = 0; i < 10; i++)
            {
                // Choisit un point aléatoire autour du crabe.
                Vector3 p = transform.position
                    + Random.insideUnitSphere * wanderRadius;

                // Limite la coordonnée X aux bornes autorisées.
                p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);

                // Limite la coordonnée Z aux bornes autorisées.
                p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);

                // Calcule la hauteur du terrain au point choisi.
                // Si aucun terrain n'existe, utilise la hauteur 10.
                p.y = terrain != null
                    ? terrain.SampleHeight(p) + terrain.transform.position.y
                    : 10f;

                // Recherche une position valide sur le NavMesh
                // dans un rayon de 3 unités autour du point choisi.
                if (NavMesh.SamplePosition(
                    p, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                {
                    // Demande au crabe de se déplacer vers cette position.
                    agent.SetDestination(hit.position);

                    // Arrête les essais dès qu'une position valide est trouvée.
                    return;
                }
            }
        }
    }
}
