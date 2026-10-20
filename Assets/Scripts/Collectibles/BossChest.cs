using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VampireSurvivors.Player;
using VampireSurvivors.UI;
using VampireSurvivors.Weapons;

namespace VampireSurvivors.Collectibles
{
    public class BossChest : MonoBehaviour
    {
        [Header("Chest Rewards")]
        public int minGold = 100;
        public int maxGold = 350;
        public float healAmount = 35f;

        private bool isOpened = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isOpened) return;

            if (collision.CompareTag("Player"))
            {
                OpenChest(collision.GetComponent<PlayerStats>());
            }
        }

        private void OpenChest(PlayerStats player)
        {
            isOpened = true;

            // Trigger Roulette Modal Celebration
            BossChestRouletteUI roulette = BossChestRouletteUI.Instance;
            if (roulette == null)
            {
#if UNITY_6000_0_OR_NEWER
                roulette = Object.FindAnyObjectByType<BossChestRouletteUI>();
#else
                roulette = Object.FindObjectOfType<BossChestRouletteUI>();
#endif
            }

            if (roulette == null)
            {
                GameObject go = new GameObject("BossChestRouletteUI");
                roulette = go.AddComponent<BossChestRouletteUI>();
            }

            if (roulette != null)
            {
                roulette.OpenRoulette(player, this);
                return;
            }

            // Fallback direct rewards if UI is absent
            int goldEarned = Random.Range(minGold, maxGold + 1);
            if (player != null)
            {
                player.AddGold(goldEarned);
                player.Heal(healAmount);
            }

            if (DamageNumberPool.Instance != null)
            {
                DamageNumberPool.Instance.SpawnGold(goldEarned, transform.position);
                DamageNumberPool.Instance.SpawnHeal(healAmount, transform.position + Vector3.up * 0.4f);
            }

            // Check for Weapon Evolution
            bool evolved = false;
            if (WeaponInventory.Instance != null)
            {
                foreach (var w in WeaponInventory.Instance.equippedWeapons)
                {
                    if (WeaponInventory.Instance.CanEvolve(w))
                    {
                        WeaponInventory.Instance.EvolveWeapon(w.weaponID);
                        evolved = true;
                        break;
                    }
                }

                // If no evolution was ready, upgrade a random weapon or passive
                if (!evolved && WeaponInventory.Instance.equippedWeapons.Count > 0)
                {
                    var upgradable = WeaponInventory.Instance.equippedWeapons.FindAll(w => w.level < w.maxLevel);
                    if (upgradable.Count > 0)
                    {
                        var target = upgradable[Random.Range(0, upgradable.Count)];
                        WeaponInventory.Instance.UpgradeWeapon(target.weaponID);
                    }
                }
            }

            // Camera shake
            var cameraJuice = Camera.main != null ? Camera.main.GetComponent<Core.CameraJuice>() : null;
            if (cameraJuice != null) cameraJuice.Shake(0.3f, 0.3f);

            StartCoroutine(Disappear());
        }

        private IEnumerator Disappear()
        {
            // Pop up and shrink
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale * 1.3f, Vector3.zero, elapsed / 0.3f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
