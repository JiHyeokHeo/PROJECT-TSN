using System;
using System.IO;
using UnityEngine;

namespace TST
{
    // ----------------------------------------------------------------
    //  Flat save record for a single ObservationRecord — JSON-safe
    // ----------------------------------------------------------------
    [Serializable]
    public class ObservationRecordData
    {
        public string id;
        public string name;
        public string description;
        public int    recordType;   // RecordType enum -> int
        public int    rarity;       // Rarity enum -> int
    }

    // ----------------------------------------------------------------
    //  Per-slot save data
    // ----------------------------------------------------------------
    [Serializable]
    public class SaveData
    {
        public int    slotIndex;
        public int    currentDay;
        public string lastSavedAt;  // "yyyy-MM-dd HH:mm"

        // PlayerParameters
        public float  fame;
        public float  sanity;
        public float  enlightenment;
        public float  madness;
        public double funds;

        // TelescopeData — indexed by (int)TelescopePartType, length == TELESCOPE_PART_COUNT
        public int[] telescopeLevels;

        // ObservationJournal
        public ObservationRecordData[] journalRecords;
        public ObservationRecordData[] pendingRecords;

        // PhaseManager
        public int currentPhase;    // GamePhase enum -> int
    }

    // ----------------------------------------------------------------
    //  SaveSystem singleton
    // ----------------------------------------------------------------
    public class SaveSystem : SingletonBase<SaveSystem>
    {
        public const int SLOT_COUNT            = 3;
        private const int TELESCOPE_PART_COUNT = 7; // TelescopePartType enum member count

        /// <summary>이번 세션에서 마지막으로 저장/로드한 슬롯. -1 = 아직 없음.</summary>
        public int LastUsedSlot { get; private set; } = -1;

        // ----------------------------------------------------------------
        //  File path helpers
        // ----------------------------------------------------------------
        private static string SlotPath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
        }

        // ----------------------------------------------------------------
        //  Public API
        // ----------------------------------------------------------------

        /// <summary>
        /// Serializes current runtime state into slot <paramref name="slotIndex"/>.
        /// Returns true on success.
        /// </summary>
        public bool Save(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return false;

            try
            {
                SaveData data = BuildSaveData(slotIndex);
                string json   = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SlotPath(slotIndex), json);
                LastUsedSlot = slotIndex;
                Debug.LogFormat("[SaveSystem] Slot {0} saved (Day {1}).", slotIndex, data.currentDay);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[SaveSystem] Save failed for slot {0}: {1}", slotIndex, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deserializes slot <paramref name="slotIndex"/> and applies values to all runtime systems.
        /// Returns true on success.
        /// </summary>
        public bool Load(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return false;

            string path = SlotPath(slotIndex);
            if (!File.Exists(path))
            {
                Debug.LogWarningFormat("[SaveSystem] No save found at slot {0}.", slotIndex);
                return false;
            }

            try
            {
                string   json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(data);
                LastUsedSlot = slotIndex;
                Debug.LogFormat("[SaveSystem] Slot {0} loaded (Day {1}).", slotIndex, data.currentDay);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[SaveSystem] Load failed for slot {0}: {1}", slotIndex, ex.Message);
                return false;
            }
        }

        /// <summary>Returns true if a save file exists for the given slot.</summary>
        public bool HasSave(int slotIndex)
        {
            return IsValidSlot(slotIndex) && File.Exists(SlotPath(slotIndex));
        }

        /// <summary>Returns true if any slot has a save file.</summary>
        public bool HasAnySave()
        {
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                if (HasSave(i)) return true;
            }
            return false;
        }

        /// <summary>
        /// Reads save metadata from disk without applying it to runtime systems.
        /// Returns null if the slot is empty or the file is unreadable.
        /// </summary>
        public SaveData GetPreview(int slotIndex)
        {
            if (!HasSave(slotIndex)) return null;

            try
            {
                string json = File.ReadAllText(SlotPath(slotIndex));
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[SaveSystem] GetPreview failed for slot {0}: {1}", slotIndex, ex.Message);
                return null;
            }
        }

