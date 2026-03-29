//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Assertions;
//using UnityEngine.EventSystems;
//using UnityEngine.TextCore.Text;

//namespace TST
//{
//    public partial class GameManager : MonoBehaviour
//    {
//        public event System.Action<ItemData, int> OnUsedItem;
//        public event System.Action <GameObject, Vector3>OnItemGenerateEvent;

//        private Dictionary<ItemList, int> itemWeights = new Dictionary<ItemList, int>()
//    {
//        { ItemList.APCBullet, 5 },
//        { ItemList.Alchemical, 10 },
//        { ItemList.Fluid, 10 },
//        { ItemList.Money, 30 },
//        { ItemList.GunPowder, 10 },
//        { ItemList.Healing_Kit, 15 },
//        { ItemList.Incendiary_Bullet, 5 },
//        { ItemList.Normal_AR_Bullet, 10 },
//        { ItemList.Normal_Pistol_Bullet, 5 }
//    };

//        // 아이템 번호로 하면 좋더 좋았을듯
//        public void BuyItem(ItemData itemData)
//        {
//            // 골드 유무를 확인해야함 
            

//            UserDataModel.Singleton.AddItemToInventory(itemData);
//        }

//        public bool CraftItem(string craft_id)
//        {
//            return CraftingItems(craft_id);
//        }

//        public void GenerateItem(Vector3 position)
//        {
//            // 미만         이하
//            // int 파라미터 float 파라미터 Range Inclusive Exclusive 
//            int randItemID = UnityEngine.Random.Range((int)ItemList.ITEMLIST_START + 1, (int)ItemList.ITEMLIST_END);

//            var itemEnum = (ItemList)randItemID;
//            string itemName = itemEnum.ToString();

//            AssetManager.Singleton.GetItemPrefab(itemName, out GameObject result);

//            if (result)
//            {
//                GameObject gameObject = Instantiate(result, position, Quaternion.identity);
//                OnItemGenerateEvent?.Invoke(gameObject, position);
//            }
//        }

//        public void GenerateItem(Vector3 position, ItemList type)
//        {
//            // 미만         이하
//            // int 파라미터 float 파라미터 Range Inclusive Exclusive 
//            int randItemID = (int)type;
//            if (randItemID <= (int)ItemList.ITEMLIST_START || randItemID >= (int)ItemList.ITEMLIST_END)
//                return;

//            var itemEnum = (ItemList)randItemID;
//            string itemName = itemEnum.ToString();

//            AssetManager.Singleton.GetItemPrefab(itemName, out GameObject result);

//            if (result)
//            {
//                GameObject gameObject = Instantiate(result, position, Quaternion.identity);
//                OnItemGenerateEvent?.Invoke(gameObject, position);
//            }
//        }

//        private ItemList GetWeightedRandomItem()
//        {
//            int totalWeight = 0;
//            foreach (var weight in itemWeights.Values)
//                totalWeight += weight;

//            int rand = UnityEngine.Random.Range(0, totalWeight);

//            // 누적 수치
//            int cumulative = 0;

//            foreach (var kvp in itemWeights)
//            {
//                cumulative += kvp.Value;
//                if (rand < cumulative)
//                    return kvp.Key;
//            }

//            return ItemList.Money;
//        }

//        public bool UseItem(int slotId, ItemData itemData, int count = 1)
//        {
//            bool result = true;
//            switch (itemData.ItemCategory)
//            {
//                case ItemCategory.Equipment:
//                    EquipmentItem(slotId, itemData);
//                    break;
//                case ItemCategory.Material:
//                    result = UserDataModel.Singleton.UseInventoryItem(itemData, count);
//                    break;
//                case ItemCategory.Consumable:
//                     UseConsumable(slotId, itemData, count);
//                    result = UserDataModel.Singleton.UseInventoryItem(itemData, count);
//                    break;
//            }

//            if (result == false)
//                return result;

//            // 마지막에 사용한 아이템을 UserDataModel에서 삭제하도록 처리를 불러주자
//            OnUsedItem?.Invoke(itemData, count);

//            return result;
//        }

