using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈 이벤트 하나를 정의하는 ScriptableObject.
    /// 노드 기반 FSM으로 Dialogue / Choice / Dice / End 를 표현합니다.
    ///
    /// 인스펙터 생성: Assets 우클릭 → Create → TST/Dream/DreamEventData
    /// </summary>
    [CreateAssetMenu(menuName = "TST/Dream/DreamEventData")]
    public class DreamEventData : ScriptableObject
    {
        public string eventId;
        public string eventTitle;

        /// <summary>이 이벤트를 발동할 수 있는 레코드 타입 (CosmicTrace).</summary>
        public RecordType triggerRecordType;

        /// <summary>대화 시퀀스 (StoryData 재활용, 필요 시 참조용).</summary>
        public StoryData[] dialogueSequences;

        /// <summary>노드 기반 이벤트 구조.</summary>
        public DreamNode[] nodes;

        /// <summary>이 이벤트 완료 후 엔딩 조건 충족 여부를 체크할지 여부.</summary>
        public bool checkEndingOnComplete;

        // ----------------------------------------------------------------
        //  Node definition
        // ----------------------------------------------------------------
        [System.Serializable]
        public class DreamNode
        {
            public string nodeId;
            public DreamNodeType nodeType;

            /// <summary>Dialogue 노드 시 사용.</summary>
            public StoryData dialogue;

            // ── Choice 노드 ──────────────────────────────────────────
            public DreamChoice[] choices;

            // ── Dice 노드 ────────────────────────────────────────────
            /// <summary>주사위 최대값 (기본 6).</summary>
            public int diceMax = 6;

            /// <summary>되감기 가능 횟수.</summary>
            public int rewindLimit;

            /// <summary>주사위 결과 >= diceSuccessThreshold 이면 이 노드로 이동.</summary>
            public string successNodeId;
            public int diceSuccessThreshold;

            /// <summary>주사위 실패 시 이동할 노드 ID.</summary>
            public string failNodeId;

            // ── 공통 ─────────────────────────────────────────────────
            /// <summary>Choice / Dice 가 아닌 노드의 다음 노드. 빈 문자열이면 이벤트 종료.</summary>
            public string nextNodeId;
        }

        public enum DreamNodeType { Dialogue, Choice, Dice, End }

        // ----------------------------------------------------------------
        //  Choice definition
        // ----------------------------------------------------------------
        [System.Serializable]
        public class DreamChoice
        {
            public string label;
            public string nextNodeId;

            /// <summary>true 이면 플레이어 파라미터 조건을 검사합니다.</summary>
            public bool requiresCondition;

            /// <summary>Enlightenment 가 이 값 미만이면 선택지 비활성.</summary>
            public float requiredEnlightenment;
        }
    }
}