        /// <summary>Deletes the save file for the given slot. No-op if slot is empty.</summary>
        public void DeleteSave(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return;

            string path = SlotPath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.LogFormat("[SaveSystem] Slot {0} deleted.", slotIndex);
            }
        }

        // ----------------------------------------------------------------
        //  Internal — build SaveData from runtime systems
        // ----------------------------------------------------------------
        private static SaveData BuildSaveData(int slotIndex)
        {
            PlayerParameters  pp   = PlayerParameters.Singleton;
            TelescopeData     td   = TelescopeData.Singleton;
            ObservationJournal oj  = ObservationJournal.Singleton;
            PhaseManager      pm   = PhaseManager.Singleton;

            // Telescope: map enum index -> level array
            var parts = (TelescopePartType[])Enum.GetValues(typeof(TelescopePartType));
            int[] telescopeLevels = new int[TELESCOPE_PART_COUNT];
            for (int i = 0; i < parts.Length && i < TELESCOPE_PART_COUNT; i++)
            {
                telescopeLevels[(int)parts[i]] = td.GetLevel(parts[i]);
            }

            // Journal records
            var allRecords     = oj.GetAllRecords();
            var pendingRecords = oj.GetPendingRecords();

            return new SaveData
            {
                slotIndex         = slotIndex,
                currentDay        = pm.CurrentDay,
                lastSavedAt       = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),

                fame              = pp.Fame,
                sanity            = pp.Sanity,
                enlightenment     = pp.Enlightenment,
                madness           = pp.Madness,
                funds             = pp.Funds,

                telescopeLevels   = telescopeLevels,

                journalRecords    = ConvertRecords(allRecords),
                pendingRecords    = ConvertRecords(pendingRecords),

                currentPhase      = (int)pm.CurrentPhase
            };
        }

        // ----------------------------------------------------------------
        //  Internal — apply SaveData to runtime systems
        // ----------------------------------------------------------------
        private static void ApplySaveData(SaveData data)
        {
            if (data == null) return;

            // PlayerParameters
            PlayerParameters pp = PlayerParameters.Singleton;
            pp.ApplySaveData(data.fame, data.sanity, data.enlightenment, data.madness, data.funds);

            // TelescopeData
            if (data.telescopeLevels != null)
            {
                var parts = (TelescopePartType[])Enum.GetValues(typeof(TelescopePartType));
                var tdSave = new TelescopeData.TelescopeSaveData
                {
                    partTypes  = new int[parts.Length],
                    partLevels = new int[parts.Length]
                };
                for (int i = 0; i < parts.Length && i < TELESCOPE_PART_COUNT; i++)
                {
                    int idx = (int)parts[i];
                    tdSave.partTypes[i]  = idx;
                    tdSave.partLevels[i] = idx < data.telescopeLevels.Length ? data.telescopeLevels[idx] : 1;
                }
                TelescopeData.Singleton.FromSaveData(tdSave);
            }

            // ObservationJournal — rebuild via JournalSaveData
            ObservationJournal oj = ObservationJournal.Singleton;
            var journalSave = new ObservationJournal.JournalSaveData
            {
                allRecords     = ConvertRecordData(data.journalRecords),
                pendingRecords = ConvertRecordData(data.pendingRecords)
            };
            oj.FromSaveData(journalSave);

            // PhaseManager — 날짜와 페이즈 복원 (이벤트 미발동)
            PhaseManager.Singleton.ForceSetDay(data.currentDay);
            PhaseManager.Singleton.ForceSetPhase((GamePhase)data.currentPhase);
        }

        // ----------------------------------------------------------------
        //  Conversion helpers
        // ----------------------------------------------------------------
        private static ObservationRecordData[] ConvertRecords(System.Collections.Generic.List<ObservationRecord> source)
        {
            if (source == null) return Array.Empty<ObservationRecordData>();

            var result = new ObservationRecordData[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var r = source[i];
                result[i] = new ObservationRecordData
                {
                    id          = r.id,
                    name        = r.name,
                    description = r.description,
                    recordType  = (int)r.type,
                    rarity      = (int)r.rarity
                };
            }
            return result;
        }

        private static System.Collections.Generic.List<ObservationRecord> ConvertRecordData(ObservationRecordData[] source)
        {
            var result = new System.Collections.Generic.List<ObservationRecord>();
            if (source == null) return result;

            foreach (var d in source)
            {
                result.Add(new ObservationRecord(
                    d.id,
                    d.name,
                    (RecordType)d.recordType,
                    (Rarity)d.rarity,
                    d.description
                ));
            }
            return result;
        }

        // ----------------------------------------------------------------
        //  Validation
        // ----------------------------------------------------------------
        private static bool IsValidSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < SLOT_COUNT) return true;
            Debug.LogErrorFormat("[SaveSystem] Invalid slot index: {0}. Valid range is 0-{1}.", slotIndex, SLOT_COUNT - 1);
            return false;
        }
    }
}
