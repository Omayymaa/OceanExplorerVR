using UnityEngine;

namespace OceanExplorer.Module2
{
    // Place le pistolet : devant la caméra en FPS, dans la main du plongeur en TPS
    public class M2_GunMount : MonoBehaviour
    {
        public M2_Diver diver;             // Le script du plongeur (pour connaître la vue FPS/TPS)
        public Transform cameraTransform;  // La caméra (Main Camera)
        public Transform handBone;         // L'os de la main droite du Racer (RightHand)

        [Header("FPS (relatif à la caméra)")]
        public Vector3 fpsPos = new Vector3(0.28f, -0.315f, 0.7f);
        public Vector3 fpsRot = Vector3.zero;
        public Vector3 fpsScale = new Vector3(0.3f, 0.3f, 0.3f);

        [Header("TPS (relatif à l'os de la main)")]
        public Vector3 handPos = Vector3.zero;
        public Vector3 handRot = Vector3.zero;
        public Vector3 handScale = new Vector3(1f, 1f, 1f);

        void LateUpdate()
        {
            if (diver == null || cameraTransform == null) return;

            // Garde le pistolet visible dans les deux vues
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.enabled = true;

            bool fps = diver.fpsMode;

            // FPS : parent = caméra. TPS : parent = os de la main (ou le Diver si l'os n'est pas défini)
            Transform parent = fps ? cameraTransform : (handBone != null ? handBone : diver.transform);
            if (transform.parent != parent) transform.SetParent(parent, false);

            // Position, rotation et taille selon la vue
            transform.localPosition = fps ? fpsPos : handPos;
            transform.localRotation = Quaternion.Euler(fps ? fpsRot : handRot);
            transform.localScale = fps ? fpsScale : handScale;
        }
    }
}