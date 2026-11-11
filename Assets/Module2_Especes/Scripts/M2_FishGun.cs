using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using OceanExplorer.Common;

namespace OceanExplorer.Module2
{
    // Pistolet de pêche sous-marine : visée, tir, capture et effets (trait + son + bulles)
    public class M2_FishGun : MonoBehaviour
    {
        [Header("Visée")]
        public Camera cam;                 // Caméra du plongeur (Main Camera)
        public Transform gun;              // Le modèle du pistolet (Harpoon_Low)
        public RectTransform crosshair;    // Le point blanc affiché à l'écran
        public float range = 30f;          // Portée maximale du tir (en mètres)

        [Header("Interface")]
        public TMP_Text messageText;       // Message de tir (Pêché, Raté...)
        public TMP_Text caughtText;        // Compteur "Pêchés : N"
        public float messageTime = 2f;     // Durée d'affichage du message (secondes)

        [Header("Style du message")]
        public Color caughtColor = new Color(0.45f, 1f, 0.55f);      // Vert : pêche réussie
        public Color protectedColor = new Color(1f, 0.4f, 0.35f);    // Rouge : espèce protégée
        public Color notCatchableColor = new Color(1f, 0.8f, 0.35f); // Orange : non pêchable
        public Color missColor = new Color(0.75f, 0.85f, 0.95f);     // Gris-bleu : raté
        public float popScale = 1.25f;     // Taille de départ du message (effet d'apparition)
        public float popTime = 0.15f;      // Durée de l'effet d'apparition (secondes)
        public float fadeTime = 0.5f;      // Durée du fondu avant disparition (secondes)

        [Header("Effet de tir")]
        public LineRenderer tracer;        // Le trait du harpon
        public float tracerTime = 0.12f;   // Durée d'affichage du trait
        public float tipDistance = 0.2f;   // Distance entre le centre du pistolet et sa pointe
        public ParticleSystem impactBubbles; // Bulles qui apparaissent au point d'impact

        [Header("Son (laisser vide = son généré)")]
        public AudioClip shotSound;        // Son de tir (facultatif)
        [Range(0f, 1f)] public float volume = 0.8f;

        int caught;                        // Nombre de poissons pêchés
        float clearTime;                   // Moment où effacer le message
        float messageStart;                // Moment où le message est apparu
        Color messageColor = Color.white;  // Couleur du message en cours
        Vector3 messageBaseScale = Vector3.one; // Taille normale du texte du message
        float tracerOff;                   // Moment où éteindre le trait
        AudioSource audioSource;           // Lecteur du son de tir

        void Start()
        {
            if (cam == null) cam = Camera.main;
            UpdateCounter();
            if (messageText != null)
            {
                messageText.text = "";
                messageBaseScale = messageText.transform.localScale;   // Mémorise la taille normale
            }
            if (tracer != null) tracer.enabled = false;   // Le trait est éteint au départ

            // Crée le lecteur audio (son 2D, pas de lecture automatique)
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;

            // Si aucun son n'est fourni, on en génère un par le code
            if (shotSound == null) shotSound = MakeShotSound();
        }

