using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TST
{
    /// <summary>
    /// 소지/보관 기록 인벤토리 팝업 (Popup_Inventory).
    /// "보유한 기록" 섹션은 pending 기록, "보관된 기록" 섹션은 archived 기록을 표시합니다.
    /// 이 팝업에서는 기록 파기 및 신규 보관이 불가합니다.
    ///
    /// Inspector 연결:
    ///   [보유한 기록 섹션]
    ///   - heldContainer      : ScrollView Content Transform
    ///   - recordItemPrefab   : RecordListItem 프리팹
    ///   - heldEmptyLabel     : 보유 기록 없음 안내 텍스트
    ///   [보관된 기록 섹션]
    ///   - archivedContainer  : ScrollView Content Transform
    ///   - archivedEmptyLabel : 보관 기록 없음 안내 텍스트
    ///   [상세 패널]
    ///   - detailPanel        : 상세 정보 패널 루트
    ///   - detailNameLabel    : 기록 이름 텍스트
    ///   - detailTypeLabel    : 기록 종류 텍스트
    ///   - detailRarityLabel  : 등급 텍스트
    ///   - detailDescLabel    : 설명 텍스트
    ///   - detailIconImage    : 상세 패널 아이콘 (optional)
    ///   [Misc]
    ///   - closeButton        : 닫기 버튼
    ///
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_Inventory
    /// </summary>
    public class InventoryPopupUI : UIBase
    {
        [Header("보유한 기록 섹션")]
        [SerializeField] private GameObject      heldContainer;
        [SerializeField] private RecordListItem  recordItemPrefab;
        [SerializeField] private TextMeshProUGUI heldEmptyLabel;

        [Header("보관된 기록 섹션")]
        [SerializeField] private GameObject      archivedContainer;
        [SerializeField] private TextMeshProUGUI archivedEmptyLabel;

        [Header("상세 패널")]
        [SerializeField] private GameObject      detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameLabel;
        [SerializeField] private TextMeshProUGUI detailTypeLabel;
        [SerializeField] private TextMeshProUGUI detailRarityLabel;
        [SerializeField] private TextMeshProUGUI detailDescLabel;
        [SerializeField] private Image           detailIconImage;

        [Header("Misc")]
        [SerializeField] private Button          closeButton;

        private readonly List<RecordListItem> _heldItems     = new List<RecordListItem>();
        private readonly List<RecordListItem> _archivedItems = new List<RecordListItem>();

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            if (detailPanel != null) detailPanel.SetActive(false);
            RefreshHeld();
            RefreshArchived();
        }

        // ── 목록 갱신 ─────────────────────────────────────────────────────

        private void RefreshHeld()
        {
            ClearPool(_heldItems, heldContainer != null ? heldContainer.transform : null);

            List<ObservationRecord> pending = ObservationJournal.Singleton.GetPendingRecords();

            foreach (var record in pending)
            {
                var captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, heldContainer != null ? heldContainer.transform : null);
                item.gameObject.SetActive(true);
                item.Setup(captured, _ => ShowDetail(captured));
                _heldItems.Add(item);
            }

            if (heldEmptyLabel != null)
                heldEmptyLabel.gameObject.SetActive(pending.Count == 0);
        }

        private void RefreshArchived()
        {
            ClearPool(_archivedItems, archivedContainer != null ? archivedContainer.transform : null);

            List<ObservationRecord> archived = ObservationJournal.Singleton.GetArchivedRecords();

            foreach (var record in archived)
            {
                var captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, archivedContainer != null ? archivedContainer.transform : null);
                item.gameObject.SetActive(true);
                item.Setup(captured, _ => ShowDetail(captured));
                _archivedItems.Add(item);
            }

            if (archivedEmptyLabel != null)
                archivedEmptyLabel.gameObject.SetActive(archived.Count == 0);
        }

        // ── 상세 보기 ─────────────────────────────────────────────────────

        private void ShowDetail(ObservationRecord record)
        {
            if (detailPanel == null) return;
            detailPanel.SetActive(true);

            if (detailNameLabel   != null) detailNameLabel.text   = record.name;
            if (detailTypeLabel   != null) detailTypeLabel.text   = record.type.ToString();
            if (detailRarityLabel != null) detailRarityLabel.text = record.rarity.ToString();
            if (detailDescLabel   != null) detailDescLabel.text   = record.description;

            if (detailIconImage != null)
            {
                // GameDataModel에서 SO 조회 → 아이콘이 있으면 표시, 없으면 rarity 색상 fallback
                bool hasIcon = false;
                if (GameDataModel.Singleton != null &&
                    GameDataModel.Singleton.GetItemData(record.id, out ItemDefinitionSO def) &&
                    def.icon != null)
                {
                    detailIconImage.sprite = def.icon;
                    detailIconImage.color  = Color.white;
                    hasIcon = true;
                }

                if (!hasIcon)
                {
                    detailIconImage.sprite = null;
                    detailIconImage.color  = GetRarityColor(record.rarity);
                }
            }
        }

        // ── 풀 정리 ───────────────────────────────────────────────────────

        private void ClearPool(List<RecordListItem> pool, Transform container = null)
        {
            foreach (var item in pool)
                if (item != null) Destroy(item.gameObject);
            pool.Clear();
        }

        // ── 헬퍼 ──────────────────────────────────────────────────────────

        private static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:    return Color.white;
                case Rarity.Uncommon:  return new Color(0.4f, 1f,    0.4f, 1f);
                case Rarity.Rare:      return new Color(0.4f, 0.6f,  1f,   1f);
                case Rarity.Legendary: return new Color(1f,   0.85f, 0f,   1f);
                default:               return Color.white;
            }
        }
    }
}
