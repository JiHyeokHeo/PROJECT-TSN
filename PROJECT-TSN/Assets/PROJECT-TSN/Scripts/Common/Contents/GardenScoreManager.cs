using System;
using System.Collections.Generic;
using UnityEngine;
using TST;

public struct GardenScoreResult
{
    public int total;
    public int baseScore;
    public int synergyScore;
    public int varietyScore;

    public EGardenGrade grade;
    public EGardenGrade nextGrade;
    public float gradeProgress01;
    public int gradeMinScore;
    public int nextGradeMinScore;
}

public class GardenScoreManager : SingletonBase<GardenScoreManager>
{
    [SerializeField] GardenScoreConfigSO scoreConfig;
    [SerializeField] GardenGradeConfigSO gradeConfig;

    public event Action<GardenScoreResult> OnScoreChanged;
    public GardenScoreResult Current { get; private set; }

    void OnEnable()
    {
        if (UserDataModel.Singleton != null)
            UserDataModel.Singleton.OnGardenChanged += Recalculate;
    }

    void OnDisable()
    {
        if (UserDataModel.Singleton != null)
            UserDataModel.Singleton.OnGardenChanged -= Recalculate;
    }

    void Start()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        if (scoreConfig == null)
        {
            Debug.LogError("GardenScoreManager: scoreConfig is null");
            return;
        }
        if (UserDataModel.Singleton == null || GameDataModel.Singleton == null)
            return;

        Dictionary<string, int> itemIdCounts = new();
        Dictionary<string, int> themeCounts = new();
        HashSet<EDecorationCategory> categories = new();

        int baseScore = 0;

        foreach (var p in UserDataModel.Singleton.GetAllPlaced())
        {
            if (!GameDataModel.Singleton.GetItemData(p.itemId, out var def) || def == null)
                continue;

            // Base: same itemId diminishing
            itemIdCounts.TryGetValue(def.itemId, out int cur);
            int next = cur + 1;
            itemIdCounts[def.itemId] = next;

            float mul = GetRepeatMultiplier(next - 1);
            baseScore += Mathf.RoundToInt(def.scoreValue * mul);

            // Synergy: themeTag
            if (!string.IsNullOrEmpty(def.themeTag))
            {
                themeCounts.TryGetValue(def.themeTag, out int t);
                themeCounts[def.themeTag] = t + 1;
            }

            // Variety: decoration category
            if (def.itemType == EItemType.Decoration)
                categories.Add(def.decorationCategory);
        }

        int synergyScore = 0;
        foreach (var kv in themeCounts)
        {
            int tiers = Mathf.FloorToInt(kv.Value / Mathf.Max(1, scoreConfig.synergyGroupSize));
            tiers = Mathf.Min(tiers, scoreConfig.synergyTierCap);
            synergyScore += tiers * scoreConfig.synergyUnit;
        }

        int varietyScore = 0;
        int u = categories.Count;
        if (u > 1)
        {
            int step = Mathf.Min(u - 1, scoreConfig.varietyCap);
            varietyScore = step * scoreConfig.varietyUnit;
        }

        int total = baseScore + synergyScore + varietyScore;

        // Grade evaluate
        EGardenGrade grade = EGardenGrade.None;
        EGardenGrade nextGrade = EGardenGrade.None;
        float prog01 = 0f;
        int gradeMin = 0;
        int nextMin = 0;

        if (gradeConfig != null)
            gradeConfig.Evaluate(total, out grade, out nextGrade, out prog01, out gradeMin, out nextMin);

        Current = new GardenScoreResult
        {
            total = total,
            baseScore = baseScore,
            synergyScore = synergyScore,
            varietyScore = varietyScore,
            grade = grade,
            nextGrade = nextGrade,
            gradeProgress01 = prog01,
            gradeMinScore = gradeMin,
            nextGradeMinScore = nextMin
        };

        OnScoreChanged?.Invoke(Current);

        // Day-4 검증용 (UI 붙이기 전까지)
        Debug.Log($"GardenScore total={total} base={baseScore} syn={synergyScore} var={varietyScore} grade={grade} -> {nextGrade} ({prog01:0.00})");
    }

    float GetRepeatMultiplier(int index)
    {
        if (scoreConfig.repeatMultipliers == null || scoreConfig.repeatMultipliers.Count == 0)
            return 1f;

        if (index < 0) return 1f;
        if (index < scoreConfig.repeatMultipliers.Count) return scoreConfig.repeatMultipliers[index];
        return scoreConfig.repeatMultipliers[^1];
    }
}