//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TST
//{
//    public class CraftingManager : MonoBehaviour
//    {
//        public static CraftingManager Instance { get; private set; }

//        private void Awake()
//        {
//            Instance = this;
//        }

//        [Sirenix.OdinInspector.Button]
//        public void Crafting(string crafting_id)
//        {
//            if (!GameDataModel.Singleton.GetCraftingData(crafting_id, out CraftDataSO craftingData))
//                return;

//            if (GameDataModel.Singleton.GetItemData(craftingData.ResultItemID, out ItemData createdItemData))
//            {
//                bool isCraftingSuccess = false;

//                float rand = Random.Range(0.001f, 1f);
//                //float successRate = createdItemData.

//                for (int i = 0; i < craftingData.ResultAmount; i++)
//                {
//                    UserDataModel.Singleton.AddItemToInventory(createdItemData);
//                }
//            }
//        }
//    }
//}
