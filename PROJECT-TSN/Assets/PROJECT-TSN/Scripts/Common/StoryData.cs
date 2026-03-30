using UnityEngine;

namespace TST
{
    /// <summary>
    /// 컷신 한 편의 데이터를 담는 ScriptableObject.
    /// 대사 배열, 배경 CG, 초상화, 종료 후 전환할 페이즈를 정의합니다.
    ///
    /// 인스펙터 생성: Assets 우클릭 → Create → TST/Story/StoryData
    /// </summary>
    [CreateAssetMenu(menuName = "TST/Story/StoryData")]
    public class StoryData : ScriptableObject
    {
        public string storyId;

        [System.Serializable]
        public class DialogueLine
        {
            public string speakerName;

            [TextArea(2, 5)]
            public string text;

            /// <summary>null이면 오른쪽 프레임 변경 없음.</summary>
            public Sprite portrait;

            /// <summary>null이면 이전 배경 유지.</summary>
            public Sprite backgroundCg;
        }

        public DialogueLine[] lines;

        /// <summary>컷신 종료 후 전환할 페이즈. goToTitle = true이면 무시됩니다.</summary>
        public GamePhase nextPhase;

        /// <summary>true이면 nextPhase 대신 Panel_Title을 표시합니다.</summary>
        public bool goToTitle;
    }
}
