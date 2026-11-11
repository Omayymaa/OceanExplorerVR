
using UnityEngine;

namespace OceanExplorer.Module2
{
    // Script qui permet à la caméra de suivre un personnage
    // de manière fluide.
    public class M2_CameraFollow : MonoBehaviour
    {
        // Le personnage que la caméra doit suivre.
        public Transform target;

        // Décalage entre la position du personnage et celle de la caméra.
        // X = 0 : pas de décalage horizontal.
        // Y = 6 : caméra placée 6 unités plus haut.
        // Z = -12 : caméra placée 12 unités derrière (selon les axes du monde).
        public Vector3 offset = new Vector3(0, 6, -12);

        // Contrôle la rapidité avec laquelle la caméra rejoint
        // sa position souhaitée.
        public float smooth = 5f;

        // LateUpdate est exécutée après Update().
        // Elle est adaptée au suivi d'un personnage qui se déplace.
        void LateUpdate()
        {
            // Si aucune cible n'est assignée, on arrête l'exécution
            // de cette méthode pour éviter une erreur.
            if (target == null) return;

            // Calcule la position souhaitée de la caméra
            // en ajoutant le décalage à la position du personnage.
            Vector3 wanted = target.position + offset;

            // Déplace progressivement la caméra de sa position actuelle
            // vers la position souhaitée pour obtenir un mouvement fluide.
            transform.position = Vector3.Lerp(
                transform.position,
                wanted,
                smooth * Time.deltaTime
            );

            // Oriente la caméra vers un point situé une unité
            // au-dessus du personnage, pour cadrer son corps.
            transform.LookAt(target.position + Vector3.up);
        }
    }
}
