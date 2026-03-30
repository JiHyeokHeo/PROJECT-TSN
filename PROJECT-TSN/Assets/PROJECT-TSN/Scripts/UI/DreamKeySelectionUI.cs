using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 꿈 이벤트 발동 전 CosmicTrace 레코드를 선택하는 패널.
    /// UIList: Panel_DreamKeySelection
    /// Prefab 경로: Resources/UI/Prefabs/UI.Panel_DreamKeySelection
    ///
    /// 흐름:
    ///   Show() → allRecords 중 CosmicTrace 필터링
    ///          → 항목 없으면 emptyMessage + emptyConfirmBtn 표시
    ///          → 항목 있으면 keyListContainer 에 keyItemPrefab 동적 생성
    ///   레코드 항목 클릭 → DreamEventRunner.StartEvent(record) → Hide()
    ///
    /// Inspector 와이어링:
    ///   keyListContainer  — 스크롤뷰 content Transform
    ///   keyItemPrefab     — RecordListItem 컴포넌트가 붙은 행 프리팹
    ///   emptyMessage      — "수집된 흔적이 없습니다" 텍스트 오브젝트
    ///   emptyConfirmBtn   — 빈 상태 확인 버튼 (DayAttic 으로 복귀)
    /// </summary>
    public class DreamKeySelectionUI : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private Transform  keyListContainer;
        [SerializeField] private GameObject keyItemPrefab;
        [SerializeField] private GameObject emptyMessage;
        [SerializeField] private Button     emptyConfirmBtn;

        // ── Unity 생명주기 ───────────────────────────────────────────
        private void Awake()
        {
            if (emptyConfirmBtn != null)
                emptyConfirmBtn.onClick.AddListener(OnEmptyConfirm);
        }

        // ── UIBase 오버라이드 ─────────────────────────────────────────
        public override void Show()
        {
            base.Show();
            Refresh();
        }

        // ── 내부 ─────────────────────────────────────────────────────
        private void Refresh()
        {
            ClearList();

            List<ObservationRecord> traces = GetCosmicTraces();
            bool isEmpty = traces.Count == 0;

            if (emptyMessage != null)
                emptyMessage.SetActive(isEmpty);

            if (emptyConfirmBtn != null)
                emptyConfirmBtn.gameObject.SetActive(isEmpty);

            if (isEmpty) return;

            foreach (ObservationRecord record in traces)
            {
                SpawnItem(record);
            }
        }

        private List<ObservationRecord> GetCosmicTraces()
        {
            var result = new List<ObservationRecord>();
            List<ObservationRecord> all = ObservationJournal.Singleton.GetAllRecords();
            foreach (ObservationRecord r in all)
            {
                if (r.type == RecordType.CosmicTrace)
                    result.Add(r);
            }
            return result;
        }

        private void SpawnItem(ObservationRecord record)
        {
            if (keyItemPrefab == null || keyListContainer == null) return;

            GameObject go = Instantiate(keyItemPrefab, keyListContainer);
            RecordListItem item = go.GetComponent<RecordListItem>();
            if (item != null)
            {
                item.Setup(record, OnRecordSelected);
            }
        }

        private void ClearList()
        {
            if (keyListContainer == null) return;

            for (int i = keyListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(keyListContainer.GetChild(i).gameObject);
            }
        }

        // ── 콜백 ─────────────────────────────────────────────────────
        private void OnRecordSelected(string recordId)
        {
            List<ObservationRecord> all = ObservationJournal.Singleton.GetAllRecords();
            ObservationRecord target = all.Find(r => r.id == recordId);
            if (target == null)
            {
                Debug.LogWarningFormat("[DreamKeySelectionUI] 레코드를 찾을 수 없습니다: {0}", recordId);
                return;
            }

            Hide();
            DreamEventRunner.Singleton.StartEvent(target);
        }

        private void OnEmptyConfirm()
        {
            Hide();
            // 레코드 없음 → DayAttic 으로 복귀
            PhaseManager.Singleton.TransitionTo(GamePhase.DayAttic);
        }
    }
}
