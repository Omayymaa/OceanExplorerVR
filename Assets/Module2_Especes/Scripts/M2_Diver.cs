using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using OceanExplorer.Common;

namespace OceanExplorer.Module2
{
    [RequireComponent(typeof(Rigidbody))]
    public class M2_Diver : MonoBehaviour
    {
        [Header("Déplacement")]
        public float speed = 4f;
        public float minX = 5f;
        public float maxX = 95f;
        public float minY = 10.5f;
        public float maxY = 30f;
        public float minZ = 5f;
        public float maxZ = 95f;

        [Header("Caméra")]
        public Transform cameraTransform;
        public float mouseSensitivity = 0.15f;
        public Vector3 fpsPosition = new Vector3(0f, 0.3f, 1.3f);
        public Vector3 tpsPosition = new Vector3(0f, 2.5f, -5f);
        public bool fpsMode = true;

        // [VR] Réglages du mode VR (XR Origin enfant du Diver)
        [Header("VR (XR Origin)")]
        [Tooltip("Coché : la caméra est pilotée par le suivi VR/simulateur.")]
        public bool vrMode = true;
        [Tooltip("XR Origin (XR Rig), enfant du Diver. Auto-détecté si vide.")]
        public Transform xrRig;
        [Tooltip("Position locale du rig dans le Diver (règle la hauteur de la tête).")]
        public Vector3 vrRigPosition = new Vector3(0f, -1.06f, 1.3f);
        [Tooltip("Optionnel : action Move du joystick gauche (XRI Left Locomotion/Move).")]
        public InputActionReference moveAction;

        [Header("Panneau d'information")]
        public GameObject infoPanel;
        public TMP_Text infoText;
        public float displayTime = 4f;

        Rigidbody rb;
        float yaw;
        float pitch;
        Vector3 inputDir;
        Renderer[] bodyRenderers;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.freezeRotation = true;

            // [VR] Retrouver le rig s'il n'est pas assigné
            if (xrRig == null) xrRig = transform.Find("XR Origin (XR Rig)");

            // Caméra : MainCamera (celle du rig en mode VR)
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (vrMode)
            {
                // [VR] On NE déplace PAS la caméra : le suivi la pilote.
                // On place seulement le rig (hauteur de la tête) dans le Diver.
                if (xrRig != null) xrRig.localPosition = vrRigPosition;
            }
            else if (cameraTransform != null)
            {
                // Mode classique (sans VR) : comme avant
                cameraTransform.SetParent(transform);
                var follow = cameraTransform.GetComponent<M2_CameraFollow>();
                if (follow != null) follow.enabled = false;
            }

            // Renderers du corps du plongeur, SANS ceux du XR Rig (contrôleurs) ni du harpon
            var fishGun = GetComponent<M2_FishGun>();   // [VR] pour connaître le harpon
            var list = new List<Renderer>();
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject == gameObject) continue;
                if (xrRig != null && r.transform.IsChildOf(xrRig)) continue;           // [VR] rig
                if (fishGun != null && fishGun.gun != null
                    && r.transform.IsChildOf(fishGun.gun)) continue;                   // [VR] harpon
                list.Add(r);
            }
            bodyRenderers = list.ToArray();

            yaw = transform.eulerAngles.y;
            ApplyView();

            if (infoPanel != null) infoPanel.SetActive(false);

            // [VR] En VR/simulateur, on laisse le simulateur gérer la souris
            if (!vrMode)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // Les entrées se lisent dans Update
        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // [VR] Touche C et souris : uniquement hors VR (sinon conflit avec le suivi)
            if (!vrMode)
            {
                if (kb.cKey.wasPressedThisFrame)
                {
                    fpsMode = !fpsMode;
                    ApplyView();
                }

                if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                {
                    Vector2 d = mouse.delta.ReadValue() * mouseSensitivity;
                    yaw += d.x;
                    pitch = Mathf.Clamp(pitch - d.y, -80f, 80f);
                    if (cameraTransform != null)
                        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                }
            }

            float x, z, y;
            if (vrMode)
            {
                // [VR] Le simulateur utilise flèches et WASD/QE : on déplace le plongeur
                // avec I/K (avant/arrière), J/L (gauche/droite), U/O (haut/bas)
                x = (kb.lKey.isPressed ? 1 : 0) - (kb.jKey.isPressed ? 1 : 0);
                z = (kb.iKey.isPressed ? 1 : 0) - (kb.kKey.isPressed ? 1 : 0);
                y = (kb.uKey.isPressed ? 1 : 0) - (kb.oKey.isPressed ? 1 : 0);

                // [VR] Joystick gauche (action Move) : s'ajoute aux touches
                if (moveAction != null && moveAction.action != null)
                {
                    Vector2 stick = moveAction.action.ReadValue<Vector2>();
                    x += stick.x;
                    z += stick.y;
                }
            }
            else
            {
                x = (kb.rightArrowKey.isPressed ? 1 : 0) - (kb.leftArrowKey.isPressed ? 1 : 0);
                z = (kb.upArrowKey.isPressed ? 1 : 0) - (kb.downArrowKey.isPressed ? 1 : 0);
                y = (kb.spaceKey.isPressed ? 1 : 0) - (kb.leftShiftKey.isPressed ? 1 : 0);
            }

            // On nage dans la direction où la tête regarde
            Transform view = cameraTransform != null ? cameraTransform : transform;
            inputDir = view.forward * z + view.right * x + Vector3.up * y;
            if (vrMode) inputDir = Vector3.ClampMagnitude(inputDir, 1f); // [VR] vitesse constante
        }

        // La physique se calcule dans FixedUpdate
        void FixedUpdate()
        {
            rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
            rb.linearVelocity = inputDir * speed;

            Vector3 p = rb.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.y = Mathf.Clamp(p.y, minY, maxY);
            p.z = Mathf.Clamp(p.z, minZ, maxZ);
            if (p != rb.position) rb.position = p;
        }

        void ApplyView()
        {
            // [VR] En VR on cache le corps (vue à la première personne) et on ne touche pas à la caméra
            if (vrMode)
            {
                if (bodyRenderers != null)
                    foreach (var r in bodyRenderers) r.enabled = false;
                return;
            }

            if (cameraTransform != null)
                cameraTransform.localPosition = fpsMode ? fpsPosition : tpsPosition;

            if (bodyRenderers != null)
                foreach (var r in bodyRenderers)
                    r.enabled = !fpsMode;
        }

        void OnTriggerEnter(Collider other)
        {
            SpeciesData data = other.GetComponentInParent<SpeciesData>();
            if (data == null) return;
            if (infoPanel == null || infoText == null) return;

            infoText.text = data.nom + "\nTaille : " + data.taille + " cm\nEspèce protégée : " + (data.protege ? "oui" : "non");
            StopAllCoroutines();
            StartCoroutine(ShowPanel());

            if (SimulationManager.Instance != null)
                SimulationManager.Instance.SpeciesObserved(data.nom);
        }

        IEnumerator ShowPanel()
        {
            infoPanel.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            infoPanel.SetActive(false);
        }
    }
}