using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 기록 처분 팝업 (Popup_RecordDisposal).
    /// 대기 중인 관측 기록 목록을 표시하고, 처분 방법 5종으로 기록을 처리합니다.
    ///
    /// DisposalMethod 매핑 (ObservationRecord.cs 기준):
    ///   발표(Announce)  → DisposalMethod.Announce   Fame+ Funds+
    ///   기록(Record)    → DisposalMethod.Destroy     Sanity+
    ///   탐구(Research)  → DisposalMethod.Explore     Enlightenment+
    ///   ???             → DisposalMethod.Madden      Madness+
    ///   돌아간다        → 팝업 닫기
    ///
    /// Inspector wiring:
    ///   - announceBtn       : 발표 버튼
    ///   - recordBtn         : 기록 버튼
    ///   - researchBtn       : 탐구 버튼
    ///   - madnessBtn        : #@!$% 버튼
    ///   - backBtn           : 돌아간다 버튼
    ///   - recordListContainer : 기록 아이템 부모 Transform (ScrollView content)
    ///   - recordItemPrefab  : 기록 한 행을 표시하는 프리팹 (RecordListItem 컴포넌트 필요)
    /// </summary>
    public class AcademyController : UIBase
    {
        // ----------------------------------------------------------------
        //  Inspector
        // ----------------------------------------------------------------
        [Header("Disposal Buttons")]
        [SerializeField] private Button announceBtn;
        [SerializeField] private Button recordBtn;
        [SerializeField] private Button researchBtn;
        [SerializeField] private Button madnessBtn;
        [SerializeField] private Button backBtn;

        [Header("Record List")]
        [SerializeField] private Transform recordListContainer;
        [SerializeField] private GameObject recordItemPrefab;

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private List<ObservationRecord> _pendingRecords = new List<ObservationRecord>();
        private string _selectedRecordId = null;

        // ----------------------------------------------------------------
        //  UIBase override
        // ----------------------------------------------------------------
        public override void Show()
        {
            base.Show();
            LoadPendingRecords();
            WireButtons();
        }

        public override void Hide()
        {
            base.Hide();
            _selectedRecordId = null;
        }

        // ----------------------------------------------------------------
        //  Initialization
        // ----------------------------------------------------------------
        private void LoadPendingRecords()
        {
            _pendingRecords = ObservationJournal.Singleton.GetPendingRecords();

            ClearList();

            bool hasPending = _pendingRecords.Count > 0;

            SetDisposalButtonsInteractable(false); // 기록 선택 전까지 비활성

            if (!hasPending)
            {
                // 기록 없음 — 처분 버튼 모두 비활성, backBtn 만 활성
                return;
            }

            foreach (ObservationRecord rec in _pendingRecords)
            {
                SpawnRecordItem(rec);
            }
        }

        private void WireButtons()
        {
            if (announceBtn != null)
            {
                announceBtn.onClick.RemoveAllListeners();
                announceBtn.onClick.AddListener(() => DisposeSelected(DisposalMethod.Announce));
            }

            if (recordBtn != null)
            {
                recordBtn.onClick.RemoveAllListeners();
                recordBtn.onClick.AddListener(() => DisposeSelected(DisposalMethod.Destroy));
            }

            if (researchBtn != null)
            {
                researchBtn.onClick.RemoveAllListeners();
                researchBtn.onClick.AddListener(() => DisposeSelected(DisposalMethod.Explore));
            }

            if (madnessBtn != null)
            {
                madnessBtn.onClick.RemoveAllListeners();
                madnessBtn.onClick.AddListener(() => DisposeSelected(DisposalMethod.Madden));
            }

            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OnBackClicked);
            }
        }

        // ----------------------------------------------------------------
        //  Record list
        // ----------------------------------------------------------------
        private void ClearList()
        {
            if (recordListContainer == null) return;

            for (int i = recordListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(recordListContainer.GetChild(i).gameObject);
            }
        }

        private void SpawnRecordItem(ObservationRecord rec)
        {
            if (recordItemPrefab == null || recordListContainer == null) return;

            GameObject go = Instantiate(recordItemPrefab, recordListContainer);

            RecordListItem item = go.GetComponent<RecordListItem>();
            if (item != null)
            {
                item.Setup(rec, OnRecordSelected);
            }
            else
            {
                // Fallback: 프리팹에 RecordListItem 이 없으면 TMP 텍스트만 채움
                TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"[{rec.rarity}] {rec.name}";

                Button btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    string capturedId = rec.id;
                    btn.onClick.AddListener(() => OnRecordSelected(capturedId));
                }
            }
        }

        // ----------------------------------------------------------------
        //  Selection & disposal
        // ----------------------------------------------------------------
        private void OnRecordSelected(string recordId)
        {
            _selectedRecordId = recordId;
            SetDisposalButtonsInteractable(_pendingRecords.Count > 0);
        }

        private void DisposeSelected(DisposalMethod method)
        {
            if (string.IsNullOrEmpty(_selectedRecordId))
            {
                Debug.LogWarning("[AcademyController] No record selected.");
                return;
            }

            DisposeResult result = ObservationJournal.Singleton.DisposePendingRecord(_selectedRecordId, method);
            if (result == null)
            {
                Debug.LogWarningFormat("[AcademyController] Disposal failed for id: {0}", _selectedRecordId);
                return;
            }

            _selectedRecordId = null;

            // 목록 갱신
            LoadPendingRecords();
        }

        private void OnBackClicked()
        {
            UIManager.Hide<AcademyController>(UIList.Popup_RecordDisposal);

            if (DayCityController.Singleton != null)
                DayCityController.Singleton.HideSubLocation();
        }

        // ----------------------------------------------------------------
        //  Button state
        // ----------------------------------------------------------------
        private void SetDisposalButtonsInteractable(bool interactable)
        {
            SetBtnInteractable(announceBtn, interactable);
            SetBtnInteractable(recordBtn,   interactable);
            SetBtnInteractable(researchBtn, interactable);
            SetBtnInteractable(madnessBtn,  interactable);
        }

        private static void SetBtnInteractable(Button btn, bool interactable)
        {
            if (btn != null) btn.interactable = interactable;
        }
    }
}
