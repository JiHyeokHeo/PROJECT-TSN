using UnityEngine;

namespace TST
{
    /// <summary>
    /// 관측선 8방향 스프라이트 교체 컨트롤러.
    /// VesselController.FacingAngle을 읽어 45° 단위 8방향 스프라이트를 선택합니다.
    ///
    /// 스프라이트 배열 인덱스 순서:
    ///   0=N(0°)  1=NE(45°)  2=E(90°)  3=SE(135°)
    ///   4=S(180°)  5=SW(225°)  6=W(270°)  7=NW(315°)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class VesselSpriteController : MonoBehaviour
    {
        // ── 설정 ─────────────────────────────────────────────────────
        [Header("8-Direction Sprites")]
        [Tooltip("N / NE / E / SE / S / SW / W / NW 순서로 8개 스프라이트를 연결하세요.")]
        [SerializeField] private Sprite[] directionSprites = new Sprite[8];

        // ── 내부 ─────────────────────────────────────────────────────
        private SpriteRenderer _renderer;
        private int            _lastDirIndex = -1;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (VesselController.Singleton == null) return;

            int dirIndex = AngleToDirectionIndex(VesselController.Singleton.FacingAngle);
            if (dirIndex == _lastDirIndex) return;

            _lastDirIndex = dirIndex;

            if (directionSprites == null
                || dirIndex >= directionSprites.Length
                || directionSprites[dirIndex] == null)
                return;

            _renderer.sprite = directionSprites[dirIndex];
        }

        /// <summary>World Y 각도를 0~7 인덱스로 변환합니다 (45° 구간, 반올림).</summary>
        private static int AngleToDirectionIndex(float angle)
        {
            // 각 구간 경계에서 반올림: 0°=N, 45°=NE, ..., 315°=NW
            int index = Mathf.RoundToInt(angle / 45f) % 8;
            return (index + 8) % 8; // 음수 방지
        }
    }
}
