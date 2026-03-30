using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    public enum SubLocation { None, Academy, University, Shop }

    /// <summary>
    /// Phase 전환 시 MainLayoutController 우측 프레임의 배경 스프라이트를 교체합니다.
    /// DayCity 내 서브 로케이션(학회/대학/상점) 진입 시에도 배경을 갱신합니다.
    /// NPC SCG 스프라이트와 나레이션 텍스트 표시도 담당합니다(선택적, null 허용).
    ///
    /// Inspector wiring:
    ///   - Assign phase background sprites in the inspector.
    ///   - npcImage / narrationLabel are optional; leave unassigned to skip.
    ///   - Requires MainLayoutController to be present in the scene (via UIManager).
    /// </summary>
    public class RightFrameContentController : SingletonBase<RightFrameContentController>
    {
        // ----------------------------------------------------------------
        //  Inspector — Phase backgrounds
        // ----------------------------------------------------------------
        [Header("Phase Background Sprites")]
        [SerializeField] private Sprite dayAtticBg;
        [SerializeField] private Sprite dayCityBg;
        [SerializeField] private Sprite academyBg;
        [SerializeField] private Sprite universityBg;
        [SerializeField] private Sprite shopBg;
        [SerializeField] private Sprite nightAtticBg;

        // ----------------------------------------------------------------
        //  Inspector — NPC view (optional)
        // ----------------------------------------------------------------
        [Header("NPC View (Optional)")]
        [SerializeField] private Image npcImage;
        [SerializeField] private TextMeshProUGUI narrationLabel;

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private SubLocation _currentSubLocation = SubLocation.None;

        // ----------------------------------------------------------------
        //  Unity lifecycle
        // ----------------------------------------------------------------
        private void OnEnable()
        {
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        // ----------------------------------------------------------------
        //  Phase handler
        // ----------------------------------------------------------------
        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            _currentSubLocation = SubLocation.None;

            Sprite bg = ResolvePhaseBackground(newPhase);
            ApplyBackground(bg);
            ClearNpc();
        }

        // ----------------------------------------------------------------
        //  Public API
        // ----------------------------------------------------------------
        /// <summary>
        /// DayCity 내 서브 로케이션 진입 시 배경을 전환합니다.
        /// SubLocation.None 이면 dayCityBg 로 복귀합니다.
        /// </summary>
        public void SetSubLocation(SubLocation loc)
        {
            _currentSubLocation = loc;

            Sprite bg = loc switch
            {
                SubLocation.Academy    => academyBg    != null ? academyBg    : dayCityBg,
                SubLocation.University => universityBg != null ? universityBg : dayCityBg,
                SubLocation.Shop       => shopBg       != null ? shopBg       : dayCityBg,
                _                      => dayCityBg
            };

            ApplyBackground(bg);
            ClearNpc();
        }

        /// <summary>
        /// NPC 스프라이트와 나레이션 텍스트를 표시합니다.
        /// sprite / text 에 null 을 전달하면 해당 요소를 숨깁니다.
        /// </summary>
        public void ShowNpc(Sprite sprite, string text)
        {
            if (npcImage != null)
            {
                npcImage.sprite  = sprite;
                npcImage.enabled = sprite != null;
            }

            if (narrationLabel != null)
            {
                narrationLabel.text    = text ?? string.Empty;
                narrationLabel.enabled = !string.IsNullOrEmpty(text);
            }
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        private Sprite ResolvePhaseBackground(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.DayAttic => dayAtticBg,
                GamePhase.DayCity  => dayCityBg,
                GamePhase.NightA   => nightAtticBg,
                GamePhase.NightB   => nightAtticBg,
                _                  => null
            };
        }

        private void ApplyBackground(Sprite sprite)
        {
            MainLayoutController layout = UIManager.Singleton.GetUI<MainLayoutController>(UIList.MainLayout);
            if (layout == null) return;

            layout.SetRightContent(sprite);
        }

        private void ClearNpc()
        {
            if (npcImage != null)
            {
                npcImage.sprite  = null;
                npcImage.enabled = false;
            }

            if (narrationLabel != null)
            {
                narrationLabel.text    = string.Empty;
                narrationLabel.enabled = false;
            }
        }
    }
}
