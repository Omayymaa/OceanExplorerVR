using System.Collections.Generic;
using UnityEngine;

namespace OceanExplorer.Module2
{
    // Remplit le fond d'une cuvette d'herbes posées sur le terrain, de façon naturelle :
    // - seulement sur le fond (pas sur les parois),
    // - en touffes irrégulières séparées par des plages de sable (bruit de Perlin),
    // - plus hautes que larges.
    public class M2_HerbierScatter : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] prefabs;        // Algues / herbes à placer

        [Header("Zone (centrée sur cet objet)")]
        public float radius = 20f;          // Rayon de recherche
        public float maxHeightAboveZone = 1f; // Hauteur max du sol au-dessus de l'objet (garde le fond)

        [Header("Densité")]
        public int count = 500;             // Nombre d'éléments souhaité
        public float minSpacing = 1f;       // Distance minimale entre deux éléments
        public int seed = 7;                // Graine : même résultat à chaque lancement

        [Header("Touffes et plages de sable")]
        public float patchScale = 0.15f;    // Taille des touffes (petit = grandes touffes)
        [Range(0f, 1f)] public float patchThreshold = 0.45f; // Plus haut = plus de sable nu

        [Header("Taille")]
        public float minScale = 0.6f;       // Largeur minimale (multiplicateur du prefab)
        public float maxScale = 1.0f;       // Largeur maximale
        public float minHeightStretch = 1.2f; // Étirement en hauteur minimal
        public float maxHeightStretch = 2.0f; // Étirement en hauteur maximal
        public float yOffset = -0.2f;       // Enfonce un peu le pied dans le sol

        void Start()
        {
            Scatter();
        }

        void Scatter()
        {
            if (prefabs == null || prefabs.Length == 0) return;

            Random.State old = Random.state;   // Sauvegarde le hasard du reste du jeu
            Random.InitState(seed);

            Vector3 center = transform.position;
            float maxY = center.y + maxHeightAboveZone;
            float offset = seed * 17.3f;       // Décale le bruit selon la graine
            var placed = new List<Vector3>();
            float minSqr = minSpacing * minSpacing;
            int attempts = 0;

            while (placed.Count < count && attempts < count * 40)
            {
                attempts++;

                Vector2 c = Random.insideUnitCircle * radius;
                Vector3 p = center + new Vector3(c.x, 0f, c.y);

                // Hauteur du sol ; hors terrain ou trop haut (paroi) : on refuse
                if (!TryGetGroundHeight(p, out float y)) continue;
                if (y > maxY) continue;

                // Bruit de Perlin : en dessous du seuil, on laisse du sable nu
                float n = Mathf.PerlinNoise(p.x * patchScale + offset, p.z * patchScale + offset);
                if (n < patchThreshold) continue;

                // Trop près d'un élément déjà placé : on refuse
                bool tooClose = false;
                foreach (var q in placed)
                {
                    float dx = p.x - q.x;
                    float dz = p.z - q.z;
                    if (dx * dx + dz * dz < minSqr) { tooClose = true; break; }
                }
                if (tooClose) continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject go = Instantiate(
                    prefab,
                    new Vector3(p.x, y + yOffset, p.z),
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                    transform);

                // Largeur aléatoire, puis hauteur étirée : forme dressée et élancée
                float w = Random.Range(minScale, maxScale);
                float h = w * Random.Range(minHeightStretch, maxHeightStretch);
                Vector3 s = prefab.transform.localScale;
                go.transform.localScale = new Vector3(s.x * w, s.y * h, s.z * w);

                placed.Add(p);
            }

            Random.state = old;   // Restaure le hasard du reste du jeu
        }

        bool TryGetGroundHeight(Vector3 p, out float y)
        {
            foreach (var t in Terrain.activeTerrains)
            {
                Vector3 tp = t.transform.position;
                Vector3 size = t.terrainData.size;
                if (p.x >= tp.x && p.x <= tp.x + size.x && p.z >= tp.z && p.z <= tp.z + size.z)
                {
                    y = t.SampleHeight(p) + tp.y;
                    return true;
                }
            }
            y = 0f;
            return false;
        }
    }
}