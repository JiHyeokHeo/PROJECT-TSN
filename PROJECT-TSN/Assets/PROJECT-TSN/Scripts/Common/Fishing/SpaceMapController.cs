using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 우주 지도 화면 컨트롤러.
    /// Inspector에서 ObservationZone ScriptableObject를 할당받아
    /// 렌즈 레벨에 따라 구역 버튼 접근을 제어합니다.
    /// </summary>
    public class SpaceMapController : MonoBehaviour
    {
        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>플레이어가 구역을 선택했을 때 발행됩니다.</summary>
        public event System.Action<ObservationZone> OnZoneSelected;

        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Observation Zones")]
        [Tooltip("사용할 ObservationZone SO 에셋 목록. 인스펙터에서 순서대로 연결하세요.")]
        [SerializeField] private List<ObservationZone> zones = new List<ObservationZone>();

        [Header("Zone Buttons")]
        [Tooltip("각 ObservationZone에 대응하는 ZoneButton 컴포넌트 목록. zones와 같은 순서로 연결하세요.")]
        [SerializeField] private List<ZoneButton> zoneButtons = new List<ZoneButton>();

        [Header("Dependencies")]
        [SerializeField] private FishingTransitionController fishingTransitionController;

        // ── 생명주기 ─────────────────────────────────────────────────

        private void OnEnable()
        {
            RefreshZoneButtons();
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// 외부(ZoneButton UI 등)에서 구역 선택을 알릴 때 호출합니다.
        /// </summary>
        public void SelectZone(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= zones.Count)
            {
                Debug.LogWarning($"[SpaceMapController] 잘못된 구역 인덱스: {zoneIndex}");
                return;
            }

            ObservationZone zone    = zones[zoneIndex];
            int accessibleCount     = TelescopeData.Singleton.GetObservableZoneCount();
            int lensLevel           = TelescopeData.Singleton.GetLevel(TelescopePartType.Lens);

            if (zoneIndex >= accessibleCount || lensLevel < zone.requiredLensLevel)
            {
                Debug.Log($"[SpaceMapController] 구역 '{zone.zoneName}' 접근 불가. 렌즈 레벨 부족.");
                return;
            }

            OnZoneSelected?.Invoke(zone);

            // Space → Fishing 전환 시작
            fishingTransitionController?.BeginFishingTransition(zone);
        }

        /// <summary>현재 등록된 구역 목록을 반환합니다.</summary>
        public IReadOnlyList<ObservationZone> GetZones() => zones;

        // ── 내부 ─────────────────────────────────────────────────────

        private void RefreshZoneButtons()
        {
            int accessibleCount = TelescopeData.Singleton.GetObservableZoneCount();
            int lensLevel       = TelescopeData.Singleton.GetLevel(TelescopePartType.Lens);

            for (int i = 0; i < zoneButtons.Count; i++)
            {
                if (zoneButtons[i] == null) continue;

                bool hasZoneData  = i < zones.Count && zones[i] != null;
                bool isAccessible = hasZoneData
                                    && i < accessibleCount
                                    && lensLevel >= zones[i].requiredLensLevel;

                zoneButtons[i].SetZone(
                    hasZoneData ? zones[i] : null,
                    isAccessible,
                    this);
            }
        }
    }

    // ── 헬퍼 컴포넌트 ────────────────────────────────────────────────

    /// <summary>
    /// 우주 지도 상의 개별 구역 버튼.
    /// SpaceMapController가 SetZone()으로 초기화합니다.
    /// </summary>
    public class ZoneButton : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private UnityEngine.UI.Text   labelText;
        [SerializeField] private UnityEngine.UI.Image  lockIcon;

        private int                _zoneIndex = -1;
        private SpaceMapController _owner;

        private void Awake()
        {
            if (button == null) button = GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        public void SetZone(ObservationZone zone, bool accessible, SpaceMapController owner)
        {
            _owner = owner;

            if (zone == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // owner.GetZones()에서 인덱스 역산
            var zoneList = owner.GetZones();
            _zoneIndex = -1;
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i] == zone) { _zoneIndex = i; break; }
            }

            if (labelText != null) labelText.text = zone.zoneName;
            if (lockIcon  != null) lockIcon.gameObject.SetActive(!accessible);
            button.interactable = accessible;
        }

        private void OnClick()
        {
            if (_owner == null || _zoneIndex < 0) return;
            _owner.SelectZone(_zoneIndex);
        }
    }
}
