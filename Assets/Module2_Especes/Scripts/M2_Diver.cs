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

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (cameraTransform != null)
            {
                cameraTransform.SetParent(transform);
                var follow = cameraTransform.GetComponent<M2_CameraFollow>();
                if (follow != null) follow.enabled = false;
            }

            var list = new List<Renderer>();
            foreach (var r in GetComponentsInChildren<Renderer>())
                if (r.gameObject != gameObject) list.Add(r);
            bodyRenderers = list.ToArray();

            yaw = transform.eulerAngles.y;
            ApplyView();

            if (infoPanel != null) infoPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

            float x = (kb.rightArrowKey.isPressed ? 1 : 0) - (kb.leftArrowKey.isPressed ? 1 : 0);
            float z = (kb.upArrowKey.isPressed ? 1 : 0) - (kb.downArrowKey.isPressed ? 1 : 0);
            float y = (kb.spaceKey.isPressed ? 1 : 0) - (kb.leftShiftKey.isPressed ? 1 : 0);

            Transform view = cameraTransform != null ? cameraTransform : transform;
            inputDir = view.forward * z + view.right * x + Vector3.up * y;
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