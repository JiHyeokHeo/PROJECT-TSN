using System.Collections;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 기록 보관소(RecordArchive) 필드 오브젝트.
    /// 관측선이 정선 상태일 때 클릭하면:
    ///   1. VesselHull 내구도를 회복합니다.
    ///   2. ObservationJournal의 Pending 레코드를 Archived 목록으로 이동합니다.
    /// cooldownTime 초 후 재활성화됩니다.
    /// SkipFishing 연동은 별도 UI 버튼으로 처리하므로 여기서는 직접 호출하지 않습니다.
    /// </summary>
    public class RecordArchive : InteractableObject
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("RecordArchive")]
        [Tooltip("상호작용 시 회복할 내구도 량 (현재 미사용 — 항상 MaxDurability 100% 회복)")]
        [SerializeField] private float recoverAmount = 100f;

        [Tooltip("상호작용 후 재활성화까지 대기 시간(초)")]
        [SerializeField] private float cooldownTime = 60f;

        [Header("Dependencies")]
        [SerializeField] private FishingPhaseController fishingPhaseController;
        [SerializeField] private VesselController       vesselController;
        [SerializeField] private VesselHull             vesselHull;

        // ── Coroutine 추적 ───────────────────────────────────────────
        private Coroutine _cooldownCoroutine;

        // ── InteractableObject 구현 ──────────────────────────────────

        protected override void OnInteract()
        {
            // 낚시 페이즈 활성 여부 확인
            if (fishingPhaseController == null || !fishingPhaseController.IsActive)
            {
                Debug.Log("[RecordArchive] 낚시 페이즈가 비활성 상태입니다 — 무시.");
                return;
            }

            // 정선 상태 확인
            if (vesselController == null || !vesselController.IsStationary)
            {
                Debug.Log("[RecordArchive] 관측선이 정선 상태가 아닙니다 — 상호작용 불가.");
                return;
            }

            Debug.Log("[RecordArchive] 상호작용 — 내구도 회복 및 기록 저장 시작.");

            // 상호작용 즉시 비활성화
            IsInteractable = false;

            // 1. 내구도 회복 (MaxDurability 기준 100% 전량 회복)
            if (vesselHull != null)
            {
                vesselHull.Recover(vesselHull.MaxDurability);
            }
            else
            {
                Debug.LogWarning("[RecordArchive] VesselHull 참조가 없습니다. Inspector에서 연결하세요.");
            }

            // 2. 보유 기록을 보관 목록으로 이동
            ObservationJournal journal = ObservationJournal.Singleton;
            if (journal != null)
            {
                journal.ArchivePendingRecords();
                Debug.Log("[RecordArchive] 보유 기록 보관 완료.");
            }
            else
            {
                Debug.LogWarning("[RecordArchive] ObservationJournal 싱글톤을 찾을 수 없습니다 — 보관 건너뜀.");
            }

            // 쿨다운 후 재활성화
            if (_cooldownCoroutine != null)
                StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }

        // ── 코루틴 ───────────────────────────────────────────────────

        private IEnumerator CooldownRoutine()
        {
            yield return new WaitForSeconds(cooldownTime);
            IsInteractable    = true;
            _cooldownCoroutine = null;
            Debug.Log($"[RecordArchive] 재활성화 완료 ({cooldownTime}초 경과).");
        }
    }
}
