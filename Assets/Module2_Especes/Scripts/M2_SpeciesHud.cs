using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OceanExplorer.Module2
{
    // Affiche le nombre de poissons, de crabes et de tortues (texte simple en haut de l'écran)
    public class M2_SpeciesHud : MonoBehaviour
    {
        public TMP_Text countText;          // Texte du compteur (CountText)
        public M2_SpeciesSpawner spawner;   // Pour relancer la génération avec la touche R
        float timer;                        // Compte le temps entre 2 mises à jour

        void Update()
        {
            // Met à jour les nombres toutes les 0,5 seconde
            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                timer = 0f;
                int fish = GameObject.FindGameObjectsWithTag("Fish").Length;        // Poissons
                int others = GameObject.FindGameObjectsWithTag("Species").Length;   // Crabes et tortues

                // Texte simple : titre + nombre sur deux lignes
                countText.text = "Poissons : " + fish + "\nCrabes et tortues : " + others;
            }

            // Touche R : supprime les animaux et les regénère
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                foreach (Transform child in spawner.transform)
                    Destroy(child.gameObject);
                spawner.Respawn();
            }
        }
    }
}