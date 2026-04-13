using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TST
{
    /// <summary>
    /// 초점 맞추기 미니게임 컨트롤러 — 세로 스크롤 그래프 + 원호형 타이밍 바 복합 방식.
    ///
    /// [흐름]
    ///   1. 세로 스크롤 그래프: ScrollGraphValue (0~1) 가 매 프레임 아래로 감소한다.
    ///      - ScrollGraphValue >= SuccessThreshold → 성공 영역
    ///      - ScrollGraphValue <  SuccessThreshold → 실패 영역
    ///   2. 원호형 타이밍 바: 포인터가 좌우 왕복하며 GreenZone 타이밍에 입력해야 한다.
    ///      - 입력 성공 → ScrollGraphValue += jumpUpAmount  (위로 상승)
    ///      - 입력 실패 → ScrollGraphValue -= jumpDownAmount (아래로 하강)
    ///      - 성공/실패 후 GreenZone 중심을 랜덤 재배치
    ///   3. 종료 조건:
    ///      - ScrollGraphValue <= 0 → 최종 실패
    ///      - ScrollGraphValue >= 1 → 최종 성공
    ///   4. 천체 등급(Rarity)에 따라 greenZoneWidth / scrollSpeed 난이도 변동
    /// </summary>
    public class FocusMiniGameController : SingletonBase<FocusMiniGameController>
    {
        // ── 아크 상수 ────────────────────────────────────────────────
        /// <summary>포인터 Z rotation 최대값 (시작 끝)</summary>
        public const float ARC_MAX_DEG   =  90f;
        /// <summary>포인터 Z rotation 최소값 (반대 끝)</summary>
        public const float ARC_MIN_DEG   = -40f;
        public const float ARC_RANGE_DEG = ARC_MAX_DEG - ARC_MIN_DEG; // 130°

        // ── GreenCenter 랜덤 범위 ────────────────────────────────────
        private const float GREEN_CENTER_MIN = 72f;
        private const float GREEN_CENTER_MAX = -30f;

        // ── 판정 상수 ────────────────────────────────────────────────
        private const float HIT_HALF_DEG = 12f;

        // ── 열거형 ────────────────────────────────────────────────────
        public enum MiniGameState { Idle, Active, Success, Fail }

        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>미니게임 완료 시 (success, 생성된 레코드 또는 null) 발행됩니다.</summary>
        public event Action<bool, ObservationRecord> OnMiniGameCompleted;

        /// <summary>미니게임 취소 시 발행됩니다. FishingGround는 소멸하지 않습니다.</summary>
        public event Action OnMiniGameCancelled;

        // ── 스크롤 그래프 설정 ───────────────────────────────────────
        [Header("Scroll Graph")]
        [Tooltip("기본 아래 스크롤 속도 (값/초). Rarity에 따라 배율이 적용됩니다.")]
        [SerializeField] private float baseScrollSpeed = 0.08f;

        [Tooltip("성공 영역 하한선 (0~1). 이 값 이상이면 성공 구간.")]
        [SerializeField, Range(0f, 1f)] private float successThreshold = 0.5f;

        [Tooltip("클릭 성공 시 ScrollGraphValue 상승량")]
        [SerializeField] private float jumpUpAmount = 0.25f;

        [Tooltip("클릭 실패 시 ScrollGraphValue 하강량")]
        [SerializeField] private float jumpDownAmount = 0.15f;

        [Tooltip("초기 ScrollGraphValue (0~1)")]
        [SerializeField, Range(0f, 1f)] private float initialGraphValue = 0.7f;

        // ── Rarity 난이도 배율 ───────────────────────────────────────
        [Header("Rarity Difficulty Multipliers (scrollSpeed x, greenZoneWidth x)")]
        [Tooltip("Common: scrollSpeed 배율")]
        [SerializeField] private float commonScrollMult   = 0.7f;
        [Tooltip("Common: greenZoneWidth 배율")]
        [SerializeField] private float commonZoneMult     = 1.4f;

        [Tooltip("Uncommon: scrollSpeed 배율")]
        [SerializeField] private float uncommonScrollMult = 1.0f;
        [Tooltip("Uncommon: greenZoneWidth 배율")]
        [SerializeField] private float uncommonZoneMult   = 1.0f;

        [Tooltip("Rare: scrollSpeed 배율")]
        [SerializeField] private float rareScrollMult     = 1.3f;
        [Tooltip("Rare: greenZoneWidth 배율")]
        [SerializeField] private float rareZoneMult       = 0.75f;

        [Tooltip("Legendary: scrollSpeed 배율")]
        [SerializeField] private float legendaryScrollMult = 1.7f;
        [Tooltip("Legendary: greenZoneWidth 배율")]
        [SerializeField] private float legendaryZoneMult   = 0.50f;

        // ── 포인터(원호형 바) 설정 ───────────────────────────────────
        [Header("Pointer (Arc Bar)")]
        [Tooltip("포인터의 좌우 왕복 속도 (도(°)/초)")]
        [SerializeField] private float pointerSpeed = 78f;

        [Header("GreenZone Settings")]
        [Tooltip("기본 GreenZone 시각 너비 (도, °). 핸들 레벨 및 Rarity에 따라 조정됩니다.")]
        [SerializeField] private float baseGreenZoneWidth = 26f;

        [Tooltip("핸들 레벨당 GreenZone 시각 너비 증가량 (도)")]
        [SerializeField] private float greenZoneWidthPerLevel = 5.2f;

        [Tooltip("GreenZone 중심 랜덤 배치 여백 (도)")]
        [SerializeField] private float greenZoneMargin = 20f;

        // ── 프로퍼티 ─────────────────────────────────────────────────
        public MiniGameState State             { get; private set; } = MiniGameState.Idle;

        /// <summary>포인터의 현재 Z rotation 각도 (ARC_MIN_DEG ~ ARC_MAX_DEG).</summary>
        public float         PointerPos        { get; private set; }

        /// <summary>현재 GreenZone 중심의 Z rotation 각도 (GREEN_CENTER_MIN ~ GREEN_CENTER_MAX).</summary>
        public float         GreenCenter       { get; private set; }

        /// <summary>현재 GreenZone의 시각 너비 (도, °).</summary>
        public float         GreenWidth        { get; private set; }

        /// <summary>세로 스크롤 그래프의 현재 값 (0~1).</summary>
        public float         ScrollGraphValue  { get; private set; }

        /// <summary>성공 영역 하한선 (0~1).</summary>
        public float         SuccessThreshold  => successThreshold;

        /// <summary>현재 미니게임의 천체 실루엣 아이콘 (FishingGround에서 설정).</summary>
        public UnityEngine.Sprite CelestialIcon { get; private set; }

        // ── 런타임 상태 ──────────────────────────────────────────────
        private SignalPoint     currentPoint;
        private ObservationZone pendingZoneOverride;
        private int             pointerDirection = 1;
        private float           activeScrollSpeed;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// ObservationZone 기반으로 미니게임을 시작합니다.
        /// FishingGround 등 SignalPoint 없이 zone만 알고 있는 경우에 사용합니다.
        /// </summary>
        /// <param name="icon">낚아올릴 천체의 실루엣 스프라이트 (null 허용).</param>
        public void StartMinigame(ObservationZone zone, UnityEngine.Sprite icon = null)
        {
            if (zone == null)
            {
                Debug.LogError("[FocusMiniGameController] StartMinigame(zone): zone이 null입니다.");
                return;
            }

            CelestialIcon        = icon;
            pendingZoneOverride = zone;
            StartMinigame(point: null);
        }

        /// <summary>SignalPoint 클릭 시 호출됩니다. 미니게임을 초기화하고 시작합니다.</summary>
        public void StartMinigame(SignalPoint point)
        {
            if (State == MiniGameState.Active)
            {
                Debug.LogWarning("[FocusMiniGameController] 이미 미니게임이 진행 중입니다.");
                return;
            }

            currentPoint     = point;
            PointerPos       = ARC_MAX_DEG;  // 90°에서 시작
            pointerDirection = -1;           // -40° 방향으로 출발
            ScrollGraphValue  = initialGraphValue;

            // zone으로부터 Rarity를 미리 샘플해서 난이도 배율 결정
            ObservationZone resolvedZone = pendingZoneOverride
                                          ?? (point != null ? point.Zone : null);

            Rarity previewRarity = SampleRarityForDifficulty(resolvedZone);
            ApplyDifficultyByRarity(previewRarity);

            // 핸들 레벨 + Rarity 배율 적용 GreenZone 시각 너비 (도)
            int handleLevel = TelescopeData.Singleton.GetLevel(TelescopePartType.Handle);
            float baseWidth = baseGreenZoneWidth + handleLevel * greenZoneWidthPerLevel;
            GreenWidth = Mathf.Clamp(baseWidth * GetZoneMultByRarity(previewRarity), 6.5f, 104f);

            RandomizeGreenCenter();

            State = MiniGameState.Active;

            UIManager.Show<UIBase>(UIList.Popup_FocusMinigame);
        }

        // ── 공개 취소 API ────────────────────────────────────────────

        /// <summary>
        /// 미니게임을 취소합니다. UI가 닫히고 FishingGround는 소멸하지 않습니다.
        /// OnMiniGameCompleted 이벤트는 발행되지 않고, OnMiniGameCancelled가 발행됩니다.
        /// </summary>
        public void CancelMinigame()
        {
            if (State != MiniGameState.Active) return;

            UIManager.Hide<UIBase>(UIList.Popup_FocusMinigame);

            State                = MiniGameState.Idle;
            currentPoint        = null;
            pendingZoneOverride = null;
            CelestialIcon        = null;

            Debug.Log("[FocusMiniGameController] 미니게임 취소 — FishingGround 유지.");
            OnMiniGameCancelled?.Invoke();
        }

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Update()
        {
            if (State != MiniGameState.Active) return;

            var mouse = Mouse.current;
            var kb    = Keyboard.current;

            // M2(마우스 우클릭) 또는 Esc 입력 시 취소
            bool cancelInput = (mouse != null && mouse.rightButton.wasPressedThisFrame)
                             || (kb    != null && kb.escapeKey.wasPressedThisFrame);
            if (cancelInput)
            {
                CancelMinigame();
                return;
            }

            // 마우스 좌클릭 시 OnClickAttempt 실행
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                OnClickAttempt();
            }

            // 세로 스크롤 그래프 — 매 프레임 아래로 이동
            ScrollGraphValue -= activeScrollSpeed * Time.deltaTime;

            // 종료 조건 검사
            if (ScrollGraphValue <= 0f)
            {
                ScrollGraphValue = 0f;
                EndMinigame(success: false);
                return;
            }
            if (ScrollGraphValue >= 1f)
            {
                ScrollGraphValue = 1f;
                EndMinigame(success: true);
                return;
            }

            // 원호형 바 포인터 이동 (도/초), 90° ↔ -40° 왕복
            PointerPos += pointerDirection * pointerSpeed * Time.deltaTime;

            if (PointerPos >= ARC_MAX_DEG)
            {
                PointerPos       = ARC_MAX_DEG;
                pointerDirection = -1;
            }
            else if (PointerPos <= ARC_MIN_DEG)
            {
                PointerPos       = ARC_MIN_DEG;
                pointerDirection = 1;
            }
        }

        // ── 입력 ─────────────────────────────────────────────────────

        private void OnClickAttempt()
        {
            if (State != MiniGameState.Active) return;

            bool hitGreen = Mathf.Abs(PointerPos - GreenCenter) <= HIT_HALF_DEG;

            if (hitGreen)
            {
                ScrollGraphValue = Mathf.Clamp01(ScrollGraphValue + jumpUpAmount);
                // 종료 조건 즉시 확인
                if (ScrollGraphValue >= 1f)
                {
                    ScrollGraphValue = 1f;
                    EndMinigame(success: true);
                    return;
                }
            }
            else
            {
                ScrollGraphValue = Mathf.Clamp01(ScrollGraphValue - jumpDownAmount);
                if (ScrollGraphValue <= 0f)
                {
                    ScrollGraphValue = 0f;
                    EndMinigame(success: false);
                    return;
                }
            }

            // 성공/실패 후 GreenZone 랜덤 재배치
            RandomizeGreenCenter();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void EndMinigame(bool success)
        {
            State = success ? MiniGameState.Success : MiniGameState.Fail;

            ObservationRecord record = null;

            ObservationZone resolvedZone = pendingZoneOverride
                                          ?? (currentPoint != null ? currentPoint.Zone : null);

            if (success && resolvedZone != null)
            {
                record = GenerateRecord(resolvedZone);
                ObservationJournal.Singleton.AddRecord(record);
                Debug.Log($"[FocusMiniGameController] 관측 성공: {record.name} ({record.rarity})");
            }
            else
            {
                Debug.Log("[FocusMiniGameController] 관측 실패 또는 zone 없음 — 레코드 없음.");
            }

            OnMiniGameCompleted?.Invoke(success, record);

            UIManager.Hide<UIBase>(UIList.Popup_FocusMinigame);

            State                = MiniGameState.Idle;
            currentPoint        = null;
            pendingZoneOverride = null;
            CelestialIcon        = null;
        }

        /// <summary>GreenZone 중심을 -72° ~ 30° 사이에서 랜덤으로 재배치합니다.</summary>
        private void RandomizeGreenCenter()
        {
            GreenCenter = UnityEngine.Random.Range(GREEN_CENTER_MIN, GREEN_CENTER_MAX);
        }

        /// <summary>Rarity에 따른 scrollSpeed 배율과 greenZone 배율을 적용합니다.</summary>
        private void ApplyDifficultyByRarity(Rarity rarity)
        {
            activeScrollSpeed = baseScrollSpeed * GetScrollMultByRarity(rarity);
        }

        private float GetScrollMultByRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:    return commonScrollMult;
                case Rarity.Uncommon:  return uncommonScrollMult;
                case Rarity.Rare:      return rareScrollMult;
                case Rarity.Legendary: return legendaryScrollMult;
                default:               return 1f;
            }
        }

        private float GetZoneMultByRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:    return commonZoneMult;
                case Rarity.Uncommon:  return uncommonZoneMult;
                case Rarity.Rare:      return rareZoneMult;
                case Rarity.Legendary: return legendaryZoneMult;
                default:               return 1f;
            }
        }

        /// <summary>
        /// 미니게임 시작 시 난이도 결정을 위해 zone의 rarityWeights로 Rarity를 미리 샘플합니다.
        /// zone이 null이면 Common을 반환합니다.
        /// </summary>
        private static Rarity SampleRarityForDifficulty(ObservationZone zone)
        {
            if (zone == null || zone.rarityWeights == null || zone.rarityWeights.Length < 4)
                return Rarity.Common;

            return SampleRarity(zone.rarityWeights);
        }

        /// <summary>
        /// zone의 availableTypes, rarityWeights, TelescopeData 보너스를 종합해
        /// ObservationRecord를 확률 기반으로 생성합니다.
        /// </summary>
        public ObservationRecord GenerateRecord(ObservationZone zone)
        {
            if (zone == null)
            {
                Debug.LogError("[FocusMiniGameController] GenerateRecord: zone이 null입니다.");
                return null;
            }

            // ── RecordType 결정 ──────────────────────────────────────
            RecordType type = zone.availableTypes[UnityEngine.Random.Range(0, zone.availableTypes.Length)];

            // ── Rarity 결정 ──────────────────────────────────────────
            float rareBonus = TelescopeData.Singleton.GetRareBonus(type);

            float[] weights = new float[zone.rarityWeights.Length];
            Array.Copy(zone.rarityWeights, weights, weights.Length);

            if (weights.Length >= 4)
            {
                float bonus    = rareBonus - 1f;
                float transfer = Mathf.Min(weights[0], bonus * 0.5f);
                weights[0] -= transfer;
                weights[2] += transfer * 0.5f;
                weights[3] += transfer * 0.5f;
            }

            Rarity rarity = SampleRarity(weights);

            int    randomSuffix = UnityEngine.Random.Range(100, 10000);
            string recordId     = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            string recordName   = $"Unknown {type} #{randomSuffix}";
            string recordDesc   = $"분류 미상의 {type} 신호. 구역: {zone.zoneId}. 추가 분석이 필요합니다.";
            return new ObservationRecord(recordId, recordName, type, rarity, recordDesc);
        }

        private static Rarity SampleRarity(float[] weights)
        {
            float total = 0f;
            foreach (float w in weights) total += w;

            float roll       = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return (Rarity)i;
            }

            return Rarity.Common;
        }
    }
}
