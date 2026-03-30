using UnityEngine;

namespace TST
{
    /// <summary>
    /// 프롤로그와 엔딩 컷신의 재생 진입점.
    ///
    /// 호출 측:
    ///   TitleUI.OnNewGame          → GameFlowDirector.Singleton.PlayPrologue()
    ///   NightTelescopePopupUI "…" → GameFlowDirector.Singleton.PlayEnding()
    ///
    /// Inspector 와이어링:
    ///   prologueData — 프롤로그 StoryData SO
    ///   endingData   — 엔딩 StoryData SO
    /// </summary>
    public class GameFlowDirector : SingletonBase<GameFlowDirector>
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private StoryData prologueData;
        [SerializeField] private StoryData endingData;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>프롤로그 컷신을 재생하고 완료 시 DayAttic으로 전환합니다.</summary>
        public void PlayPrologue()
        {
            if (prologueData == null)
            {
                Debug.LogWarning("[GameFlowDirector] prologueData가 할당되지 않았습니다. 컷신 없이 DayAttic으로 전환합니다.");
                PhaseManager.Singleton.TransitionTo(GamePhase.DayAttic);
                return;
            }

            var cutscene = UIManager.Show<CutsceneController>(UIList.Panel_Cutscene);
            cutscene?.PlayCutscene(prologueData, () =>
            {
                PhaseManager.Singleton.TransitionTo(GamePhase.DayAttic);
            });
        }

        /// <summary>엔딩 컷신을 재생하고 완료 시 타이틀로 돌아갑니다.</summary>
        public void PlayEnding()
        {
            if (endingData == null)
            {
                Debug.LogWarning("[GameFlowDirector] endingData가 할당되지 않았습니다. 타이틀로 이동합니다.");
                UIManager.Show<UIBase>(UIList.Panel_Title);
                return;
            }

            var cutscene = UIManager.Show<CutsceneController>(UIList.Panel_Cutscene);
            cutscene?.PlayCutscene(endingData, () =>
            {
                UIManager.Show<UIBase>(UIList.Panel_Title);
            });
        }
    }
}
