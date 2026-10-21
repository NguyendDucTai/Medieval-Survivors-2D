using UnityEngine;
using VampireSurvivors.Player;
using VampireSurvivors.UI;

namespace VampireSurvivors.Collectibles
{
    public class GoldCoin : MonoBehaviour
    {
        [Header("Coin Settings")]
        public int goldAmount = 5;
        public float moveSpeed = 9f;

        private Transform playerTransform;
        private PlayerStats playerStats;
        private bool isBeingCollected = false;

        private void Start()
        {
            FindPlayer();
        }

        private void OnEnable()
        {
            isBeingCollected = false;
            FindPlayer();
        }

        private void FindPlayer()
        {
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
                playerStats = PlayerStats.Instance != null ? PlayerStats.Instance : PlayerController.Instance.GetComponent<PlayerStats>();
                return;
            }

#if UNITY_6000_0_OR_NEWER
            PlayerController player = FindAnyObjectByType<PlayerController>();
#else
            PlayerController player = FindObjectOfType<PlayerController>();
#endif
            if (player != null)
            {
                playerTransform = player.transform;
                playerStats = player.GetComponent<PlayerStats>();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (!isBeingCollected && playerStats != null)
            {
                if (distance <= playerStats.pickupRadius)
                {
                    isBeingCollected = true;
                }
            }

            if (isBeingCollected)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

                if (distance < 0.35f)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            if (playerStats != null)
            {
                float mapBonus = PlayerPrefs.GetFloat("SelectedMapGoldBonus", 1.0f);
                int finalGold = Mathf.Max(1, Mathf.RoundToInt(goldAmount * mapBonus));
                playerStats.AddGold(finalGold);
                if (DamageNumberPool.Instance != null)
                {
                    DamageNumberPool.Instance.SpawnGold(finalGold, transform.position);
                }
            }
            Destroy(gameObject);
        }
    }
}
