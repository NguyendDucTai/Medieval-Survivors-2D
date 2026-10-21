using UnityEngine;

namespace VampireSurvivors.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        public float smoothSpeed = 8f;

        [Header("Vampire Survivors Camera View")]
        [Tooltip("Orthographic camera size. 7.5 to 8.0 matches the exact Vampire Survivors player-to-world ratio!")]
        public float orthographicSize = 7.5f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
            }
        }

        private void Start()
        {
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
            }
            FindTarget();
            if (target != null)
            {
                // Snap directly on start
                transform.position = target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            smoothedPosition.z = offset.z;

            // Add camera shake offset from CameraJuice if present
            if (Core.CameraJuice.Instance != null)
            {
                smoothedPosition += Core.CameraJuice.Instance.GetShakeOffset();
            }

            transform.position = smoothedPosition;
        }

        private void FindTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null)
                {
                    target = pc.transform;
                }
            }
        }
    }
}
