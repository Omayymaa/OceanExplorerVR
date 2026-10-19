using UnityEngine;

namespace OceanExplorer.Common
{
    public class SpeciesData : MonoBehaviour
    {
        public string nom = "Espèce";
        public float taille = 10f;
        public bool protege = false;
        [TextArea] public string description = "";
    }
}