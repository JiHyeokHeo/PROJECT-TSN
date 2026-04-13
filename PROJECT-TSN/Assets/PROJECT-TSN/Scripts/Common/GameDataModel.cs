using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// 아이템 정의 SO의 런타임 캐시 및 조회 서비스.
    /// Inspector에서 ItemDatas 리스트에 ItemDefinitionSO 에셋을 할당하십시오.
    /// Initialize()는 Awake 또는 최초 GetItemData() 호출 시 자동으로 실행됩니다.
    ///
    /// Inspector 연결 (GameDataModel GameObject):
    ///   - ItemDatas: Assets/PROJECT-TSN/Data/Items/ 아래 생성한 ItemDefinitionSO 에셋 목록
    /// </summary>
    public class GameDataModel : SingletonBase<GameDataModel>
    {
        [field: SerializeField]
        public List<ItemDefinitionSO> ItemDatas { get; private set; } = new List<ItemDefinitionSO>();

        private Dictionary<string, ItemDefinitionSO> _itemById;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
            Debug.Log($"[GameDataModel] Initialized — {_itemById.Count}개 아이템 캐싱 완료. (InstanceID: {gameObject.GetInstanceID()})");
        }

        /// <summary>
        /// ItemDatas 리스트를 Dictionary로 캐싱합니다.
        /// itemId가 비어 있거나 null인 항목은 무시됩니다.
        /// </summary>
        public void Initialize()
        {
            _itemById = new Dictionary<string, ItemDefinitionSO>();
            foreach (var item in ItemDatas)
            {
                if (item == null || string.IsNullOrEmpty(item.itemId))
                    continue;

                if (_itemById.ContainsKey(item.itemId))
                {
                    Debug.LogWarningFormat("[GameDataModel] 중복된 itemId '{0}' 감지 — 후순위 항목이 무시됩니다.", item.itemId);
                    continue;
                }

                _itemById[item.itemId] = item;
            }
        }

        /// <summary>
        /// itemId로 ItemDefinitionSO를 조회합니다.
        /// </summary>
        /// <param name="itemId">ObservationRecord.id와 동일한 값</param>
        /// <param name="resultData">조회 성공 시 해당 SO, 실패 시 null</param>
        /// <returns>조회 성공 여부</returns>
        public bool GetItemData(string itemId, out ItemDefinitionSO resultData)
        {
            if (_itemById == null || _itemById.Count == 0)
                Initialize();

            return _itemById.TryGetValue(itemId, out resultData);
        }
    }
}
