using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 기록 처분 목록의 단일 행 컴포넌트.
    /// AcademyController 가 생성하며, 기록 정보를 표시하고 선택 콜백을 전달합니다.
    ///
    /// Inspector wiring:
    ///   - nameLabel    : 기록 이름 텍스트
    ///   - rarityLabel  : 희귀도 텍스트 (선택적)
    ///   - selectButton : 행 선택 버튼 (없으면 Button 컴포넌트 자동 사용)
    /// </summary>
    public class RecordListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI rarityLabel;
        [SerializeField] private Button selectButton;

        private string _recordId;
        private Action<string> _onSelected;

        private void Awake()
        {
            if (selectButton == null)
                selectButton = GetComponent<Button>();

            if (selectButton != null)
                selectButton.onClick.AddListener(OnClicked);
        }

        public void Setup(ObservationRecord record, Action<string> onSelected)
        {
            _recordId   = record.id;
            _onSelected = onSelected;

            if (nameLabel != null)
                nameLabel.text = record.name;

            if (rarityLabel != null)
                rarityLabel.text = record.rarity.ToString();
        }

        private void OnClicked()
        {
            _onSelected?.Invoke(_recordId);
        }
    }
}
