using UnityEngine;

namespace TST
{
    /// <summary>
    /// 아이템(관측 기록)의 정적 메타데이터를 담는 ScriptableObject.
    /// 런타임 저장에는 사용되지 않습니다. JSON 저장에는 itemId(string)만 저장하고,
    /// 표시 시점에 GameDataModel.GetItemData()로 이 SO를 조회하십시오.
    ///
    /// Create: 우클릭 → Create → TST → Item Definition
    /// 권장 에셋 경로: Assets/PROJECT-TSN/Data/Items/
    /// </summary>
    [CreateAssetMenu(fileName = "Item_New", menuName = "TST/Item Definition", order = 10)]
    public class ItemDefinitionSO : ScriptableObject
    {
        [Tooltip("ObservationRecord.id와 일치해야 합니다.")]
        public string itemId;

        [Tooltip("UI에 표시할 아이템 이름")]
        public string itemName;

        [Tooltip("아이템 종류")]
        public RecordType recordType;

        [Tooltip("아이템 등급")]
        public Rarity rarity;

        [TextArea(2, 5)]
        [Tooltip("아이템 설명 텍스트")]
        public string description;

        [Tooltip("인벤토리 상세 패널에 표시할 아이콘 스프라이트 (없으면 rarity 색상 fallback)")]
        public Sprite icon;
    }
}
