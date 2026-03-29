using System;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class ResourceManager : SingletonBase<ResourceManager>
    {
        // 재화 변경 이벤트: (타입, 새 총량)
        public event Action<ResourceType, double> OnResourceChanged;
        // 재화 획득 이벤트: (타입, 추가된 양) - 팝업용
        public event Action<ResourceType, double> OnResourceAdded;

        readonly Dictionary<ResourceType, double> resources = new Dictionary<ResourceType, double>
        {
            { ResourceType.Gold,  100 },
            { ResourceType.Mana,  0   },
            { ResourceType.Wood,  0   },
            { ResourceType.Stone, 0   },
            { ResourceType.Food,  0   },
        };

        public void Initialize(Dictionary<ResourceType, double> savedResources)
        {
            if (savedResources == null) return;
            foreach (var kv in savedResources)
            {
                resources[kv.Key] = kv.Value;
            }
        }

        public double GetResource(ResourceType type)
        {
            return resources.TryGetValue(type, out double val) ? val : 0;
        }

        public void AddResource(ResourceType type, double amount)
        {
            if (amount <= 0) return;
            if (!resources.ContainsKey(type)) resources[type] = 0;
            resources[type] += amount;
            OnResourceChanged?.Invoke(type, resources[type]);
            OnResourceAdded?.Invoke(type, amount);
        }

        public bool SpendResource(ResourceType type, double amount)
        {
            if (amount <= 0) return true;
            if (GetResource(type) < amount) return false;
            resources[type] -= amount;
            OnResourceChanged?.Invoke(type, resources[type]);
            return true;
        }

        public bool HasEnough(ResourceType type, double amount) => GetResource(type) >= amount;

        public Dictionary<ResourceType, double> GetAllResources()
        {
            return new Dictionary<ResourceType, double>(resources);
        }

        public static string FormatAmount(double amount)
        {
            if (amount >= 1_000_000)
                return $"{amount / 1_000_000:F1}M";
            if (amount >= 1_000)
                return $"{amount / 1_000:F1}k";
            return $"{Mathf.FloorToInt((float)amount)}";
        }
    }
}
