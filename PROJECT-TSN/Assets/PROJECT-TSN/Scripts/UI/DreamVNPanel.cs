using System;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈 파트 우측 VN 패널.
    /// UIList: Panel_DreamVN
    /// Prefab 경로: Resources/UI/Prefabs/UI.Panel_DreamVN
    ///
    /// 역할:
    ///   - DialogueSystem 에 StoryData 재생을 위임합니다.
    ///   - 각 DialogueLine 의 portrait / backgroundCg 는 DialogueSystem 이
    ///     MainLayoutController.SetRightContent / SetLeftContent 를 통해 반영합니다.
    ///
    /// Inspector 와이어링:
    ///   없음 — DialogueSystem 과 MainLayoutController 참조는 코드에서 처리합니다.
    /// </summary>
    public class DreamVNPanel : UIBase
    {
        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>
        /// StoryData 를 VN 패널에서 재생합니다.
        /// DialogueSystem 이 MainLayoutController 를 통해 배경/초상화/대화를 출력합니다.
        /// 재생 완료 시 onComplete 를 호출합니다.
        /// </summary>
        public void PlayDialogue(StoryData data, Action onComplete)
        {
            if (data == null)
            {
                Debug.LogWarning("[DreamVNPanel] PlayDialogue: StoryData 가 null 입니다. onComplete 바로 호출.");
                onComplete?.Invoke();
                return;
            }

            DialogueSystem.Singleton.PlayStory(data, onComplete);
        }

        // ── UIBase 오버라이드 ─────────────────────────────────────────
        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            // 진행 중인 다이얼로그가 있으면 중단하지 않음 — 완료 후 Hide 는 DreamEventRunner 가 관리
            base.Hide();
        }
    }
}
