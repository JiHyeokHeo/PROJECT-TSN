using UnityEngine;

namespace TST
{
    /// <summary>
    /// 우주 관측 구역 데이터 에셋.
    /// ScriptableObject로 관리되며 Inspector에서 직접 편집할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ObservationZone_New",
        menuName = "TST/Fishing/ObservationZone",
        order    = 10)]
    public class ObservationZone : ScriptableObject
    {
        // ── 식별 ─────────────────────────────────────────────────────

        [Tooltip("구역 고유 ID (저장 데이터 키로 사용됩니다)")]
        [SerializeField] public string zoneId;

        [Tooltip("화면에 표시되는 구역 이름")]
        [SerializeField] public string zoneName;

        // ── 접근 조건 ────────────────────────────────────────────────

        [Tooltip("이 구역에 진입하기 위해 필요한 최소 렌즈 레벨")]
        [SerializeField] public int requiredLensLevel = 1;

        // ── 관측 데이터 ──────────────────────────────────────────────

        [Tooltip("이 구역에서 등장할 수 있는 레코드 타입 목록")]
        [SerializeField] public RecordType[] availableTypes;

        /// <summary>
        /// Common / Uncommon / Rare / Legendary 순서의 기본 희귀도 가중치.
        /// 합계가 1.0이 되도록 정규화됩니다.
        /// </summary>
        [Tooltip("Common / Uncommon / Rare / Legendary 순서의 희귀도 가중치 (4개 고정)")]
        [SerializeField] public float[] rarityWeights = { 0.60f, 0.28f, 0.10f, 0.02f };
    }
}
