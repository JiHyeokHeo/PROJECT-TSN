using System;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    // ----------------------------------------------------------------
    //  Enums
    // ----------------------------------------------------------------
    public enum RecordType
    {
        CelestialBody,
        Phenomenon,
        CosmicTrace
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    public enum DisposalMethod
    {
        Announce,  // Fame+, Funds+
        Destroy,   // Sanity+
        Explore,   // Enlightenment+
        Madden     // Madness+
    }

    // ----------------------------------------------------------------
    //  Disposal result
    // ----------------------------------------------------------------
    [Serializable]
    public class DisposeResult
    {
        public string recordId;
        public DisposalMethod method;
        public float fameChange;
        public float sanityChange;
        public float enlightenmentChange;
        public float madnessChange;
        public double fundsChange;
    }

    // ----------------------------------------------------------------
    //  Single observation record
    // ----------------------------------------------------------------
    [Serializable]
    public class ObservationRecord
    {
        public string id;
        public string name;
        public RecordType type;
        public Rarity rarity;
        public string description;
        public bool isDiscoveredThisNight;

        public ObservationRecord() { }

        public ObservationRecord(string id, string name, RecordType type, Rarity rarity, string description)
        {
            this.id                  = id;
            this.name                = name;
            this.type                = type;
            this.rarity              = rarity;
            this.description         = description;
            this.isDiscoveredThisNight = false;
        }
    }

    // ----------------------------------------------------------------
    //  Rarity multiplier helper
    // ----------------------------------------------------------------
    internal static class RarityMultiplier
    {
        internal static float Get(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:    return 1.0f;
                case Rarity.Uncommon:  return 1.5f;
                case Rarity.Rare:      return 2.5f;
                case Rarity.Legendary: return 5.0f;
                default:               return 1.0f;
            }
        }
    }

    // ----------------------------------------------------------------
    //  Disposal parameter delta table
    // ----------------------------------------------------------------
    internal static class DisposalTable
    {
        private const float BaseValue = 10f;

        internal static DisposeResult Calculate(ObservationRecord record, DisposalMethod method)
        {
            float mult = RarityMultiplier.Get(record.rarity);
            float delta = BaseValue * mult;

            var result = new DisposeResult
            {
                recordId = record.id,
                method   = method
            };

            switch (method)
            {
                case DisposalMethod.Announce:
                    result.fameChange   = delta;
                    result.fundsChange  = delta * 100.0;
                    break;
                case DisposalMethod.Destroy:
                    result.sanityChange = delta;
                    break;
                case DisposalMethod.Explore:
                    result.enlightenmentChange = delta;
                    break;
                case DisposalMethod.Madden:
                    result.madnessChange = delta;
                    break;
            }

            return result;
        }
    }

    // ----------------------------------------------------------------
    //  Journal singleton
    // ----------------------------------------------------------------
    public class ObservationJournal : SingletonBase<ObservationJournal>
    {
        // ----------------------------------------------------------------
        //  Save data
        // ----------------------------------------------------------------
        [Serializable]
        public class JournalSaveData
        {
            public List<ObservationRecord> allRecords      = new List<ObservationRecord>();
            public List<ObservationRecord> pendingRecords  = new List<ObservationRecord>();
            public List<ObservationRecord> archivedRecords = new List<ObservationRecord>();
        }

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        private List<ObservationRecord> _allRecords      = new List<ObservationRecord>();
        private List<ObservationRecord> _pendingRecords  = new List<ObservationRecord>();
        private List<ObservationRecord> _archivedRecords = new List<ObservationRecord>();

        // ----------------------------------------------------------------
        //  Properties
        // ----------------------------------------------------------------
        public bool HasCosmicTrace
        {
            get
            {
                foreach (var r in _pendingRecords)
                {
                    if (r.type == RecordType.CosmicTrace) return true;
                }
                return false;
            }
        }

        // ----------------------------------------------------------------
        //  Journal operations
        // ----------------------------------------------------------------
        public void AddRecord(ObservationRecord record)
        {
            if (record == null) return;

            record.isDiscoveredThisNight = true;
            _pendingRecords.Add(record);
            _allRecords.Add(record);
        }

        public List<ObservationRecord> GetAllRecords()
        {
            return new List<ObservationRecord>(_allRecords);
        }

        public List<ObservationRecord> GetPendingRecords()
        {
            return new List<ObservationRecord>(_pendingRecords);
        }

        public List<ObservationRecord> GetArchivedRecords()
        {
            return new List<ObservationRecord>(_archivedRecords);
        }

        /// <summary>pending 기록 전체를 archived로 이동합니다. RecordArchive 상호작용 시 호출하십시오.</summary>
        public void ArchivePendingRecords()
        {
            foreach (var r in _pendingRecords)
            {
                r.isDiscoveredThisNight = false;
                _archivedRecords.Add(r);
            }

            int count = _pendingRecords.Count;
            _pendingRecords.Clear();

            Debug.LogFormat("[ObservationJournal] ArchivePendingRecords — {0}개 기록 보관 완료.", count);
        }

        public DisposeResult DisposePendingRecord(string id, DisposalMethod method)
        {
            ObservationRecord target = _pendingRecords.Find(r => r.id == id);
            if (target == null)
            {
                Debug.LogWarningFormat("[ObservationJournal] DisposePendingRecord: id '{0}' not found in pending list.", id);
                return null;
            }

            DisposeResult result = DisposalTable.Calculate(target, method);

            // Apply parameter changes
            PlayerParameters p = PlayerParameters.Singleton;
            if (result.fameChange          != 0f) p.AddFame(result.fameChange);
            if (result.sanityChange        != 0f) p.AddSanity(result.sanityChange);
            if (result.enlightenmentChange != 0f) p.AddEnlightenment(result.enlightenmentChange);
            if (result.madnessChange       != 0f) p.AddMadness(result.madnessChange);
            if (result.fundsChange         != 0.0) p.AddFunds(result.fundsChange);

            _pendingRecords.Remove(target);
            return result;
        }

        /// <summary>Clears the pending list without applying any effects. Call at the start of each night.</summary>
        public void ClearPendingRecords()
        {
            foreach (var r in _pendingRecords)
            {
                r.isDiscoveredThisNight = false;
            }
            _pendingRecords.Clear();
        }

        /// <summary>
        /// 미보관(pending) 기록을 전체 기록 목록에서도 제거합니다.
        /// 관측선 침몰 패널티 — 보관되지 않은 관측 기록 소실 시 호출하십시오.
        /// 이미 처리(보관)된 기록은 영향을 받지 않습니다.
        /// </summary>
        public void ClearHeldRecords()
        {
            if (_pendingRecords.Count == 0) return;

            foreach (var r in _pendingRecords)
            {
                _allRecords.Remove(r);
            }

            int lost = _pendingRecords.Count;
            _pendingRecords.Clear();

            Debug.LogFormat("[ObservationJournal] 침몰 패널티 — 미보관 기록 {0}개 소실.", lost);
        }

        // ----------------------------------------------------------------
        //  Persistence
        // ----------------------------------------------------------------
        public JournalSaveData ToSaveData()
        {
            return new JournalSaveData
            {
                allRecords      = new List<ObservationRecord>(_allRecords),
                pendingRecords  = new List<ObservationRecord>(_pendingRecords),
                archivedRecords = new List<ObservationRecord>(_archivedRecords)
            };
        }

        public void FromSaveData(JournalSaveData data)
        {
            if (data == null) return;
            _allRecords      = data.allRecords      ?? new List<ObservationRecord>();
            _pendingRecords  = data.pendingRecords  ?? new List<ObservationRecord>();
            _archivedRecords = data.archivedRecords ?? new List<ObservationRecord>();
        }

        public void Save()
        {
            // TODO: FileManager.WriteFileFromString("save/journal.json", JsonUtility.ToJson(ToSaveData()));
        }

        public void Load()
        {
            // TODO:
            // if (FileManager.ReadFileData("save/journal.json", out string json))
            //     FromSaveData(JsonUtility.FromJson<JournalSaveData>(json));
        }
    }
}
