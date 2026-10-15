using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VampireSurvivors.Data;
using VampireSurvivors.MetaProgression;

namespace VampireSurvivors.UI
{
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance;

        [Header("UI Containers")]
        public GameObject panel;
        public Text goldBalanceText;
        public Button closeButton;
        public Transform itemsScrollContent;

        [Header("Shop Item Template")]
        public GameObject shopItemPrefab;

        private List<ShopItemRow> activeRows = new List<ShopItemRow>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (panel != null) panel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
        }

        public void OpenShop()
        {
            if (panel == null) return;
            panel.SetActive(true);
            RefreshShop();
        }

        public void CloseShop()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void RefreshShop()
        {
            if (SaveManager.Instance.Data == null) SaveManager.Instance.Load();
            if (UpgradeManager.Instance == null) return;

            if (goldBalanceText != null)
            {
                goldBalanceText.text = $"Quỹ Vàng: {SaveManager.Instance.Data.gold:N0} Vàng";
            }

            // Clear old rows
            foreach (var r in activeRows)
            {
                if (r != null && r.gameObject != null) Destroy(r.gameObject);
            }
            activeRows.Clear();

            // Populate all upgrades
            var upgrades = UpgradeManager.Instance.allUpgrades;
            foreach (var up in upgrades)
            {
                int currentRank = UpgradeManager.Instance.GetRank(up.id);
                int cost = up.GetCostForRank(currentRank);
                bool isMax = currentRank >= up.maxRank;
                bool canAfford = !isMax && SaveManager.Instance.Data.gold >= cost;

                GameObject rowGO = null;
                if (shopItemPrefab != null && itemsScrollContent != null)
                {
                    rowGO = Instantiate(shopItemPrefab, itemsScrollContent);
                }
                else
                {
                    // Fallback procedural row creation
                    rowGO = CreateProceduralRow(up, currentRank, cost, isMax, canAfford);
                }

                if (rowGO != null)
                {
                    ShopItemRow row = rowGO.GetComponent<ShopItemRow>();
                    if (row != null)
                    {
                        row.Setup(up, currentRank, cost, isMax, canAfford, () => OnPurchaseClicked(up.id));
                        activeRows.Add(row);
                    }
                }
            }
        }

        private GameObject CreateProceduralRow(UpgradeData up, int rank, int cost, bool isMax, bool canAfford)
        {
            if (itemsScrollContent == null) return null;

            GameObject rowGO = new GameObject("Row_" + up.id);
            rowGO.transform.SetParent(itemsScrollContent, false);

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = 45f;
            le.preferredHeight = 45f;

            HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 10f;

            // Title & Rank Text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(rowGO.transform, false);
            Text t = textGO.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 14;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            string rankStr = isMax ? "[TỐI ĐA]" : $"[{rank}/{up.maxRank}]";
            string costStr = isMax ? "" : $" ({cost} Vàng)";
            t.text = $"{up.displayName} {rankStr}\n<size=11><color=#aaaaaa>{up.description}{costStr}</color></size>";

            // Buy Button
            GameObject btnGO = new GameObject("BuyButton");
            btnGO.transform.SetParent(rowGO.transform, false);
            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = isMax ? Color.gray : (canAfford ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.6f, 0.2f, 0.2f));
            Button btn = btnGO.AddComponent<Button>();
            btn.interactable = canAfford;
            string upId = up.id;
            btn.onClick.AddListener(() => OnPurchaseClicked(upId));

            GameObject btnTextGO = new GameObject("BtnText");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            Text bt = btnTextGO.AddComponent<Text>();
            bt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bt.fontSize = 13;
            bt.alignment = TextAnchor.MiddleCenter;
            bt.color = Color.white;
            bt.text = isMax ? "MAX" : "Nâng Cấp";

            RectTransform brt = btnGO.GetComponent<RectTransform>();
            brt.sizeDelta = new Vector2(90, 35);
            RectTransform btrt = btnTextGO.GetComponent<RectTransform>();
            btrt.anchorMin = Vector2.zero;
            btrt.anchorMax = Vector2.one;
            btrt.sizeDelta = Vector2.zero;

            return rowGO;
        }

        private void OnPurchaseClicked(string upgradeId)
        {
            if (UpgradeManager.Instance.PurchaseUpgrade(upgradeId))
            {
                RefreshShop();
            }
        }
    }

    public class ShopItemRow : MonoBehaviour
    {
        public Text titleText;
        public Text descText;
        public Text costText;
        public Button buyButton;

        public void Setup(UpgradeData up, int rank, int cost, bool isMax, bool canAfford, UnityEngine.Events.UnityAction onBuy)
        {
            if (titleText != null)
            {
                string rankStr = isMax ? "[CẤP TỐI ĐA]" : $"[Cấp {rank}/{up.maxRank}]";
                titleText.text = $"{up.displayName} {rankStr}";
            }
            if (descText != null) descText.text = up.description;
            if (costText != null) costText.text = isMax ? "HOÀN TẤT" : $"{cost} VÀNG";
            if (buyButton != null)
            {
                buyButton.interactable = canAfford;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(onBuy);
            }
        }
    }
}
