using UnityEngine;
using TMPro;
using OceanExplorer.Common;

namespace OceanExplorer.Module2
{
    // Affiche une étiquette 3D flottante au-dessus de l'espèce visée par la caméra.
    // L'étiquette est créée par le code : aucun prefab à préparer.
    public class M2_SpeciesLabel : MonoBehaviour
    {
        [Header("Références")]
        public Transform cam;                 // Main Camera du Diver

        [Header("Détection")]
        public float range = 15f;             // distance max pour afficher l'étiquette
        public float hideDelay = 0.3f;        // délai avant de cacher (évite le clignotement)

        [Header("Apparence")]
        public float fontSize = 3f;           // taille du texte
        public float sizePerMeter = 0.08f;    // taille à l'écran constante, quelle que soit la distance
        public float heightOffset = 0.4f;     // hauteur de l'étiquette au-dessus de l'animal (m)

        [Header("Couleurs du statut")]
        public Color catchableColor = new Color(0.35f, 1f, 0.5f);   // vert : pêchable
        public Color protectedColor = new Color(1f, 0.35f, 0.3f);   // rouge : protégée
        public Color otherColor = new Color(1f, 0.8f, 0.3f);        // orange : non pêchable

        TextMeshPro label;                    // texte 3D créé dans Start
        SpeciesData current;                  // espèce actuellement affichée
        float lastSeen;                       // dernier instant où une espèce était visée

        void Start()
        {
            // Création du texte 3D (objet indépendant du Diver)
            GameObject go = new GameObject("SpeciesLabel3D");
            label = go.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.richText = true;
            label.textWrappingMode = TextWrappingModes.NoWrap;   // CORRIGÉ : pas de retour à la ligne auto
            label.outlineWidth = 0.12f;                          // contour plus fin = texte plus net
            label.fontStyle = FontStyles.Bold;                   // texte en gras, plus lisible
            label.outlineColor = new Color32(0, 20, 40, 255);
            go.SetActive(false);
        }

        void Update()
        {
            if (cam == null || label == null) return;

            SpeciesData found = FindTarget();

            if (found != null)
            {
                current = found;
                lastSeen = Time.time;
                label.text = BuildText(found);
                if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
            }
            else if (Time.time - lastSeen > hideDelay)
            {
                current = null;
                label.gameObject.SetActive(false);
            }

            if (current != null) PlaceLabel();
        }

        // Cherche l'espèce visée au centre de l'écran (la plus proche, hors Diver)
        SpeciesData FindTarget()
        {
            RaycastHit[] hits = Physics.RaycastAll(cam.position, cam.forward, range,
                                                   ~0, QueryTriggerInteraction.Collide);
            float best = float.MaxValue;
            SpeciesData result = null;
            foreach (var h in hits)
            {
                if (h.transform.IsChildOf(transform)) continue;       // ignore le Diver
                SpeciesData d = h.collider.GetComponentInParent<SpeciesData>();
                if (d != null && h.distance < best) { best = h.distance; result = d; }
            }
            return result;
        }

        // Construit le texte : nom, taille, statut en couleur, description
        string BuildText(SpeciesData d)
        {
            string statut; Color c;
            if (d.protege) { statut = "ESPÈCE PROTÉGÉE"; c = protectedColor; }
            else if (d.CompareTag("Fish")) { statut = "PÊCHABLE"; c = catchableColor; }
            else { statut = "NON PÊCHABLE"; c = otherColor; }

            string hex = ColorUtility.ToHtmlStringRGB(c);
            string s = "<b>" + d.nom + "</b>\n";
            s += "Taille : " + d.taille + " cm\n";
            s += "<color=#" + hex + "><b>" + statut + "</b></color>";
            if (!string.IsNullOrEmpty(d.description))
                s += "\n<size=70%>" + d.description + "</size>";
            return s;
        }

        // Place l'étiquette au-dessus de l'animal et la tourne vers la caméra
        void PlaceLabel()
        {
            // Hauteur du haut de l'animal (somme des Renderers)
            Renderer[] rs = current.GetComponentsInChildren<Renderer>();
            Bounds b = new Bounds(current.transform.position, Vector3.zero);
            foreach (var r in rs) b.Encapsulate(r.bounds);

            Vector3 pos = new Vector3(b.center.x, b.max.y + heightOffset, b.center.z);
            label.transform.position = pos;
            label.transform.rotation = cam.rotation;                 // billboard
            float dist = Vector3.Distance(cam.position, pos);
            label.transform.localScale = Vector3.one * dist * sizePerMeter;   // taille constante
        }
    }
}