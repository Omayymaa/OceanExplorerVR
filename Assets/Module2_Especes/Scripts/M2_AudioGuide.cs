using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OceanExplorer.Module2
{
    // Une étape du guide : un identifiant, un clip audio et un sous-titre
    [System.Serializable]
    public class GuideStep
    {
        public string id;                  // Nom de l'étape (ex : "welcome", "herbier")
        public AudioClip clip;             // Voix du guide (facultative)
        [TextArea] public string subtitle; // Texte affiché à l'écran
    }

    // Audio guide : joue une voix et affiche son sous-titre. Touche G : couper / réactiver.
    public class M2_AudioGuide : MonoBehaviour
    {
        public static M2_AudioGuide Instance;   // Accès facile depuis les autres scripts

        public GuideStep[] steps;               // Toutes les étapes du guide
        public TMP_Text subtitleText;           // Texte des sous-titres (Canvas)
        [Range(0f, 1f)] public float volume = 1f;
        public bool playWelcomeOnStart = true;  // Joue "welcome" au lancement

        AudioSource source;                     // Lecteur audio du guide
        bool guideOn = true;                    // Guide actif ou coupé
        Coroutine subtitleRoutine;              // Pour effacer le sous-titre
        readonly HashSet<string> playedOnce = new HashSet<string>();   // Étapes déjà jouées via PlayOnce

        void Awake() { Instance = this; }

        void Start()
        {
            // Lecteur audio 2D (même volume partout)
            source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.playOnAwake = false;
            source.volume = volume;

            if (subtitleText != null) subtitleText.text = "";
            if (playWelcomeOnStart) Play("welcome");
        }

        void Update()
        {
            // Touche G : coupe ou réactive le guide
            var kb = Keyboard.current;
            if (kb != null && kb.gKey.wasPressedThisFrame)
            {
                guideOn = !guideOn;
                if (!guideOn) Stop();
            }
        }

        // Joue l'étape portant cet identifiant
        public void Play(string id)
        {
            if (!guideOn) return;
            foreach (var s in steps)
            {
                if (s.id != id) continue;

                Stop();   // Arrête la voix précédente
                if (s.clip != null)
                {
                    source.clip = s.clip;
                    source.volume = volume;
                    source.Play();
                }
                if (subtitleText != null)
                {
                    subtitleText.text = s.subtitle;
                    // Durée d'affichage = durée de l'audio, sinon estimation selon la longueur du texte
                    float t = s.clip != null ? s.clip.length : 3f + s.subtitle.Length * 0.05f;
                    subtitleRoutine = StartCoroutine(HideAfter(t + 0.5f));
                }
                return;
            }
        }

        // Joue l'étape une seule fois (par exemple le message de la première prise)
        public void PlayOnce(string id)
        {
            if (playedOnce.Contains(id)) return;
            playedOnce.Add(id);
            Play(id);
        }

        // Arrête la voix et efface le sous-titre
        public void Stop()
        {
            if (source != null) source.Stop();
            if (subtitleRoutine != null) StopCoroutine(subtitleRoutine);
            if (subtitleText != null) subtitleText.text = "";
        }

        IEnumerator HideAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (subtitleText != null) subtitleText.text = "";
        }
    }
}