        // Génère un son de tir réaliste sous l'eau : choc sourd + souffle + bulles, le tout étouffé
        AudioClip MakeShotSound()
        {
            int rate = 44100;                       // Fréquence d'échantillonnage
            float length = 0.9f;                    // Durée du son (secondes)
            int n = (int)(rate * length);           // Nombre d'échantillons
            float[] data = new float[n];
            System.Random rnd = new System.Random(11);

            // --- Bulles : petits "bloup" qui montent en fréquence, à des moments aléatoires ---
            int bubbleCount = 7;
            float[] bubbleStart = new float[bubbleCount];   // Début de chaque bulle (secondes)
            float[] bubbleFreq = new float[bubbleCount];    // Fréquence de départ de chaque bulle
            for (int b = 0; b < bubbleCount; b++)
            {
                bubbleStart[b] = 0.03f + (float)rnd.NextDouble() * 0.5f;
                bubbleFreq[b] = 250f + (float)rnd.NextDouble() * 500f;
            }

            float soft = 0f;                        // Mémoire du filtre du souffle
            float muffled = 0f;                     // Mémoire du filtre final (effet "sous l'eau")

            for (int i = 0; i < n; i++)
            {
                float time = (float)i / rate;       // Temps écoulé (secondes)

                // 1) Choc sourd : note grave qui descend vite (120 Hz -> 40 Hz)
                float thudEnv = Mathf.Exp(-time * 14f);
                float thudFreq = Mathf.Lerp(120f, 40f, Mathf.Clamp01(time / 0.25f));
                float thud = Mathf.Sin(2f * Mathf.PI * thudFreq * time) * thudEnv * 0.9f;

                // 2) Souffle de l'air comprimé : bruit très filtré qui s'éteint doucement
                float noise = (float)(rnd.NextDouble() * 2.0 - 1.0);
                soft += (noise - soft) * 0.06f;
                float hiss = soft * Mathf.Exp(-time * 5f) * 1.6f;

                // 3) Bulles : chaque bulle est un petit son qui monte en fréquence
                float bubbles = 0f;
                for (int b = 0; b < bubbleCount; b++)
                {
                    float bt = time - bubbleStart[b];             // Temps depuis le début de la bulle
                    if (bt > 0f && bt < 0.06f)
                    {
                        float f = bubbleFreq[b] * (1f + bt * 18f);        // La bulle monte en fréquence
                        float env = Mathf.Sin(Mathf.PI * bt / 0.06f);     // Apparaît puis disparaît
                        bubbles += Mathf.Sin(2f * Mathf.PI * f * bt) * env * 0.25f;
                    }
                }

                // 4) Mélange, puis filtre passe-bas fort : l'eau étouffe les aigus
                float mix = thud + hiss + bubbles;
                muffled += (mix - muffled) * 0.18f;
                data[i] = muffled;
            }

            var clip = AudioClip.Create("ShotSound", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Position de la pointe du pistolet (départ du trait)
        Vector3 GunTip()
        {
            return gun != null
                ? gun.position + gun.forward * tipDistance
                : cam.transform.position;
        }

        // Lance un rayon dans la direction de visée.
        // Retourne true si quelque chose est touché (hit) et donne le point visé (aimPoint).
        bool Aim(out RaycastHit hit, out Vector3 aimPoint)
        {
            Vector3 origin = gun != null ? gun.position : cam.transform.position;
            Vector3 dir = cam.transform.forward;

            // On ignore les colliders du plongeur lui-même, puis on garde le plus proche
            var hits = Physics.RaycastAll(origin, dir, range, ~0, QueryTriggerInteraction.Collide)
                              .Where(h => !h.collider.transform.IsChildOf(transform))
                              .OrderBy(h => h.distance).ToArray();
            if (hits.Length > 0)
            {
                hit = hits[0];
                aimPoint = hit.point;
                return true;
            }
            hit = default;
            aimPoint = origin + dir * range;   // Rien touché : point à la portée maximale
            return false;
        }

        // Fait apparaître les bulles au point d'impact
        void PlayImpactBubbles(Vector3 point)
        {
            if (impactBubbles == null) return;
            impactBubbles.transform.position = point;   // Place les bulles sur le point visé
            impactBubbles.Clear();                      // Efface les anciennes bulles
            impactBubbles.Play();                       // Lance l'éclatement de bulles
        }

        // Anime le message : petit effet de zoom à l'apparition, puis fondu avant l'effacement
        void AnimateMessage()
        {
            if (messageText == null || clearTime <= 0f) return;

            float age = Time.time - messageStart;                 // Temps depuis l'apparition
            float k = Mathf.Clamp01(age / popTime);               // 0 -> 1 pendant l'effet de zoom
            messageText.transform.localScale = messageBaseScale * Mathf.Lerp(popScale, 1f, k);

            float remaining = clearTime - Time.time;              // Temps avant l'effacement
            float alpha = Mathf.Clamp01(remaining / fadeTime);    // 1 -> 0 pendant le fondu
            Color c = messageColor;
            c.a = alpha;
            messageText.color = c;
        }

        void Update()
        {
            // Anime le message (zoom + fondu)
            AnimateMessage();

            // Efface le message une fois le délai écoulé
            if (messageText != null && clearTime > 0f && Time.time > clearTime)
            {
                messageText.text = "";
                messageText.transform.localScale = messageBaseScale;   // Remet la taille normale
                clearTime = 0f;
            }

            // Éteint le trait une fois son délai écoulé
            if (tracer != null && tracer.enabled && Time.time > tracerOff)
                tracer.enabled = false;

            // Calcule la visée à chaque image
            bool found = Aim(out RaycastHit hit, out Vector3 aimPoint);

            // Place le point blanc à l'endroit visé par le pistolet
            if (crosshair != null)
            {
                Vector3 sp = cam.WorldToScreenPoint(aimPoint);
                if (sp.z > 0f) crosshair.position = sp;
            }

            // Tir au clic gauche
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            // Joue le son de tir
            if (audioSource != null && shotSound != null)
                audioSource.PlayOneShot(shotSound, volume);

            // Affiche le trait entre la pointe du pistolet et la cible
            if (tracer != null)
            {
                tracer.SetPosition(0, GunTip());
                tracer.SetPosition(1, aimPoint);
                tracer.enabled = true;
                tracerOff = Time.time + tracerTime;
            }

            // Bulles à l'endroit du clic (point d'impact)
            PlayImpactBubbles(aimPoint);

            // Si on a touché un animal, on applique les règles de pêche
            if (found)
            {
                var data = hit.collider.GetComponentInParent<SpeciesData>();
                if (data != null)
                {
                    // Seuls les poissons (tag "Fish") sont pêchables
                    if (!data.CompareTag("Fish"))
                    {
                        Show("<b>" + data.nom + " : non pêchable</b>\n<size=70%>Ce n'est pas une cible de pêche</size>",
                             notCatchableColor);
                        return;
                    }

                    // Pêche sélective : une espèce protégée ne peut pas être pêchée
                    if (data.protege)
                    {
                        Show("<b>Espèce protégée : " + data.nom + "</b>\n<size=70%>Pêche interdite, l'animal est épargné</size>",
                             protectedColor);
                        return;
                    }

                    // Capture : le poisson disparaît et le compteur augmente
                    string nom = data.nom;
                    float taille = data.taille;
                    Destroy(data.gameObject);
                    caught++;
                    UpdateCounter();
                    Show("<b>Pêché : " + nom + "</b>\n<size=70%>Taille : " + taille + " cm - pêche sélective réussie</size>",
                         caughtColor);
                    return;
                }
            }
            Show("<b>Raté</b>\n<size=70%>Visez le poisson et tirez</size>", missColor);
        }

        // Met à jour le texte du compteur (texte simple : "Pêchés : N")
        void UpdateCounter()
        {
            if (caughtText != null) caughtText.text = "Pêchés : " + caught;
        }

        // Affiche un message temporaire, coloré, avec effet de zoom puis fondu
        void Show(string msg, Color color)
        {
            if (messageText == null) return;
            messageText.text = msg;
            messageColor = color;                    // Couleur selon le résultat
            messageStart = Time.time;                // Démarre l'effet de zoom
            clearTime = Time.time + messageTime;     // Moment de l'effacement
            AnimateMessage();                        // Applique tout de suite zoom et couleur
        }
    }
}