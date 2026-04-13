using System.Collections;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 위험 요소(Hazard) 필드 오브젝트.
    /// isTrigger Collider2D를 통해 관측선과 충돌하면 VesselHull에 피해를 줍니다.
    /// 관측선 오브젝트는 "Vessel" 태그로 식별합니다.
    ///
    /// 소실/리스폰:
    ///   - 최초 접촉 후 despawnDelay 초 뒤 오브젝트 비활성화
    ///   - 비활성화 후 respawnDelay 초 뒤 재활성화 및 상태 초기화
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("Hazard")]
        [Tooltip("관측선에 입히는 피해량")]
        [SerializeField] private float damageAmount = 10f;

        [Tooltip("한 번 충돌 후 다음 피해를 입힐 때까지 대기 시간(초). 0 = 매 프레임 중복 피해 없음 보호용 최소값 사용.")]
        [SerializeField] private float damageCooldown = 1f;

        [Header("Despawn / Respawn")]
        [Tooltip("최초 접촉 후 오브젝트 소실까지 대기 시간(초)")]
        [SerializeField] private float despawnDelay = 3f;

        [Tooltip("소실 후 리스폰까지 대기 시간(초)")]
        [SerializeField] private float respawnDelay = 10f;

        [Header("Dependencies")]
        [SerializeField] private VesselHull             vesselHull;
        [SerializeField] private FishingPhaseController fishingPhaseController;

        // ── 태그 상수 ────────────────────────────────────────────────
        private const string VesselTag = "Vessel";

        // ── 런타임 상태 ──────────────────────────────────────────────
        private float     _lastDamageTime  = -999f;
        private bool      _firstContact    = false;
        private Coroutine _despawnRoutine;

        // ── Unity 이벤트 ─────────────────────────────────────────────

        private void OnEnable()
        {
            // 재활성화 시 상태 초기화
            ResetState();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(VesselTag)) return;
            HandleContact();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(VesselTag)) return;
            TryDealDamage();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void HandleContact()
        {
            TryDealDamage();

            // 최초 접촉 시 카메라 쉐이킹 + 경고등 + 소실 코루틴 시작
            if (!_firstContact)
            {
                _firstContact = true;

                // 카메라 쉐이킹
                if (VesselCameraController.Singleton != null)
                    VesselCameraController.Singleton.ShakeCamera(0.5f);

                // 경고등 UI
                FishingHudUI.Singleton?.ShowWarningLight();

                // 소실 코루틴 시작
                if (_despawnRoutine != null)
                    StopCoroutine(_despawnRoutine);
                _despawnRoutine = StartCoroutine(DespawnRoutine());
            }
        }

        private void TryDealDamage()
        {
            float cooldown = Mathf.Max(damageCooldown, 0.1f);
            if (Time.time - _lastDamageTime < cooldown) return;

            _lastDamageTime = Time.time;

            if (vesselHull == null)
            {
                Debug.LogWarning("[Hazard] VesselHull 참조가 없습니다. Inspector에서 연결하세요.");
                return;
            }

            vesselHull.TakeDamage(damageAmount);
            Debug.Log($"[Hazard] '{name}' 충돌 — 피해: {damageAmount}");
        }

        private void ResetState()
        {
            _firstContact   = false;
            _lastDamageTime = -999f;
            _despawnRoutine = null;
        }

        // ── 코루틴 ───────────────────────────────────────────────────

        private IEnumerator DespawnRoutine()
        {
            yield return new WaitForSeconds(despawnDelay);

            Debug.Log($"[Hazard] '{name}' 소실 — {respawnDelay}초 뒤 리스폰.");

            // SetActive(false) 전에 리스폰 코루틴을 위임합니다.
            // SetActive(false)가 호출되는 순간 이 코루틴은 중단되므로
            // 반드시 위임이 먼저여야 합니다.
            if (fishingPhaseController != null)
                fishingPhaseController.StartCoroutine(RespawnRoutine(gameObject, respawnDelay));

            gameObject.SetActive(false);
        }

        private static IEnumerator RespawnRoutine(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (target != null)
            {
                target.SetActive(true);
                Debug.Log($"[Hazard] '{target.name}' 리스폰 완료.");
            }
        }
    }
}
