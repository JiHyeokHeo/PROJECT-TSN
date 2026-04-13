using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 전용 하단 HUD.
    ///
    /// 표시 항목:
    ///   - 조타륜 레이어1(helmImage): VesselController.FacingAngle 에 따라 Z 축 회전
    ///   - 조타륜 레이어2(compassImage): 현재 필드의 RecordArchive 방향을 가리킴
    ///   - 내구도 바(durabilityBar): VesselHull.CurrentDurability / MaxDurability
    ///   - 속도 바(circle): VesselController.SpeedRatio (0~1)
    ///   - 경고등(warningLight): Hazard 접촉 시 1.5초 표시
    ///
    /// Inspector 와이어링:
    ///   helmImage      — 조타륜 레이어1 Image (FacingAngle 회전)
    ///   compassImage   — 조타륜 레이어2 Image (RecordArchive 방향 회전)
    ///   durabilityBar  — Image, Fill Method = Horizontal, Fill Origin = Left
    ///   circle         — 속도 표시 RectTransform
    ///   warningLight   — 경고등 GameObject (WarningLight 이름의 Image)
    /// </summary>
    public class FishingHudUI : UIBase
    {
        public static FishingHudUI Singleton { get; private set; }

        private void Awake()
        {
            Singleton = this;
        }

        private void OnDestroy()
        {
            if (Singleton == this) Singleton = null;
        }

        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private VesselController vesselController;
        [SerializeField] private VesselHull       vesselHull;

        [Header("Helm (Compass)")]
        [Tooltip("조타륜 레이어1 이미지. FacingAngle 에 따라 Z 축으로 회전합니다.")]
        [SerializeField] private Image helmImage;

        [Tooltip("조타륜 레이어2 이미지. RecordArchive 방향을 가리킵니다.")]
        [SerializeField] private Image compassImage;

        [Header("Bars")]
        [Tooltip("내구도 바 (Image, Filled Horizontal). VesselHull 이 없으면 갱신 생략.")]
        [SerializeField] private Image durabilityBar;

        [Tooltip("속도 원형 (Image, Filled Horizontal). VesselController.SpeedRatio.")]
        [SerializeField] private RectTransform circle;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float minY = -55;
        [SerializeField] float maxY = 105;
        [SerializeField] float smoothTime = 0.08f;
        float currentYVelocity;

        [Header("Warning Light")]
        [Tooltip("경고등 GameObject (FishingHudUI 하위 WarningLight Image). Hazard 접촉 시 1.5초 표시.")]
        [SerializeField] private GameObject warningLight;

        // ── 내부 상태 ─────────────────────────────────────────────────
        private Coroutine        _warningLightCoroutine;
        private RecordArchive    _recordArchive;

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            base.Show();
            BindDurabilityEvent();
            // 첫 프레임 즉시 동기화
            SyncDurability();
            // RecordArchive 참조 캐시
            _recordArchive = Object.FindFirstObjectByType<RecordArchive>();
            // 경고등 초기 비활성화
            if (warningLight != null) warningLight.SetActive(false);
        }

        public override void Hide()
        {
            UnbindDurabilityEvent();
            if (_warningLightCoroutine != null)
            {
                StopCoroutine(_warningLightCoroutine);
                _warningLightCoroutine = null;
            }
            base.Hide();
        }

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Update()
        {
            UpdateHelm();
            UpdateCompass();
            UpdateSpeedBar();
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>Hazard 접촉 시 경고등을 1.5초 동안 표시합니다.</summary>
        public void ShowWarningLight()
        {
            if (warningLight == null) return;

            if (_warningLightCoroutine != null)
                StopCoroutine(_warningLightCoroutine);

            _warningLightCoroutine = StartCoroutine(WarningLightRoutine());
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void UpdateHelm()
        {
            if (helmImage == null) return;
            if (vesselController == null) return;

            // FacingAngle(World Y) → UI Z 회전 (반전: 선수가 위를 향할 때 0°)
            float angle = -vesselController.FacingAngle;
            helmImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void UpdateCompass()
        {
            if (compassImage == null) return;
            if (vesselController == null) return;

            // RecordArchive가 없으면 회전 정지
            if (_recordArchive == null) return;

            Vector3 vesselPos  = vesselController.transform.position;
            Vector3 archivePos = _recordArchive.transform.position;

            // 관측선에서 RecordArchive로의 수평 방향 벡터
            Vector2 dir = new Vector2(archivePos.x - vesselPos.x, archivePos.z - vesselPos.z);
            if (dir.sqrMagnitude < 0.0001f) return;

            // 월드 방향각 (0° = +Z 기준 시계 방향)
            float worldAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            // UI 회전: 나침반 바늘이 위를 향할 때 0°, 시계 방향 음수
            float uiAngle = -worldAngle;
            compassImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, uiAngle);
        }

        private void UpdateSpeedBar()
        {
            if (circle == null) return;
            if (vesselController == null) return;

            float clampedRatio = Mathf.Clamp01(vesselController.SpeedRatio);
            float targetY = Mathf.Lerp(minY, maxY, clampedRatio);

            Vector2 pos = circle.anchoredPosition;
            pos.y = Mathf.SmoothDamp(pos.y, targetY, ref currentYVelocity, smoothTime);
            circle.anchoredPosition = pos;
        }

        private IEnumerator WarningLightRoutine()
        {
            warningLight.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            warningLight.SetActive(false);
            _warningLightCoroutine = null;
        }

        // ── 내구도 이벤트 바인딩 ──────────────────────────────────────

        private void BindDurabilityEvent()
        {
            if (vesselHull != null)
                vesselHull.OnDurabilityChanged += HandleDurabilityChanged;
        }

        private void UnbindDurabilityEvent()
        {
            if (vesselHull != null)
                vesselHull.OnDurabilityChanged -= HandleDurabilityChanged;
        }

        private void HandleDurabilityChanged(float newDurability)
        {
            ApplyDurability(newDurability);
        }

        /// <summary>Show() 시점에 현재 내구도 값으로 즉시 동기화.</summary>
        private void SyncDurability()
        {
            if (vesselHull == null) return;
            ApplyDurability(vesselHull.CurrentDurability);
        }

        private void ApplyDurability(float durability)
        {
            if (durabilityBar == null) return;
            if (vesselHull == null) return;

            float max = vesselHull.MaxDurability;
            durabilityBar.fillAmount = (max > 0f) ? Mathf.Clamp01(durability / max) : 0f;
        }
    }
}
