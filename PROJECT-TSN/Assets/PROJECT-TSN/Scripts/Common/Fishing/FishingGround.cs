using UnityEngine;

namespace TST
{
    /// <summary>
    /// 어장(FishingGround) 필드 오브젝트.
    /// 관측선이 정선 상태일 때 상호작용하면 초점 맞추기 미니게임을 시작합니다.
    ///
    /// - 성공 또는 실패 시 → 이 오브젝트를 소멸(Destroy)시킵니다.
    /// - [M2][Esc]로 취소 시 → 이 오브젝트를 유지하고 다시 상호작용 가능 상태로 복원합니다.
    /// </summary>
    public class FishingGround : InteractableObject
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("FishingGround")]
        [Tooltip("이 어장이 속한 관측 구역. 미니게임 레코드 생성에 사용됩니다.")]
        [SerializeField] private ObservationZone zone;

        [Tooltip("낚아올릴 천체의 실루엣 스프라이트. 미니게임 UI 아이콘으로 표시됩니다.")]
        [SerializeField] private Sprite celestialSilhouette;

        [Header("Dependencies")]
        [SerializeField] private FishingPhaseController  fishingPhaseController;
        [SerializeField] private VesselController        vesselController;
        [SerializeField] private FocusMiniGameController focusMiniGameController;

        // ── InteractableObject 구현 ──────────────────────────────────

        protected override void OnInteract()
        {
            // 낚시 페이즈 활성 여부 확인
            if (fishingPhaseController == null || !fishingPhaseController.IsActive)
            {
                Debug.Log("[FishingGround] 낚시 페이즈가 비활성 상태입니다 — 무시.");
                return;
            }

            // 정선 상태 확인
            if (vesselController == null || !vesselController.IsStationary)
            {
                Debug.Log("[FishingGround] 관측선이 정선 상태가 아닙니다 — 상호작용 불가.");
                return;
            }

            if (focusMiniGameController == null)
            {
                Debug.LogError("[FishingGround] FocusMiniGameController가 연결되지 않았습니다.");
                return;
            }

            // zone 없으면 현재 페이즈의 구역으로 폴백
            ObservationZone resolvedZone = zone != null ? zone : fishingPhaseController.CurrentZone;

            if (resolvedZone == null)
            {
                Debug.LogError("[FishingGround] ObservationZone이 설정되지 않았고 현재 페이즈 구역도 없습니다.");
                return;
            }

            Debug.Log($"[FishingGround] 미니게임 시작 — 구역: {resolvedZone.zoneName}");

            // 미니게임이 진행되는 동안 상호작용 비활성화
            IsInteractable = false;

            // 이벤트 구독 (중복 방지를 위해 먼저 해제)
            focusMiniGameController.OnMiniGameCompleted -= OnMinigameCompleted;
            focusMiniGameController.OnMiniGameCancelled -= OnMinigameCancelled;
            focusMiniGameController.OnMiniGameCompleted += OnMinigameCompleted;
            focusMiniGameController.OnMiniGameCancelled += OnMinigameCancelled;

            // zone + 실루엣 아이콘 기반 미니게임 시작
            focusMiniGameController.StartMinigame(resolvedZone, celestialSilhouette);
        }

        // ── 미니게임 결과 처리 ───────────────────────────────────────

        private void OnMinigameCompleted(bool success, ObservationRecord record)
        {
            UnsubscribeEvents();

            if (success)
                Debug.Log($"[FishingGround] 낚시 성공 — 오브젝트 소멸. 레코드: {record?.name}");
            else
                Debug.Log("[FishingGround] 낚시 실패 — 오브젝트 소멸.");

            // 성공 또는 실패 모두 어장 오브젝트 소멸
            Destroy(gameObject);
        }

        private void OnMinigameCancelled()
        {
            UnsubscribeEvents();

            // 취소 시 어장 유지 — 다시 상호작용 가능 상태로 복원
            IsInteractable = true;
            Debug.Log("[FishingGround] 낚시 취소 — 오브젝트 유지, 상호작용 복원.");
        }

        // ── 정리 ─────────────────────────────────────────────────────

        private void UnsubscribeEvents()
        {
            if (focusMiniGameController == null) return;
            focusMiniGameController.OnMiniGameCompleted -= OnMinigameCompleted;
            focusMiniGameController.OnMiniGameCancelled -= OnMinigameCancelled;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
    }
}
