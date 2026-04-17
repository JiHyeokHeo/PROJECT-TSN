using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Inventory popup (Popup_Inventory).
    /// Shows held records and archived records with detail view.
    /// </summary>
    public class InventoryPopupUI : UIBase
    {
        [Header("Held Records")]
        [SerializeField] private GameObject heldContainer;
        [SerializeField] private RecordListItem recordItemPrefab;
        [SerializeField] private TextMeshProUGUI heldEmptyLabel;

        [Header("Archived Records")]
        [SerializeField] private GameObject archivedContainer;
        [SerializeField] private TextMeshProUGUI archivedEmptyLabel;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameLabel;
        [SerializeField] private TextMeshProUGUI detailTypeLabel;
        [SerializeField] private TextMeshProUGUI detailRarityLabel;
        [SerializeField] private TextMeshProUGUI detailDescLabel;
        [SerializeField] private Image detailIconImage;

        [Header("Misc")]
        [SerializeField] private Button closeButton;

        private readonly List<RecordListItem> heldItems = new List<RecordListItem>();
        private readonly List<RecordListItem> archivedItems = new List<RecordListItem>();

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        public override void Show()
        {
            base.Show();

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }

            RefreshHeld();
            RefreshArchived();
        }

        private void RefreshHeld()
        {
            ClearPool(heldItems);

            List<ObservationRecord> pending = ObservationJournal.Singleton.GetPendingRecords();
            Transform parent = heldContainer != null ? heldContainer.transform : null;

            foreach (ObservationRecord record in pending)
            {
                ObservationRecord captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, parent);
                item.gameObject.SetActive(true);
                item.Setup(captured, _ => ShowDetail(captured));
                heldItems.Add(item);
            }

            if (heldEmptyLabel != null)
            {
                heldEmptyLabel.gameObject.SetActive(pending.Count == 0);
            }
        }

        private void RefreshArchived()
        {
            ClearPool(archivedItems);

            List<ObservationRecord> archived = ObservationJournal.Singleton.GetArchivedRecords();
            Transform parent = archivedContainer != null ? archivedContainer.transform : null;

            foreach (ObservationRecord record in archived)
            {
                ObservationRecord captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, parent);
                item.gameObject.SetActive(true);
                item.Setup(captured, _ => ShowDetail(captured));
                archivedItems.Add(item);
            }

            if (archivedEmptyLabel != null)
            {
                archivedEmptyLabel.gameObject.SetActive(archived.Count == 0);
            }
        }

        private void ShowDetail(ObservationRecord record)
        {
            if (detailPanel == null)
                return;

            detailPanel.SetActive(true);

            if (detailNameLabel != null)
            {
                detailNameLabel.text = record.name;
            }

            if (detailTypeLabel != null)
            {
                detailTypeLabel.text = record.type.ToString();
            }

            if (detailRarityLabel != null)
            {
                detailRarityLabel.text = record.rarity.ToString();
            }

            if (detailDescLabel != null)
            {
                detailDescLabel.text = record.description;
            }

            if (detailIconImage == null)
                return;

            bool hasIcon = false;
            if (GameDataModel.Singleton != null &&
                GameDataModel.Singleton.GetItemData(record.id, out ItemDefinitionSO definition) &&
                definition.icon != null)
            {
                detailIconImage.sprite = definition.icon;
                detailIconImage.color = Color.white;
                hasIcon = true;
            }

            if (!hasIcon)
            {
                detailIconImage.sprite = null;
                detailIconImage.color = GetRarityColor(record.rarity);
            }
        }

        private static Color GetRarityColor(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    return Color.white;
                case Rarity.Uncommon:
                    return new Color(0.4f, 1f, 0.4f, 1f);
                case Rarity.Rare:
                    return new Color(0.4f, 0.6f, 1f, 1f);
                case Rarity.Legendary:
                    return new Color(1f, 0.85f, 0f, 1f);
                default:
                    return Color.white;
            }
        }

        private static void ClearPool(List<RecordListItem> pool)
        {
            foreach (RecordListItem item in pool)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            pool.Clear();
        }
    }
}
