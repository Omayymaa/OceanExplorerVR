using UnityEngine;

namespace OceanExplorer.Module2
{
    // Zone de détection : quand le plongeur y entre, elle lance une étape du guide
    [RequireComponent(typeof(Collider))]
    public class M2_GuideZone : MonoBehaviour
    {
        public string stepId = "herbier";   // Étape à jouer
        public bool onlyOnce = true;        // Joue une seule fois
        bool done;                          // Déjà jouée

        // À l'ajout du script : le collider devient une zone (trigger)
        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (done && onlyOnce) return;
            // Seul le plongeur déclenche la zone
            if (other.GetComponentInParent<M2_Diver>() == null) return;

            if (M2_AudioGuide.Instance != null) M2_AudioGuide.Instance.Play(stepId);
            done = true;
        }
    }
}