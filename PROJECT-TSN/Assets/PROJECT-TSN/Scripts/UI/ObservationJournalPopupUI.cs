using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 천체 관측 일지 팝업 (Popup_ObservationJournal).
    /// ObservationJournal에 저장된 모든 기록을 열람합니다.
    ///
    /// 필터 탭: 전체 / 천체 / 현상 / 흔적
    /// 목록 클릭 시 상세 패널(이름, 등급, 설명)을 표시합니다.
    ///
    /// Inspector 연결:
    ///   - recordListContainer  : 기록 항목 ScrollView의 Content Transform
    ///   - recordItemPrefab     : RecordListItem 컴포넌트가 있는 항목 프리팹
    ///   - filterButtons[4]     : 전체/천체/현상/흔적 순서로 배치된 필터 버튼
    ///   - detailPanel          : 상세 정보 패널 루트
    ///   - detailNameLabel      : 상세 이름 텍스트
    ///   - detailRarityLabel    : 상세 등급 텍스트
    ///   - detailDescLabel      : 상세 설명 텍스트
    ///   - emptyLabel           : 기록 없음 안내 텍스트
    ///   - closeButton          : 닫기 버튼
    ///
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_ObservationJournal
    /// </summary>
    public class ObservationJournalPopupUI : UIBase
    {
        // ── Filter 순서와 일치 ────────────────────────────────────────────
        private static readonly RecordType?[] FilterTypes =
        {
            null,                    // 0 = 전체
            RecordType.CelestialBody,
            RecordType.Phenomenon,
            RecordType.CosmicTrace
        };

        // ── Inspector ─────────────────────────────────────────────────────
        [SerializeField] private Transform            recordListContainer;
        [SerializeField] private RecordListItem       recordItemPrefab;
        [SerializeField] private Button[]             filterButtons;   // length 4

        [Header("Detail Panel")]
        [SerializeField] private GameObject           detailPanel;
        [SerializeField] private TextMeshProUGUI      detailNameLabel;
        [SerializeField] private TextMeshProUGUI      detailRarityLabel;
        [SerializeField] private TextMeshProUGUI      detailDescLabel;

        [Header("Misc")]
        [SerializeField] private TextMeshProUGUI      emptyLabel;
        [SerializeField] private Button               closeButton;

        // ── Runtime ───────────────────────────────────────────────────────
        private int _activeFilter = 0;  // 0 = 전체
        private readonly List<RecordListItem> _pooledItems = new List<RecordListItem>();

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            for (int i = 0; i < filterButtons.Length; i++)
            {
                int index = i;
                filterButtons[i].onClick.AddListener(() => OnFilterClicked(index));
            }
            closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            _activeFilter = 0;
            if (detailPanel != null) detailPanel.SetActive(false);
            RefreshList();
        }

        // ── 필터 ─────────────────────────────────────────────────────────
        private void OnFilterClicked(int index)
        {
            _activeFilter = index;
            RefreshList();
        }

        // ── 목록 갱신 ─────────────────────────────────────────────────────
        private void RefreshList()
        {
            ClearPool();

            List<ObservationRecord> allRecords = ObservationJournal.Singleton.GetAllRecords();
            RecordType? filterType = FilterTypes[_activeFilter];

            int count = 0;
            foreach (var record in allRecords)
            {
                if (filterType.HasValue && record.type != filterType.Value) continue;

                var captured = record;
                RecordListItem item = Instantiate(recordItemPrefab, recordListContainer);
                item.Setup(captured, _ => ShowDetail(captured));
                _pooledItems.Add(item);
                count++;
            }

            if (emptyLabel != null)
                emptyLabel.gameObject.SetActive(count == 0);
        }

        // ── 상세 보기 ─────────────────────────────────────────────────────
        private void ShowDetail(ObservationRecord record)
        {
            if (detailPanel == null) return;
            detailPanel.SetActive(true);

            if (detailNameLabel   != null) detailNameLabel.text   = record.name;
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
