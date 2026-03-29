using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class AssetManager : SingletonBase<AssetManager>
    {
        public void Initialize()
        {
            
        }

        public bool LoadAsset<T>(string path, out T result) where T : UnityEngine.Object
        {
            result = Resources.Load<T>(path);
            return result != null;
        }

        public bool GetItemIcon(string item_id, out Sprite result)
        {
            return LoadAsset($"UI/ItemIcon/itemicon_{item_id}", out result);
        }

        public bool GetItemPrefab(string item_id, out GameObject result)
        {
            return LoadAsset($"Item Base Prefabs/Drop Item [{item_id}]", out result);
        }

        public bool GetItemAmmoPrefab(string item_id, out GameObject result)
        {
            return LoadAsset($"Ammo Prefab/{item_id}", out result);
        }
    }
}
