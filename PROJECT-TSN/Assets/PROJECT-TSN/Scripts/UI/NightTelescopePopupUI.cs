using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 밤 망원경 팝업 UI.
    /// UIList: Popup_NightTelescope
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_NightTelescope
    ///
    /// 버튼 3종:
    ///   fishingBtn  — "우주의 비밀을 낚아올리자" → Fishing 페이즈 전환
    ///   waitBtn     — "잠깐 기다려" → 팝업 닫기
    ///   endingBtn   — "…" → 엔딩 컷신 재생 (Enlightenment >= 80 일 때만 활성)
    ///
    /// Inspector 와이어링:
    ///   fishingBtn, waitBtn, endingBtn — Button 컴포넌트
    /// </summary>
    public class NightTelescopePopupUI : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private FishingTransitionController fishingTransitionController;

        [Header("Buttons")]
        [SerializeField] private Button fishingBtn;
        [SerializeField] private Button waitBtn;
        [SerializeField] private Button endingBtn;

        // ── 엔딩 개방 임계값 ─────────────────────────────────────────
        private const float EndingEnlightenmentThreshold = 80f;

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            if (fishingBtn != null)
                fishingBtn.onClick.AddListener(OnFishingClicked);

            if (waitBtn != null)
                waitBtn.onClick.AddListener(OnWaitClicked);

            if (endingBtn != null)
                endingBtn.onClick.AddListener(OnEndingClicked);
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// Show 직후 TelescopeObject에서 호출해 씬 의존성을 주입합니다.
        /// UIManager가 Resources에서 인스턴스화하므로 prefab에 직접 연결할 수 없어
        /// 호출자가 컨텍스트를 알고 있는 Show 시점에 넘겨주는 방식을 사용합니다.
        /// </summary>
        public void Initialize(FishingTransitionController controller)
        {
            fishingTransitionController = controller;
        }

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            base.Show();
            RefreshEndingButton();
        }

        // ── 버튼 핸들러 ──────────────────────────────────────────────

        private void OnFishingClicked()
        {
            Hide();
            fishingTransitionController?.BeginTransition();
        }

        private void OnWaitClicked()
        {
            Hide();
        }

        private void OnEndingClicked()
        {
            Hide();
            GameFlowDirector.Singleton.PlayEnding();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        /// <summary>Enlightenment 수치가 임계값 이상일 때만 엔딩 버튼을 활성화합니다.</summary>
        private void RefreshEndingButton()
        {
            if (endingBtn == null) return;

            bool canEnd = PlayerParameters.Singleton != null
                       && PlayerParameters.Singleton.Enlightenment >= EndingEnlightenmentThreshold;

            endingBtn.interactable = canEnd;
        }
    }
}
