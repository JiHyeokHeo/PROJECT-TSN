using System.Collections;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 어장(FishingGround) 필드 오브젝트.
    /// 관측선이 정선 상태일 때 클릭하면 초점 맞추기 미니게임을 시작합니다.
    /// 상호작용 후 respawnTime 초 뒤에 재활성화됩니다.
    /// </summary>
    public class FishingGround : InteractableObject
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("FishingGround")]
        [Tooltip("이 어장이 속한 관측 구역. 미니게임 레코드 생성에 사용됩니다.")]
        [SerializeField] private ObservationZone zone;

        [Tooltip("상호작용 후 재활성화까지 대기 시간(초)")]
        [SerializeField] private float respawnTime = 30f;

        // ── Coroutine 추적 ───────────────────────────────────────────
        private Coroutine _respawnCoroutine;

        // ── InteractableObject 구현 ──────────────────────────────────

        protected override void OnInteract()
        {
            // 낚시 페이즈 활성 여부 확인
            FishingPhaseController fc = FishingPhaseController.Singleton;
            if (fc == null || !fc.IsActive)
            {
                Debug.Log("[FishingGround] 낚시 페이즈가 비활성 상태입니다 — 무시.");
                return;
            }

            // 정선 상태 확인
            VesselController vc = VesselController.Singleton;
            if (vc == null || !vc.IsStationary)
            {
                Debug.Log("[FishingGround] 관측선이 정선 상태가 아닙니다 — 상호작용 불가.");
                return;
            }

            // zone 없으면 현재 페이즈의 구역으로 폴백
            ObservationZone resolvedZone = zone != null ? zone : fc.CurrentZone;

            if (resolvedZone == null)
            {
                Debug.LogError("[FishingGround] ObservationZone이 설정되지 않았고 현재 페이즈 구역도 없습니다.");
                return;
            }

            Debug.Log($"[FishingGround] 미니게임 시작 — 구역: {resolvedZone.zoneName}");

            // 상호작용 즉시 비활성화 (쿨다운 후 재활성화)
            IsInteractable = false;

            // zone 기반 미니게임 시작
            FocusMiniGameController.Singleton.StartMinigame(resolvedZone);

            // 재활성화 코루틴 시작
            if (_respawnCoroutine != null)
                StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }

        // ── 코루틴 ───────────────────────────────────────────────────

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnTime);
            IsInteractable = true;
            _respawnCoroutine = null;
            Debug.Log($"[FishingGround] 재활성화 완료 ({respawnTime}초 경과).");
        }
    }
}
