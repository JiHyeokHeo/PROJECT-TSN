using System;
using UnityEngine;

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
        // ── 열거형 ────────────────────────────────────────────────────
        public enum MiniGameState { Idle, Active, Success, Fail }

        // ── 이벤트 ────────────────────────────────────────────────────
        /// <summary>미니게임 완료 시 (success, 생성된 레코드 또는 null) 발행됩니다.</summary>
        public event Action<bool, ObservationRecord> OnMiniGameCompleted;

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
        [Tooltip("포인터의 좌우 왕복 속도 (정규화 좌표 단위/초)")]
        [SerializeField] private float pointerSpeed = 0.6f;

        [Header("GreenZone Settings")]
        [Tooltip("기본 GreenZone 너비 (정규화 0~1). 핸들 레벨 및 Rarity에 따라 조정됩니다.")]
        [SerializeField] private float baseGreenZoneWidth = 0.20f;

        [Tooltip("핸들 레벨당 GreenZone 너비 증가량")]
        [SerializeField] private float greenZoneWidthPerLevel = 0.04f;

        [Tooltip("GreenZone 중심 랜덤 배치 가능 범위 (min, max). 0~1 정규화.")]
        [SerializeField, Range(0f, 0.5f)] private float greenZoneMargin = 0.15f;

        // ── 프로퍼티 ─────────────────────────────────────────────────
        public MiniGameState State             { get; private set; } = MiniGameState.Idle;

        /// <summary>포인터의 현재 정규화 위치 (0 = 왼쪽 끝, 1 = 오른쪽 끝).</summary>
        public float         PointerPos        { get; private set; }

        /// <summary>현재 GreenZone의 중심 위치 (정규화).</summary>
        public float         GreenCenter       { get; private set; }

        /// <summary>현재 GreenZone의 너비 (정규화).</summary>
        public float         GreenWidth        { get; private set; }

        /// <summary>세로 스크롤 그래프의 현재 값 (0~1).</summary>
        public float         ScrollGraphValue  { get; private set; }

        /// <summary>성공 영역 하한선 (0~1).</summary>
        public float         SuccessThreshold  => successThreshold;

        // ── 런타임 상태 ──────────────────────────────────────────────
        private SignalPoint     _currentPoint;
        private ObservationZone _pendingZoneOverride;
        private int             _pointerDirection = 1;
        private float           _activeScrollSpeed;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// ObservationZone 기반으로 미니게임을 시작합니다.
        /// FishingGround 등 SignalPoint 없이 zone만 알고 있는 경우에 사용합니다.
        /// </summary>
        public void StartMinigame(ObservationZone zone)
        {
            if (zone == null)
            {
                Debug.LogError("[FocusMiniGameController] StartMinigame(zone): zone이 null입니다.");
                return;
            }

            _pendingZoneOverride = zone;
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

            _currentPoint     = point;
            PointerPos        = 0f;
            _pointerDirection = 1;
            ScrollGraphValue  = initialGraphValue;

            // zone으로부터 Rarity를 미리 샘플해서 난이도 배율 결정
            ObservationZone resolvedZone = _pendingZoneOverride
                                          ?? (point != null ? point.Zone : null);

            Rarity previewRarity = SampleRarityForDifficulty(resolvedZone);
            ApplyDifficultyByRarity(previewRarity);

            // 핸들 레벨 + Rarity 배율 적용 GreenZone 너비
            int handleLevel = TelescopeData.Singleton.GetLevel(TelescopePartType.Handle);
            float baseWidth = Mathf.Clamp01(baseGreenZoneWidth + handleLevel * greenZoneWidthPerLevel);
            GreenWidth = Mathf.Clamp(baseWidth * GetZoneMultByRarity(previewRarity), 0.05f, 0.8f);

            RandomizeGreenCenter();

            State = MiniGameState.Active;

            UIManager.Show<UIBase>(UIList.Popup_FocusMinigame);
            InputSystem.Singleton.OnInput_Shoot += OnClickAttempt;
        }

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Update()
        {
            if (State != MiniGameState.Active) return;

            // 세로 스크롤 그래프 — 매 프레임 아래로 이동
            ScrollGraphValue -= _activeScrollSpeed * Time.deltaTime;

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

            // 원호형 바 포인터 이동
            PointerPos += _pointerDirection * pointerSpeed * Time.deltaTime;

            if (PointerPos >= 1f)
            {
                PointerPos        = 1f;
                _pointerDirection = -1;
            }
            else if (PointerPos <= 0f)
            {
                PointerPos        = 0f;
                _pointerDirection = 1;
            }
        }

        // ── 입력 ─────────────────────────────────────────────────────

        private void OnClickAttempt()
        {
            if (State != MiniGameState.Active) return;

            float halfWidth = GreenWidth * 0.5f;
            bool hitGreen   = PointerPos >= (GreenCenter - halfWidth)
                              && PointerPos <= (GreenCenter + halfWidth);

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

            InputSystem.Singleton.OnInput_Shoot -= OnClickAttempt;

            ObservationRecord record = null;

            ObservationZone resolvedZone = _pendingZoneOverride
                                          ?? (_currentPoint != null ? _currentPoint.Zone : null);

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
            _currentPoint        = null;
            _pendingZoneOverride = null;
        }

        /// <summary>GreenZone 중심을 margin 범위를 고려해 랜덤으로 재배치합니다.</summary>
        private void RandomizeGreenCenter()
        {
            float half  = GreenWidth * 0.5f;
            float lower = Mathf.Clamp(half + greenZoneMargin, 0f, 1f);
            float upper = Mathf.Clamp(1f - half - greenZoneMargin, 0f, 1f);

            if (lower >= upper)
            {
                GreenCenter = 0.5f;
                return;
            }

            GreenCenter = UnityEngine.Random.Range(lower, upper);
        }

        /// <summary>Rarity에 따른 scrollSpeed 배율과 greenZone 배율을 적용합니다.</summary>
        private void ApplyDifficultyByRarity(Rarity rarity)
        {
            _activeScrollSpeed = baseScrollSpeed * GetScrollMultByRarity(rarity);
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
