using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 꿈 이벤트 선택지 팝업 (최대 4종).
    /// UIList: Popup_Choice
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_Choice
    ///
    /// 흐름:
    ///   Show(choices, onChoiceSelected)
    ///     → choiceContainer 에 choiceBtnPrefab 동적 생성 (최대 4개)
    ///     → requiresCondition && Enlightenment < requiredEnlightenment → interactable = false
    ///     → 활성 버튼 클릭 → onChoiceSelected(index) → Hide()
    ///
    /// Inspector 와이어링:
    ///   choiceContainer  — 버튼을 붙일 부모 Transform
    ///   choiceBtnPrefab  — TextMeshProUGUI + Button 이 붙은 버튼 프리팹
    /// </summary>
    public class ChoicePopupUI : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private Transform  choiceContainer;
        [SerializeField] private GameObject choiceBtnPrefab;

        private const int MaxChoices = 4;

        // ── 런타임 상태 ──────────────────────────────────────────────
        private Action<int> _onChoiceSelected;

        // ── 공개 API ─────────────────────────────────────────────────
        /// <summary>선택지 팝업을 표시합니다.</summary>
        public void Show(DreamEventData.DreamChoice[] choices, Action<int> onChoiceSelected)
        {
            _onChoiceSelected = onChoiceSelected;

            ClearContainer();

            if (choices == null || choices.Length == 0)
            {
                Debug.LogWarning("[ChoicePopupUI] 선택지가 없습니다.");
                return;
            }

            int count = Mathf.Min(choices.Length, MaxChoices);
            for (int i = 0; i < count; i++)
            {
                SpawnChoice(i, choices[i]);
            }

            base.Show();
        }

        // ── 내부 ─────────────────────────────────────────────────────
        private void SpawnChoice(int index, DreamEventData.DreamChoice choice)
        {
            if (choiceBtnPrefab == null || choiceContainer == null) return;

            GameObject go = Instantiate(choiceBtnPrefab, choiceContainer);

            // 레이블 설정
            TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = choice.label ?? string.Empty;

            // 조건 검사
            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                bool conditionFailed = choice.requiresCondition
                    && PlayerParameters.Singleton.Enlightenment < choice.requiredEnlightenment;

                btn.interactable = !conditionFailed;

                int capturedIndex = index;
                btn.onClick.AddListener(() => OnChoiceClicked(capturedIndex));
            }
        }

        private void OnChoiceClicked(int index)
        {
            Hide();

            Action<int> callback = _onChoiceSelected;
            _onChoiceSelected = null;
            callback?.Invoke(index);
        }

        private void ClearContainer()
        {
            if (choiceContainer == null) return;

            for (int i = choiceContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(choiceContainer.GetChild(i).gameObject);
            }
        }
    }
}
