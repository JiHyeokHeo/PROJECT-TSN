using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 소지 기록 인벤토리 팝업 (Popup_Inventory).
    /// 아직 처분되지 않은 관측 기록(pendingRecords)을 확인합니다.
    /// 처분하려면 학회(Popup_RecordDisposal)를 이용하십시오.
    ///
    /// Inspector 연결:
    ///   - recordListContainer  : 항목 ScrollView의 Content Transform
    ///   - recordItemPrefab     : RecordListItem 컴포넌트가 있는 항목 프리팹
    ///   - detailPanel          : 선택한 기록의 상세 정보 패널 루트
    ///   - detailNameLabel      : 기록 이름 텍스트
    ///   - detailTypeLabel      : 기록 종류 텍스트 (천체/현상/흔적)
    ///   - detailRarityLabel    : 등급 텍스트
    ///   - detailDescLabel      : 설명 텍스트
    ///   - emptyLabel           : 소지 기록 없음 안내 텍스트
    ///   - closeButton          : 닫기 버튼
    ///
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_Inventory
    /// </summary>
    public class InventoryPopupUI : UIBase
    {
        [SerializeField] private Transform       recordListContainer;
        [SerializeField] private RecordListItem  recordItemPrefab;

        [Header("Detail Panel")]
        [SerializeField] private GameObject      detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameLabel;
        [SerializeField] private TextMeshProUGUI detailTypeLabel;
        [SerializeField] private TextMeshProUGUI detailRarityLabel;
        [SerializeField] private TextMeshProUGUI detailDescLabel;

        [Header("Misc")]
        [SerializeField] private TextMeshProUGUI emptyLabel;
        [SerializeField] private Button          closeButton;

        private readonly List<RecordListItem> _pooledItems = new List<RecordListItem>();

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            if (detailPanel != null) detailPanel.SetActive(false);
            RefreshList();
        }

        // ── 목록 갱신 ─────────────────────────────────────────────────────
        private void RefreshList()
        {
            ClearPool();

            List<ObservationRecord> pending = ObservationJournal.Singleton.GetPendingRecords();

            foreach (var record in pending)
            {
                var captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, recordListContainer);
                item.Setup(captured, _ => ShowDetail(captured));
                _pooledItems.Add(item);
            }

            if (emptyLabel != null)
                emptyLabel.gameObject.SetActive(pending.Count == 0);
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
        }

        // ── 풀 정리 ───────────────────────────────────────────────────────
        private void ClearPool()
        {
            foreach (var item in _pooledItems)
                if (item != null) Destroy(item.gameObject);
            _pooledItems.Clear();
        }
    }
}
