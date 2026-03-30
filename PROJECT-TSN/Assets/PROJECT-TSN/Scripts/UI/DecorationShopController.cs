using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 장식품 구매 팝업 (Popup_DecorationShop).
    /// 플레이스홀더 장식품 3종을 표시하고 구매를 처리합니다.
    ///
    /// 구매 버튼 비활성 조건:
    ///   - PlayerParameters.Singleton.Funds &lt; 아이템 가격
    ///
    /// Inspector wiring:
    ///   - shopItemsContainer : 아이템 행 부모 Transform
    ///   - shopItemPrefab     : 아이템 행 프리팹 (ShopItemRow 컴포넌트 또는 TMP+Button 구조)
    ///   - closeBtn           : 닫기 버튼
    /// </summary>
    public class DecorationShopController : UIBase
    {
        // ----------------------------------------------------------------
        //  Inspector
        // ----------------------------------------------------------------
        [Header("Shop Items")]
        [SerializeField] private Transform  shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("Close Button")]
        [SerializeField] private Button closeBtn;

        // ----------------------------------------------------------------
        //  Placeholder catalogue
        // ----------------------------------------------------------------
        private static readonly (string name, double cost)[] Catalogue =
        {
            ("별자리 포스터",   50.0),
            ("우주 모빌",      100.0),
            ("망원경 미니어처", 200.0),
        };

        // ----------------------------------------------------------------
        //  UIBase override
        // ----------------------------------------------------------------
        public override void Show()
        {
            base.Show();
            RefreshList();
            WireCloseButton();
        }

        // ----------------------------------------------------------------
        //  List building
        // ----------------------------------------------------------------
        private void RefreshList()
        {
            ClearList();

            for (int i = 0; i < Catalogue.Length; i++)
            {
                SpawnShopItem(Catalogue[i].name, Catalogue[i].cost);
            }
        }

        private void ClearList()
        {
            if (shopItemsContainer == null) return;

            for (int i = shopItemsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(shopItemsContainer.GetChild(i).gameObject);
            }
        }

        private void SpawnShopItem(string itemName, double cost)
        {
            if (shopItemPrefab == null || shopItemsContainer == null) return;

            GameObject go = Instantiate(shopItemPrefab, shopItemsContainer);

            ShopItemRow rowItem = go.GetComponent<ShopItemRow>();
            if (rowItem != null)
            {
                bool canAfford = PlayerParameters.Singleton.Funds >= cost;
                string capturedName = itemName;
                double capturedCost = cost;
                rowItem.Setup(itemName, cost, canAfford, () => OnPurchaseClicked(capturedName, capturedCost));
                return;
            }

            // Fallback: 프리팹에 ShopItemRow 없을 때 TMP 텍스트 + Button 으로 채움
            TextMeshProUGUI[] labels = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length >= 1) labels[0].text = itemName;
            if (labels.Length >= 2) labels[1].text = $"{cost:N0}G";

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.interactable = PlayerParameters.Singleton.Funds >= cost;

                string captured  = itemName;
                double captCost  = cost;
                btn.onClick.AddListener(() => OnPurchaseClicked(captured, captCost));
            }
        }

        // ----------------------------------------------------------------
        //  Purchase
        // ----------------------------------------------------------------
        private void OnPurchaseClicked(string itemName, double cost)
        {
            if (PlayerParameters.Singleton.Funds < cost)
            {
                Debug.LogFormat("[DecorationShopController] Insufficient funds for '{0}'. Required: {1}, Available: {2}",
                    itemName, cost, PlayerParameters.Singleton.Funds);
                return;
            }

            PlayerParameters.Singleton.AddFunds(-cost);
            Debug.LogFormat("[DecorationShopController] Purchased '{0}' for {1}G.", itemName, cost);

            // 구매 후 버튼 상태 갱신
            RefreshList();
        }

        // ----------------------------------------------------------------
        //  Close
        // ----------------------------------------------------------------
        private void WireCloseButton()
        {
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnCloseClicked()
        {
            UIManager.Hide<DecorationShopController>(UIList.Popup_DecorationShop);

            if (DayCityController.Singleton != null)
                DayCityController.Singleton.HideSubLocation();
        }
    }

    // ====================================================================
    //  ShopItemRow — 상점 아이템 행 컴포넌트
    // ====================================================================
    /// <summary>
    /// 장식품 상점 내 아이템 행 한 줄 컴포넌트.
    ///
    /// Inspector wiring:
    ///   - nameLabel   : 아이템명 텍스트
    ///   - priceLabel  : 가격 텍스트
    ///   - buyBtn      : 구매 버튼
    /// </summary>
    public class ShopItemRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Button          buyBtn;

        public void Setup(string itemName, double cost, bool canAfford, Action onBuy)
        {
            if (nameLabel  != null) nameLabel.text  = itemName;
            if (priceLabel != null) priceLabel.text = $"{cost:N0}G";

            if (buyBtn != null)
            {
                buyBtn.interactable = canAfford;
                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() => onBuy?.Invoke());
            }
        }
    }
}
