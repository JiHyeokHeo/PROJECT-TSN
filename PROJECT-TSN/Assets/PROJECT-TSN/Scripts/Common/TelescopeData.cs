using System;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public enum TelescopePartType
    {
        Lens,
        Filter,
        Handle,
        OpticalAdjuster,
        FocusTracker,
        SignalAmplifier,
        RecordingDevice
    }

    public class TelescopeData : SingletonBase<TelescopeData>
    {
        // ----------------------------------------------------------------
        //  Constants
        // ----------------------------------------------------------------
        private const int MinLevel = 1;
        private const int MaxLevel = 5;

        // ----------------------------------------------------------------
        //  Save data
        // ----------------------------------------------------------------
        [Serializable]
        public class TelescopeSaveData
        {
            // Parallel arrays to avoid Dictionary serialization issues with JsonUtility
            public int[] partTypes;
            public int[] partLevels;
        }

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        [SerializeField]
        private SerializableWrapDictionary<TelescopePartType, int> _levels = new SerializableWrapDictionary<TelescopePartType, int>();

        // ----------------------------------------------------------------
        //  Initialization
        // ----------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            foreach (TelescopePartType part in Enum.GetValues(typeof(TelescopePartType)))
            {
                if (!_levels.ContainsKey(part))
                    _levels[part] = MinLevel;
            }
        }

        // ----------------------------------------------------------------
        //  Queries
        // ----------------------------------------------------------------
        public int GetLevel(TelescopePartType part)
        {
            if (_levels.TryGetValue(part, out int level)) return level;
            return MinLevel;
        }

        /// <summary>
        /// Number of observable zones, driven by Lens level.
        /// Lens lv1 = 1 zone, each subsequent level adds 1 zone.
        /// </summary>
        public int GetObservableZoneCount()
        {
            return GetLevel(TelescopePartType.Lens);
        }

        /// <summary>
        /// Returns an additive rare-discovery probability bonus (0~1 range) for the given record type.
        /// CelestialBody: boosted by Filter
        /// Phenomenon:    boosted by SignalAmplifier + FocusTracker
        /// CosmicTrace:   boosted by RecordingDevice + OpticalAdjuster
        /// </summary>
        public float GetRareBonus(RecordType type)
        {
            const float PerLevel = 0.04f; // 4% per level above 1

            switch (type)
            {
                case RecordType.CelestialBody:
                    return (GetLevel(TelescopePartType.Filter) - 1) * PerLevel;

                case RecordType.Phenomenon:
                    return ((GetLevel(TelescopePartType.SignalAmplifier) - 1)
                          + (GetLevel(TelescopePartType.FocusTracker)    - 1)) * PerLevel;

                case RecordType.CosmicTrace:
                    return ((GetLevel(TelescopePartType.RecordingDevice)  - 1)
                          + (GetLevel(TelescopePartType.OpticalAdjuster)  - 1)) * PerLevel;

                default:
                    return 0f;
            }
        }

        // ----------------------------------------------------------------
        //  Upgrade
        // ----------------------------------------------------------------
        /// <summary>
        /// Attempts to upgrade the given part. Deducts cost from PlayerParameters.Funds.
        /// Returns false if already at max level or insufficient funds.
        /// </summary>
        public bool TryUpgrade(TelescopePartType part, double cost)
        {
            int current = GetLevel(part);
            if (current >= MaxLevel)
            {
                Debug.LogFormat("[TelescopeData] {0} is already at max level {1}.", part, MaxLevel);
                return false;
            }

            PlayerParameters p = PlayerParameters.Singleton;
            if (p.Funds < cost)
            {
                Debug.LogFormat("[TelescopeData] Insufficient funds. Required: {0}, Available: {1}", cost, p.Funds);
                return false;
            }

            p.AddFunds(-cost);
            _levels[part] = current + 1;
            Debug.LogFormat("[TelescopeData] {0} upgraded to level {1}.", part, _levels[part]);
            return true;
        }

        // ----------------------------------------------------------------
        //  Persistence
        // ----------------------------------------------------------------
        public TelescopeSaveData ToSaveData()
        {
            var partTypes  = new List<int>();
            var partLevels = new List<int>();

            foreach (var kv in _levels)
            {
                partTypes.Add((int)kv.Key);
                partLevels.Add(kv.Value);
            }

            return new TelescopeSaveData
            {
                partTypes  = partTypes.ToArray(),
                partLevels = partLevels.ToArray()
            };
        }

        public void FromSaveData(TelescopeSaveData data)
        {
            if (data == null || data.partTypes == null) return;

            InitializeDefaults();

            int count = Mathf.Min(data.partTypes.Length, data.partLevels.Length);
            for (int i = 0; i < count; i++)
            {
                TelescopePartType part = (TelescopePartType)data.partTypes[i];
                _levels[part] = Mathf.Clamp(data.partLevels[i], MinLevel, MaxLevel);
            }
        }

        public void Save()
        {
            // TODO: FileManager.WriteFileFromString("save/telescope.json", JsonUtility.ToJson(ToSaveData()));
        }

        public void Load()
        {
            // TODO:
            // if (FileManager.ReadFileData("save/telescope.json", out string json))
            //     FromSaveData(JsonUtility.FromJson<TelescopeSaveData>(json));
        }
    }
}
