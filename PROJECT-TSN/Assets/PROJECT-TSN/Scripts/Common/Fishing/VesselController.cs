using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// Vessel movement controller for fishing phase.
    /// W: forward, S: backward (limited), A/D: turn.
    /// </summary>
    public class VesselController : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float maxForwardSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float backwardSpeedRatio = 0.3f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float deceleration = 8f;

        [Header("Turning")]
        [SerializeField] private float turnSpeed = 90f;

        [Header("Stationary")]
        [SerializeField] private float stationaryThreshold = 0.1f;

        [Header("Visual")]
        [SerializeField] private Transform visual;

        [Header("Phase")]
        [SerializeField] private GameObject vesselRoot;

        public float CurrentSpeed { get; private set; }
        public float FacingAngle { get; private set; }
        public bool IsStationary => Mathf.Abs(CurrentSpeed) <= stationaryThreshold;
        public float SpeedRatio => Mathf.Abs(CurrentSpeed) / maxForwardSpeed;

        private float maxBackwardSpeed;

        private void Awake()
        {
            maxBackwardSpeed = maxForwardSpeed * backwardSpeedRatio;
            FacingAngle = transform.eulerAngles.y;

            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;

            if (vesselRoot != null)
            {
                vesselRoot.SetActive(PhaseManager.Singleton.CurrentPhase == GamePhase.Fishing);
            }

        }

        private void OnDestroy()
        {
            if (PhaseManager.Singleton != null)
            {
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            bool isFishing = newPhase == GamePhase.Fishing;

            if (vesselRoot != null)
            {
                vesselRoot.SetActive(isFishing);
            }

            if (!isFishing)
            {
                CurrentSpeed = 0f;
            }
        }

        private void Update()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Fishing)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            HandleTurning(keyboard);
            HandleThrottle(keyboard);
            ApplyMovement();
        }

        private void LateUpdate()
        {
            if (PhaseManager.Singleton.CurrentPhase != GamePhase.Fishing)
            {
                return;
            }

            if (visual != null && Camera.main != null)
            {
                visual.rotation = Camera.main.transform.rotation;
            }
        }

        private void HandleTurning(Keyboard keyboard)
        {
            float turnInput = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                turnInput = -1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                turnInput = 1f;
            }

            if (turnInput == 0f)
            {
                return;
            }

            FacingAngle += turnInput * turnSpeed * Time.deltaTime;
            FacingAngle = (FacingAngle % 360f + 360f) % 360f;
            transform.rotation = Quaternion.Euler(0f, FacingAngle, 0f);
        }

        private void HandleThrottle(Keyboard keyboard)
        {
            bool forward = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
            bool backward = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;

            if (forward && !backward)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, maxForwardSpeed, acceleration * Time.deltaTime);
            }
            else if (backward && !forward)
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, -maxBackwardSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, deceleration * Time.deltaTime);
            }
        }

        private void ApplyMovement()
        {
            transform.position += transform.forward * (CurrentSpeed * Time.deltaTime);
        }

        public void Initialize(Vector3 position, float facingAngle = 0f)
        {
            transform.position = position;
            FacingAngle = facingAngle;
            CurrentSpeed = 0f;
            transform.rotation = Quaternion.Euler(0f, FacingAngle, 0f);
        }

        public void ForceStop()
        {
            CurrentSpeed = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxBackwardSpeed = maxForwardSpeed * backwardSpeedRatio;
        }
#endif
    }
}
