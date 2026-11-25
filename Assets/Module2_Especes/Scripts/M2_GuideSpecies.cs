using System.Collections.Generic;
using UnityEngine;
using OceanExplorer.Common;

namespace OceanExplorer.Module2
{
    // Lance une étape du guide quand le plongeur rencontre une espèce.
    // À poser sur le Diver (il reçoit les mêmes événements de collision que M2_Diver).
    public class M2_GuideSpecies : MonoBehaviour
    {
        // Une règle : si le nom de l'espèce contient un mot, on joue cette étape
        [System.Serializable]
        public class Rule
        {
            public string nameContains;   // Mot cherché dans le nom (ex : "hippocampe")
            public string stepId;         // Étape du guide à jouer (ex : "hippocampe")
        }

        public Rule[] rules;                                   // Liste des règles
        readonly HashSet<string> played = new HashSet<string>(); // Étapes déjà jouées (une seule fois)

        void OnTriggerEnter(Collider other)
        {
            // Retrouve la fiche de l'espèce touchée
            SpeciesData data = other.GetComponentInParent<SpeciesData>();
            if (data == null || string.IsNullOrEmpty(data.nom)) return;

            foreach (var r in rules)
            {
                if (played.Contains(r.stepId)) continue;
                if (data.nom.ToLower().Contains(r.nameContains.ToLower()))
                {
                    if (M2_AudioGuide.Instance != null) M2_AudioGuide.Instance.Play(r.stepId);
                    played.Add(r.stepId);   // Ne rejoue pas ce message
                    break;
                }
            }
        }
    }
}