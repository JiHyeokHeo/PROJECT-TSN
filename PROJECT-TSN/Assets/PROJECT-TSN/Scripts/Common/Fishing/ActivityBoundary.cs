using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 활동 경계선 시스템.
    /// 관측선이 boundaryRadius를 초과하면 경고 이벤트와 암전 강도를 발행하고,
    /// returnTimeLimit 초 이내에 복귀하지 않으면 VesselHull.TakeDamage를 반복 호출해 침몰시킵니다.
    ///
    /// [이벤트]
    ///   OnBoundaryVignetteChanged(float)  — 0 = 정상, 1 = 완전 암전
    ///   OnBoundaryWarningChanged(bool)    — true = 경계 초과 경고, false = 경고 해제
    ///
    /// [Inspector 연결]
    ///   1. boundaryCenter  : 경계 원점 (월드 좌표). 보통 세션 시작 위치.
    ///   2. boundaryRadius  : 허용 반경 (유닛).
    ///   3. returnTimeLimit : 경계 초과 후 침몰 판정까지 유예 시간 (초).
    ///   4. sinkDamage      : returnTimeLimit 초과 시 VesselHull에 가하는 피해량.
    ///   5. vesselTransform : VesselController Transform. 비워두면 자동 참조.
    /// </summary>
    public class ActivityBoundary : MonoBehaviour
    {
        // ── 이벤트 ────────────────────────────────────────────────────

        /// <summary>
        /// 암전 강도 변화 이벤트.
        /// 0 = 정상(경계 내), 1 = returnTimeLimit 도달(완전 암전).
        /// Post Processing Volume 의 Vignette intensity 등에 연결합니다.
        /// </summary>
        public event Action<float> OnBoundaryVignetteChanged;

        /// <summary>
        /// 경고 상태 변화 이벤트.
        /// true  = 경계 초과 → 경고 UI 표시
        /// false = 경계 내 복귀 → 경고 UI 해제
        /// </summary>
        public event Action<bool> OnBoundaryWarningChanged;

        // ── Inspector 필드 ────────────────────────────────────────────

        [Header("Boundary Shape")]
        [Tooltip("경계 원점 (월드 좌표). SetBoundaryCenter()로 런타임 갱신 가능.")]
        [SerializeField] private Vector3 boundaryCenter = Vector3.zero;

        [Tooltip("경계 반경 (유닛).")]
        [SerializeField] private float boundaryRadius = 50f;

        [Header("Timeout")]
        [Tooltip("경계 초과 후 복귀 없이 이 시간(초)이 지나면 침몰 피해를 줍니다.")]
        [SerializeField] private float returnTimeLimit = 10f;

        [Tooltip("returnTimeLimit 초과 시 VesselHull에 가하는 피해량. 100 이상이면 즉시 침몰.")]
        [SerializeField] private float sinkDamage = 100f;

        [Header("References")]
        [Tooltip("관측선 Transform. 비워두면 VesselController 참조에서 자동 설정.")]
        [SerializeField] private Transform vesselTransform;

        [Tooltip("VesselController. vesselTransform이 비어 있을 때 transform을 자동 참조합니다.")]
        [SerializeField] private VesselController vesselController;

        [SerializeField] private VesselHull             vesselHull;
        [SerializeField] private FishingPhaseController fishingPhaseController;

        // ── 프로퍼티 ─────────────────────────────────────────────────

        /// <summary>현재 경계 초과 여부.</summary>
        public bool IsOutOfBounds { get; private set; }

        /// <summary>경계 초과 누적 시간 (초). 경계 내 복귀 시 0으로 리셋됩니다.</summary>
        public float OutOfBoundsTimer { get; private set; }

        /// <summary>암전 강도 (0~1). returnTimeLimit 대비 OutOfBoundsTimer 비율.</summary>
        public float VignetteIntensity => IsOutOfBounds
            ? Mathf.Clamp01(OutOfBoundsTimer / returnTimeLimit)
            : 0f;

        // ── 런타임 상태 ──────────────────────────────────────────────

        private bool  _sinkDealt;         // 침몰 피해 중복 방지
        private float _lastVignette = -1f; // 직전 암전 강도 (불필요한 이벤트 억제)

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            if (vesselTransform == null && vesselController != null)
                vesselTransform = vesselController.transform;
        }

        private void Update()
        {
            if (vesselTransform == null) return;

            // 낚시 세션이 활성 상태일 때만 동작
            if (fishingPhaseController == null || !fishingPhaseController.IsActive)
            {
                if (IsOutOfBounds) ExitBoundary();
                return;
            }

            float dist       = HorizontalDistance(vesselTransform.position, boundaryCenter);
            bool  outNow     = dist > boundaryRadius;

            if (outNow)
            {
                if (!IsOutOfBounds)
                    EnterBoundaryViolation();

                OutOfBoundsTimer += Time.deltaTime;

                float vignette = Mathf.Clamp01(OutOfBoundsTimer / returnTimeLimit);
                BroadcastVignette(vignette);

                if (!_sinkDealt && OutOfBoundsTimer >= returnTimeLimit)
                    DealSinkDamage();
            }
            else
            {
                if (IsOutOfBounds)
                    ExitBoundary();
            }
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// 경계 원점을 런타임에 변경합니다.
        /// 낚시 세션 시작 시 관측선 초기 위치로 설정하는 용도로 사용합니다.
        /// </summary>
        public void SetBoundaryCenter(Vector3 center)
        {
            boundaryCenter = center;
        }

        /// <summary>
        /// 경계 시스템을 리셋합니다 (세션 시작 시 호출).
        /// OutOfBoundsTimer, 경고 상태, 암전 강도를 모두 초기화합니다.
        /// </summary>
        public void ResetBoundary()
        {
            _sinkDealt = false;

            if (IsOutOfBounds)
            {
                IsOutOfBounds    = false;
                OutOfBoundsTimer = 0f;
                OnBoundaryWarningChanged?.Invoke(false);
            }
            else
            {
                OutOfBoundsTimer = 0f;
            }

            BroadcastVignette(0f);
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void EnterBoundaryViolation()
        {
            IsOutOfBounds    = true;
            OutOfBoundsTimer = 0f;
            _sinkDealt       = false;

            Debug.LogWarning("[ActivityBoundary] 경계 초과 — 경고 발행.");
            OnBoundaryWarningChanged?.Invoke(true);
        }

        private void ExitBoundary()
        {
            IsOutOfBounds    = false;
            OutOfBoundsTimer = 0f;
            _sinkDealt       = false;

            Debug.Log("[ActivityBoundary] 경계 내 복귀 — 경고 해제.");
            OnBoundaryWarningChanged?.Invoke(false);
            BroadcastVignette(0f);
        }

        private void DealSinkDamage()
        {
            _sinkDealt = true;
            Debug.LogWarning($"[ActivityBoundary] 복귀 시간 초과 — VesselHull에 피해 {sinkDamage} 적용.");

            if (vesselHull != null)
                vesselHull.TakeDamage(sinkDamage);
            else
                fishingPhaseController?.EndFishing();
        }

        private void BroadcastVignette(float value)
        {
            // 값 변화가 없으면 이벤트 생략 (매 프레임 과도한 호출 방지)
            if (Mathf.Approximately(_lastVignette, value)) return;

            _lastVignette = value;
            OnBoundaryVignetteChanged?.Invoke(value);
        }

        /// <summary>XZ 평면 기준 수평 거리를 반환합니다.</summary>
        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 경계 원을 Scene 뷰에 표시
            Gizmos.color = Color.cyan;
            DrawCircleXZ(boundaryCenter, boundaryRadius, 64);
        }

        private static void DrawCircleXZ(Vector3 center, float radius, int segments)
        {
            float step = 360f / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float rad  = i * step * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(rad) * radius, 0f, Mathf.Sin(rad) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