//        public void UnEquipmentItem(ItemEquipmentCategory category, int slotId)
//        {
//            UserDataModel.Singleton.UnEquipItem(category, slotId);
//        }

//        private void EquipmentItem(int slotId, ItemData itemData)
//        {
//            // 실제 데이터 변경은 이쪽에서
//            var category = (ItemEquipmentCategory)itemData.ItemSubCategory;
//            UserDataModel.Singleton.EquipItem(category, slotId);
//        }

//        private void UseConsumable(int slotId, ItemData itemData, int count = 1)
//        {
//            switch (itemData.ItemSubCategory)
//            {
//                case (int)ItemConsumableCategory.HealingKit:
//                    {
//                        ItemStatBase stat = itemData.ItemStat;
//                        if (stat == null)
//                        {
//                            Assert.IsNull(stat, $"{stat} is Not Valid");
//                            return;
//                        }

//                        Debug.Log("HealingKit Used");
//                        if (stat is ConsumableStat consumableStat)
//                        {
//                            CharacterController.Instance.linkedCharacter.CurrentHp += consumableStat.buffValue;
//                        }
//                        break;
//                    }
//                case (int)ItemConsumableCategory.Ammo:
//                    {
//                        ItemStatBase stat = itemData.ItemStatSubAdded;

//                        if (stat == null)
//                        {
//                            Assert.IsNull(stat, $"{stat} is Not Valid");
//                            return;
//                        }

//                        if (stat is AmmoStat consumableStat)
//                        {
//                            CharacterBase user = CharacterController.Instance.linkedCharacter;
//                            int existRifleIndex = user.rifleAmmos.FindLastIndex(x => x.data.AmmoType.Equals(consumableStat.AmmoType));
//                            int existPistolIndex = user.pistolAmmos.FindLastIndex(x => x.data.AmmoType.Equals(consumableStat.AmmoType));
//                            if (existRifleIndex >= 0)
//                            {
//                                user.rifleAmmos[existRifleIndex].CurrentBulletAmount += consumableStat.bulletAmount;
//                                user.primaryWeapon.AddMaxAmountBullet(consumableStat.bulletAmount);
//                            }

//                            if (existPistolIndex >= 0)
//                            {
//                                user.pistolAmmos[existPistolIndex].CurrentBulletAmount += consumableStat.bulletAmount;
//                                user.subWeapon.AddMaxAmountBullet(consumableStat.bulletAmount);
//                            }

//                            UIManager.Singleton.GetUI<MainHudUI>(UIList.MainHudUI).SetBulletTextImage();
//                        }
//                        break;
//                    }
//                case (int)ItemConsumableCategory.Key:
//                    {
                        
//                        break;
//                    }
//            }
//        }
        
//        private bool CraftingItems(string crafting_Id)
//        {
//            if (!GameDataModel.Singleton.GetCraftingData(crafting_Id, out CraftDataSO craftingData))
//                return false;

//            // 먼저 내가 소유하고 있는 아이템의 갯수가 충분 한지 확인
//            if (craftingData.RequireItems.Count > 0)
//            {
//                for (int i = 0; i < craftingData.RequireItems.Count; i++)
//                {
//                    CraftingDataBase requireItemData = craftingData.RequireItems[i];

//                    GameDataModel.Singleton.GetItemData(requireItemData.ItemID, out ItemData usingItemData);
//                    var data = UserDataModel.Singleton.UserItemData.GetUserItemData(requireItemData.ItemID);

//                    if (data == null)
//                        return false;

//                    if (data.itemCount < requireItemData.RequireAmount)
//                        return false;

//                    UseItem(-1, usingItemData, requireItemData.RequireAmount);
//                }
//            }

//            if (GameDataModel.Singleton.GetItemData(craftingData.ResultItemID, out ItemData createdItemData))
//            {
//                bool isCraftingSuccess = false;

//                float rand = UnityEngine.Random.Range(0.001f, 1f);
//                //float successRate = createdItemData.

//                //for (int i = 0; i < craftingData.ResultAmount; i++)
//                //{
//                    UserDataModel.Singleton.AddItemToInventory(createdItemData, craftingData.ResultAmount);
//                //}
//            }

//            return true;
//        }
//    }
//}
