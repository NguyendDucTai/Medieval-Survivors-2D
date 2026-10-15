using UnityEngine;

namespace VampireSurvivors.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        public float smoothSpeed = 8f;

        private void Start()
        {
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
