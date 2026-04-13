using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// 관측선 이동 컨트롤러 (DREDGE 참조).
    /// W: 전진 / S: 후진(최대 전진 속도의 30%) / A·D: 선회.
    /// 가속·감속 개념을 갖추며, 속도 0(정선 상태)에서만 능동 상호작용이 허용됩니다.
    /// Input System(Keyboard.current) 방식 사용 — PlayerInput 컴포넌트 불필요.
    /// </summary>
    public class VesselController : MonoBehaviour
    {
        // ── 설정 ─────────────────────────────────────────────────────

        [Header("Speed")]
        [Tooltip("최대 전진 속도 (유닛/초)")]
        [SerializeField] private float maxForwardSpeed = 10f;

        [Tooltip("후진 속도 상한 = 전진 최대 속도 × 이 비율 (GDD: 30%)")]
        [SerializeField, Range(0f, 1f)] private float backwardSpeedRatio = 0.3f;

        [Tooltip("가속도 (유닛/초²) — 입력이 있을 때 목표 속도로 수렴하는 비율")]
        [SerializeField] private float acceleration = 5f;

        [Tooltip("감속도 (유닛/초²) — 입력이 없을 때 0으로 수렴하는 비율")]
        [SerializeField] private float deceleration = 8f;

        [Header("Turning")]
        [Tooltip("선회 속도 (도/초)")]
        [SerializeField] private float turnSpeed = 90f;

        [Header("Stationary")]
        [Tooltip("이 속도(절댓값) 이하면 정선(IsStationary = true) 판정")]
        [SerializeField] private float stationaryThreshold = 0.1f;

        [Header("Visual")]
        [SerializeField] Transform visual;

        [Header("Phase")]
        [Tooltip("Fishing 페이즈 진입/이탈 시 토글되는 Vessel 루트 (Character의 Vessel 자식)")]
        [SerializeField] private GameObject vesselRoot;

        // ── 프로퍼티 ─────────────────────────────────────────────────

        /// <summary>현재 이동 속도. 양수 = 전진, 음수 = 후진.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>현재 바라보는 방향 (World Y 각도, 0° = +Z).</summary>
        public float FacingAngle { get; private set; }

        /// <summary>
        /// 정선 상태 여부.
        /// 능동 상호작용(어장, 기록 보관소)은 이 값이 true 일 때만 허용됩니다.
        /// </summary>
        public bool IsStationary => Mathf.Abs(CurrentSpeed) <= stationaryThreshold;

        /// <summary>현재 속도를 최대 전진 속도 기준 0~1 비율로 반환 (UI 속도 바용).</summary>
        public float SpeedRatio => Mathf.Abs(CurrentSpeed) / maxForwardSpeed;

        // ── 내부 ─────────────────────────────────────────────────────

        private float _maxBackwardSpeed;

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _maxBackwardSpeed = maxForwardSpeed * backwardSpeedRatio;
            FacingAngle       = transform.eulerAngles.y;

            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;

            // 초기 상태: Fishing 페이즈가 아니면 Vessel 비활성화
            if (vesselRoot != null)
                vesselRoot.SetActive(PhaseManager.Singleton.CurrentPhase == GamePhase.Fishing);
        }

        private void OnDestroy()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            bool isFishing = newPhase == GamePhase.Fishing;
            if (vesselRoot != null)
                vesselRoot.SetActive(isFishing);

            if (!isFishing)
                CurrentSpeed = 0f;
        }

        private void Update()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Fishing) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            HandleTurning(kb);
            HandleThrottle(kb);
            ApplyMovement();
        }

        private void LateUpdate()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Fishing) return;

            visual.rotation = Camera.main.transform.rotation;
        }

        // ── 입력 ─────────────────────────────────────────────────────

        private void HandleTurning(Keyboard kb)
        {
            float turnInput = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  turnInput = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) turnInput =  1f;

            if (turnInput == 0f) return;

            FacingAngle        += turnInput * turnSpeed * Time.deltaTime;
            FacingAngle         = (FacingAngle % 360f + 360f) % 360f;
            transform.rotation  = Quaternion.Euler(0f, FacingAngle, 0f);
        }

        private void HandleThrottle(Keyboard kb)
        {
            bool fwd = kb.wKey.isPressed || kb.upArrowKey.isPressed;
            bool bwd = kb.sKey.isPressed || kb.downArrowKey.isPressed;

            if (fwd && !bwd)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed,  maxForwardSpeed,   acceleration * Time.deltaTime);
            }
            else if (bwd && !fwd)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, -_maxBackwardSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                // 입력 없음 — 자연 감속
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, deceleration * Time.deltaTime);
            }
        }

        private void ApplyMovement()
        {
            transform.position += transform.forward * (CurrentSpeed * Time.deltaTime);
        }

        // ── 외부 API ─────────────────────────────────────────────────

        /// <summary>페이즈 시작 시 초기 위치·방향·속도를 설정합니다.</summary>
        public void Initialize(Vector3 position, float facingAngle = 0f)
        {
            transform.position = position;
            FacingAngle        = facingAngle;
            CurrentSpeed       = 0f;
            transform.rotation = Quaternion.Euler(0f, FacingAngle, 0f);
        }

        /// <summary>침몰 등 강제 종료 시 즉시 정지합니다.</summary>
        public void ForceStop()
        {
            CurrentSpeed = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Inspector에서 maxForwardSpeed 변경 시 내부 후진 속도도 즉시 갱신
            _maxBackwardSpeed = maxForwardSpeed * backwardSpeedRatio;
        }
#endif
    }
}
