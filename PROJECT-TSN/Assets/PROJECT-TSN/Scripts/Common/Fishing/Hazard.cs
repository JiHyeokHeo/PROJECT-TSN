using UnityEngine;

namespace TST
{
    /// <summary>
    /// 위험 요소(Hazard) 필드 오브젝트.
    /// isTrigger Collider2D를 통해 관측선과 충돌하면 VesselHull에 피해를 줍니다.
    /// 관측선 오브젝트는 "Vessel" 태그로 식별합니다.
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

        // ── 태그 상수 ────────────────────────────────────────────────
        private const string VesselTag = "Vessel";

        // ── 쿨다운 상태 ──────────────────────────────────────────────
        private float _lastDamageTime = -999f;

        // ── Unity 이벤트 ─────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(VesselTag)) return;
            TryDealDamage();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(VesselTag)) return;
            TryDealDamage();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void TryDealDamage()
        {
            float cooldown = Mathf.Max(damageCooldown, 0.1f);
            if (Time.time - _lastDamageTime < cooldown) return;

            _lastDamageTime = Time.time;

            VesselHull hull = VesselHull.Singleton;
            if (hull == null)
            {
                Debug.LogWarning("[Hazard] VesselHull 싱글톤을 찾을 수 없습니다.");
                return;
            }

            hull.TakeDamage(damageAmount);
            Debug.Log($"[Hazard] '{name}' 충돌 — 피해: {damageAmount}");
        }
    }
}
