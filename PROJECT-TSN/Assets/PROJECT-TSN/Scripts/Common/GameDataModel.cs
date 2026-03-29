using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class GameDataModel : SingletonBase<GameDataModel>
    {
        [field: SerializeField] public List<ItemDefinitionSO> ItemDatas { get; private set; } = new List<ItemDefinitionSO>();

        Dictionary<string, ItemDefinitionSO> itemById;

        protected override void Awake()
        {
            base.Awake();

            Debug.Log($"{gameObject.GetInstanceID()}");
        }

        public void Initialize()
        {
            itemById = new Dictionary<string, ItemDefinitionSO>();
            foreach (var it in ItemDatas)
            {
                if (it == null || string.IsNullOrEmpty(it.itemId))
                    continue;

                itemById[it.itemId] = it;
            }
        }

        public bool GetItemData(string itemId, out ItemDefinitionSO resultData)
        {
            if (itemById == null || itemById.Count == 0)
                Initialize();

            return itemById.TryGetValue(itemId, out resultData);
        }
    }
}
