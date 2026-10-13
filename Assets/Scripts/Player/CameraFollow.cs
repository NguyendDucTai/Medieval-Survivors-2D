using UnityEngine;

namespace VampireSurvivors.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        public float smoothSpeed = 6f;

        private void Start()
        {
            FindTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        private void FindTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
}
