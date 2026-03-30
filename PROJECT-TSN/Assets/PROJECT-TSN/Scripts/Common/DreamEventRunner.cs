using UnityEngine;

namespace TST
{
    /// <summary>
    /// 꿈 이벤트 FSM 실행기.
    ///
    /// 진입점:
    ///   DreamKeySelectionUI → DreamEventRunner.Singleton.StartEvent(record)
    ///
    /// 노드 처리:
    ///   Dialogue → DreamVNPanel.PlayDialogue
    ///   Choice   → ChoicePopupUI.Show
    ///   Dice     → DiceRollPopupUI.Show
    ///   End / 빈 nextNodeId → OnEventComplete
    ///
    /// 완료 처리:
    ///   checkEndingOnComplete && Enlightenment >= 80 → GameFlowDirector.PlayEnding()
    ///   그 외 → UIManager.Show(Panel_SaveScreen)
    ///
    /// Inspector 와이어링:
    ///   availableEvents — DreamEventData SO 배열 (없으면 Resources.LoadAll 로 자동 로드)
    /// </summary>
    public class DreamEventRunner : SingletonBase<DreamEventRunner>
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private DreamEventData[] availableEvents;

        private const string ResourcesPath    = "Dream/Events";
        private const float  EndingThreshold  = 80f;

        // ── 런타임 상태 ──────────────────────────────────────────────
        private DreamEventData _currentEvent;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>레코드 타입에 맞는 이벤트를 찾아 첫 번째 노드부터 실행합니다.</summary>
        public void StartEvent(ObservationRecord triggerRecord)
        {
            if (triggerRecord == null)
            {
                Debug.LogWarning("[DreamEventRunner] StartEvent: triggerRecord 가 null 입니다.");
                OnEventComplete(null);
                return;
            }

            DreamEventData data = FindEventForRecord(triggerRecord);

            if (data == null)
            {
                Debug.LogWarningFormat(
                    "[DreamEventRunner] '{0}' 타입에 맞는 DreamEventData 를 찾지 못했습니다. 이벤트 없이 종료합니다.",
                    triggerRecord.type);
                OnEventComplete(null);
                return;
            }

            _currentEvent = data;

            if (_currentEvent.nodes == null || _currentEvent.nodes.Length == 0)
            {
                Debug.LogWarningFormat("[DreamEventRunner] '{0}' 의 nodes 배열이 비어 있습니다.", data.eventId);
                OnEventComplete(data);
                return;
            }

            RunNode(_currentEvent.nodes[0].nodeId);
        }

        // ── 노드 FSM ─────────────────────────────────────────────────
        private void RunNode(string nodeId)
        {
            if (_currentEvent == null) return;

            // 빈 nodeId = 이벤트 종료
            if (string.IsNullOrEmpty(nodeId))
            {
                OnEventComplete(_currentEvent);
                return;
            }

            DreamEventData.DreamNode node = FindNode(nodeId);
            if (node == null)
            {
                Debug.LogWarningFormat("[DreamEventRunner] 노드 '{0}' 를 찾을 수 없습니다. 이벤트 종료.", nodeId);
                OnEventComplete(_currentEvent);
                return;
            }

            switch (node.nodeType)
            {
                case DreamEventData.DreamNodeType.Dialogue:
                    ExecuteDialogueNode(node);
                    break;

                case DreamEventData.DreamNodeType.Choice:
                    ExecuteChoiceNode(node);
                    break;

                case DreamEventData.DreamNodeType.Dice:
                    ExecuteDiceNode(node);
                    break;

                case DreamEventData.DreamNodeType.End:
                default:
                    OnEventComplete(_currentEvent);
                    break;
            }
        }

        private void ExecuteDialogueNode(DreamEventData.DreamNode node)
        {
            var vnPanel = UIManager.Singleton.GetUI<DreamVNPanel>(UIList.Panel_DreamVN);
            if (vnPanel == null)
            {
                Debug.LogWarning("[DreamEventRunner] DreamVNPanel 을 찾을 수 없습니다.");
                RunNode(node.nextNodeId);
                return;
            }

            vnPanel.PlayDialogue(node.dialogue, () => RunNode(node.nextNodeId));
        }

        private void ExecuteChoiceNode(DreamEventData.DreamNode node)
        {
            var popup = UIManager.Show<ChoicePopupUI>(UIList.Popup_Choice);
            if (popup == null)
            {
                Debug.LogWarning("[DreamEventRunner] ChoicePopupUI 를 찾을 수 없습니다.");
                OnEventComplete(_currentEvent);
                return;
            }

            popup.Show(node.choices, idx =>
            {
                if (node.choices != null && idx >= 0 && idx < node.choices.Length)
                    RunNode(node.choices[idx].nextNodeId);
                else
                    OnEventComplete(_currentEvent);
            });
        }

        private void ExecuteDiceNode(DreamEventData.DreamNode node)
        {
            var popup = UIManager.Show<DiceRollPopupUI>(UIList.Popup_DiceRoll);
            if (popup == null)
            {
                Debug.LogWarning("[DreamEventRunner] DiceRollPopupUI 를 찾을 수 없습니다.");
                OnEventComplete(_currentEvent);
                return;
            }

            int diceMax   = node.diceMax > 0 ? node.diceMax : 6;
            popup.Show(diceMax, node.rewindLimit, result =>
            {
                string nextId = result >= node.diceSuccessThreshold
                    ? node.successNodeId
                    : node.failNodeId;
                RunNode(nextId);
            });
        }

        // ── 이벤트 완료 ───────────────────────────────────────────────
        private void OnEventComplete(DreamEventData completedEvent)
        {
            _currentEvent = null;

            // 엔딩 체크
            if (completedEvent != null
                && completedEvent.checkEndingOnComplete
                && PlayerParameters.Singleton.Enlightenment >= EndingThreshold)
            {
                GameFlowDirector.Singleton.PlayEnding();
                return;
            }

            UIManager.Show<UIBase>(UIList.Panel_SaveScreen);
        }

        // ── 헬퍼 ─────────────────────────────────────────────────────
        private DreamEventData.DreamNode FindNode(string nodeId)
        {
            if (_currentEvent?.nodes == null) return null;

            foreach (var node in _currentEvent.nodes)
            {
                if (node.nodeId == nodeId) return node;
            }
            return null;
        }

        private DreamEventData FindEventForRecord(ObservationRecord record)
        {
            DreamEventData[] pool = GetAvailableEvents();
            if (pool == null) return null;

            foreach (DreamEventData data in pool)
            {
                if (data != null && data.triggerRecordType == record.type)
                    return data;
            }
            return null;
        }

        private DreamEventData[] GetAvailableEvents()
        {
            if (availableEvents != null && availableEvents.Length > 0)
                return availableEvents;

            // Inspector 에서 연결되지 않았으면 Resources 에서 자동 로드
            DreamEventData[] loaded = Resources.LoadAll<DreamEventData>(ResourcesPath);
            if (loaded != null && loaded.Length > 0)
                return loaded;

            return null;
        }
    }
}
