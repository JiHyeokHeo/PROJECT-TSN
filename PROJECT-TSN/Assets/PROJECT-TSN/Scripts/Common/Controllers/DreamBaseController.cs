using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈 파트 좌측 베이스 패널 (다락방 사이드뷰 + 잠든 주인공).
    /// UIList: Panel_Dream
    /// Prefab 경로: Resources/UI/Prefabs/UI.Panel_Dream
    ///
    /// 역할:
    ///   - BedObject 또는 PhaseManager 이벤트 구독자가 UIManager.Show 를 호출합니다.
    ///   - Show() 에서 Panel_DreamVN 활성화 → Panel_DreamKeySelection 표시 순서를 보장합니다.
    ///   - PhaseManager.OnPhaseChanged 를 구독해 Dream 이탈 시 패널을 닫습니다.
    ///
    /// Inspector 와이어링:
    ///   sleepAnimator — 잠든 주인공 Animator (선택적)
    /// </summary>
    public class DreamBaseController : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [Header("Optional")]
        [SerializeField] private Animator sleepAnimator;

        // ── Unity 생명주기 ───────────────────────────────────────────
        private void OnEnable()
        {
            PhaseManager.Singleton.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (PhaseManager.Singleton != null)
                PhaseManager.Singleton.OnPhaseChanged -= HandlePhaseChanged;
        }

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        /// <summary>
        /// Dream 진입 흐름의 시작점.
        /// BedObject → UIManager.Show(Panel_Dream) → 이 메서드 호출 순서가 보장됩니다.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (sleepAnimator != null)
                sleepAnimator.SetBool("IsSleeping", true);

            // 우측 VN 패널 활성화
            UIManager.Show<DreamVNPanel>(UIList.Panel_DreamVN);

            // 키(레코드) 선택 UI 표시
            UIManager.Show<DreamKeySelectionUI>(UIList.Panel_DreamKeySelection);
        }

        // ── 페이즈 핸들러 ─────────────────────────────────────────────
        private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
        {
            // Dream 페이즈 이탈 시 패널 정리
            if (oldPhase == GamePhase.Dream && newPhase != GamePhase.Dream)
            {
                OnExitDream();
            }
        }

        // ── 내부 ─────────────────────────────────────────────────────
        private void OnEnterDream()
        {
            if (sleepAnimator != null)
                sleepAnimator.SetBool("IsSleeping", true);

            // 우측 VN 패널 활성화
            UIManager.Show<DreamVNPanel>(UIList.Panel_DreamVN);

            // 키(레코드) 선택 UI 표시
            UIManager.Show<DreamKeySelectionUI>(UIList.Panel_DreamKeySelection);
        }

        // ── 내부 ─────────────────────────────────────────────────────
        private void OnExitDream()
        {
            if (sleepAnimator != null)
                sleepAnimator.SetBool("IsSleeping", false);

            UIManager.Hide<DreamVNPanel>(UIList.Panel_DreamVN);
            UIManager.Hide<DreamKeySelectionUI>(UIList.Panel_DreamKeySelection);
            Hide();
        }
    }
}
