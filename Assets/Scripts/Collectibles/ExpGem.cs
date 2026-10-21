using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Player;

namespace VampireSurvivors.Collectibles
{
    public class ExpGem : MonoBehaviour
    {
        public static readonly HashSet<ExpGem> activeGems = new HashSet<ExpGem>();
        public static ExpGem consolidatedRedGem = null;
        public const int MaxGemsOnGround = 140;

        [Header("Gem Settings")]
        public float expAmount = 10f;
        public float moveSpeed = 8.5f;
        public bool isRedGem = false;

        private Transform playerTransform;
        private PlayerStats playerStats;
        private bool isBeingCollected = false;
        private SpriteRenderer spriteRenderer;
        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void OnEnable()
        {
            isBeingCollected = false;
            activeGems.Add(this);
            FindPlayer();

            if (!isRedGem)
            {
                ResetVisual();
            }
        }

        private void OnDisable()
        {
            activeGems.Remove(this);
            if (consolidatedRedGem == this)
            {
                consolidatedRedGem = null;
            }
        }

        public void MakeRedGem()
        {
            isRedGem = true;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.18f, 0.25f); // Glowing Crimson Red
            }
            transform.localScale = baseScale * 1.55f;
        }

        public void ResetVisual()
        {
            isRedGem = false;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }
            transform.localScale = baseScale;
        }

        public void PulseOnAccumulate()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PulseFlash());
            }
        }

        private IEnumerator PulseFlash()
        {
            transform.localScale = baseScale * 1.8f;
            yield return new WaitForSeconds(0.08f);
            transform.localScale = baseScale * 1.55f;
        }

        public static void DropExp(Vector3 position, float expValue, GameObject fallbackPrefab)
        {
            // Vampire Survivors Gem Consolidation:
            // When gems exceed MaxGemsOnGround, stop creating new gems and accumulate into the Red Gem!
            if (activeGems.Count >= MaxGemsOnGround)
            {
                if (consolidatedRedGem == null || !consolidatedRedGem.gameObject.activeInHierarchy)
                {
                    // Find the gem furthest from player to transform into the Consolidated Red Gem
                    ExpGem furthest = null;
                    float maxDist = -1f;
                    Vector3 pPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : position;

                    foreach (var gem in activeGems)
                    {
                        if (gem != null && gem.gameObject.activeInHierarchy && !gem.isBeingCollected)
                        {
                            float d = Vector2.Distance(gem.transform.position, pPos);
                            if (d > maxDist)
                            {
                                maxDist = d;
                                furthest = gem;
                            }
                        }
                    }

                    if (furthest != null)
                    {
                        consolidatedRedGem = furthest;
                        consolidatedRedGem.MakeRedGem();
                    }
                }

                if (consolidatedRedGem != null)
                {
                    consolidatedRedGem.expAmount += expValue;
                    consolidatedRedGem.PulseOnAccumulate();
                    return; // Succeeded! 0 new GameObjects spawned!
                }
            }

            // Normal spawn via pool or instantiate
            GameObject gemGO = null;
            if (Core.ObjectPooler.Instance != null && Core.ObjectPooler.Instance.poolDictionary.ContainsKey("ExpGem"))
            {
                gemGO = Core.ObjectPooler.Instance.SpawnFromPool("ExpGem", position, Quaternion.identity);
            }
            else if (fallbackPrefab != null)
            {
                gemGO = Instantiate(fallbackPrefab, position, Quaternion.identity);
            }

            if (gemGO != null)
            {
                ExpGem eg = gemGO.GetComponent<ExpGem>();
                if (eg != null)
                {
                    eg.expAmount = expValue;
                    eg.ResetVisual();
                }
            }
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
                playerStats.AddExperience(expAmount);
            }

            // If Red Gem was collected, show floating text
            if (isRedGem && UI.DamageNumberPool.Instance != null)
            {
                UI.DamageNumberPool.Instance.SpawnExp(expAmount, transform.position, true);
            }

            gameObject.SetActive(false);
        }
    }
}
