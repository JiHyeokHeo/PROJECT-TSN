using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// ObservationZone ScriptableObject 에셋 일괄 생성 메뉴.
    /// Tools > TST > Create Observation Zone Assets 를 실행하면
    /// Assets/PROJECT-TSN/ScriptableObjects/ObservationZones/ 아래에
    /// zone_1 ~ zone_9 까지 9개의 에셋을 생성합니다.
    /// </summary>
    public static class CreateObservationZoneAssets
    {
        private const string OutputFolder = "Assets/PROJECT-TSN/ScriptableObjects/ObservationZones";

        [MenuItem("Tools/TST/Create Observation Zone Assets")]
        public static void Create()
        {
            EnsureFolder(OutputFolder);

            foreach (var def in ZoneDefinitions)
                CreateAsset(def);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TST] ObservationZone 에셋 생성 완료 ({ZoneDefinitions.Length}개) → {OutputFolder}");
        }

        // ── 9개 구역 사양 ────────────────────────────────────────────

        private static readonly ZoneDef[] ZoneDefinitions =
        {
            // ── Tier 1: requiredLensLevel = 1 ────────────────────────
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_1",
                zoneId             = "zone_1",
                zoneName           = "근지구 궤도",
                requiredLensLevel  = 1,
                availableTypes     = new[] { RecordType.CelestialBody, RecordType.Phenomenon },
                // Common 위주: C=0.60 U=0.28 R=0.10 L=0.02
                rarityWeights      = new[] { 0.60f, 0.28f, 0.10f, 0.02f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_2",
                zoneId             = "zone_2",
                zoneName           = "내행성 벨트",
                requiredLensLevel  = 1,
                availableTypes     = new[] { RecordType.CelestialBody, RecordType.CosmicTrace },
                // C=0.58 U=0.28 R=0.11 L=0.03
                rarityWeights      = new[] { 0.58f, 0.28f, 0.11f, 0.03f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_3",
                zoneId             = "zone_3",
                zoneName           = "황도 성운 가장자리",
                requiredLensLevel  = 1,
                availableTypes     = new[] { RecordType.CelestialBody, RecordType.Phenomenon, RecordType.CosmicTrace },
                // C=0.55 U=0.30 R=0.12 L=0.03
                rarityWeights      = new[] { 0.55f, 0.30f, 0.12f, 0.03f }
            },

            // ── Tier 2: requiredLensLevel = 2 ────────────────────────
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_4",
                zoneId             = "zone_4",
                zoneName           = "외행성 산개 성단",
                requiredLensLevel  = 2,
                availableTypes     = new[] { RecordType.CelestialBody, RecordType.Phenomenon },
                // C=0.45 U=0.32 R=0.17 L=0.06
                rarityWeights      = new[] { 0.45f, 0.32f, 0.17f, 0.06f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_5",
                zoneId             = "zone_5",
                zoneName           = "심우주 성운대",
                requiredLensLevel  = 2,
                availableTypes     = new[] { RecordType.CelestialBody, RecordType.Phenomenon, RecordType.CosmicTrace },
                // C=0.43 U=0.30 R=0.20 L=0.07
                rarityWeights      = new[] { 0.43f, 0.30f, 0.20f, 0.07f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_6",
                zoneId             = "zone_6",
                zoneName           = "이중성 조류 지대",
                requiredLensLevel  = 2,
                availableTypes     = new[] { RecordType.Phenomenon, RecordType.CosmicTrace },
                // C=0.40 U=0.32 R=0.20 L=0.08
                rarityWeights      = new[] { 0.40f, 0.32f, 0.20f, 0.08f }
            },

            // ── Tier 3: requiredLensLevel = 3 ────────────────────────
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_7",
                zoneId             = "zone_7",
                zoneName           = "공허의 틈새",
                requiredLensLevel  = 3,
                availableTypes     = new[] { RecordType.CosmicTrace, RecordType.Phenomenon },
                // Rare/Legendary 비중 높음: C=0.30 U=0.28 R=0.27 L=0.15
                rarityWeights      = new[] { 0.30f, 0.28f, 0.27f, 0.15f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_8",
                zoneId             = "zone_8",
                zoneName           = "블랙홀 사건 지평 외곽",
                requiredLensLevel  = 3,
                availableTypes     = new[] { RecordType.CosmicTrace, RecordType.Phenomenon, RecordType.CelestialBody },
                // C=0.28 U=0.26 R=0.28 L=0.18
                rarityWeights      = new[] { 0.28f, 0.26f, 0.28f, 0.18f }
            },
            new ZoneDef
            {
                fileName           = "ObservationZone_zone_9",
                zoneId             = "zone_9",
                zoneName           = "원시 우주 잔광 지대",
                requiredLensLevel  = 3,
                availableTypes     = new[] { RecordType.CosmicTrace, RecordType.Phenomenon },
                // C=0.25 U=0.25 R=0.30 L=0.20
                rarityWeights      = new[] { 0.25f, 0.25f, 0.30f, 0.20f }
            },
        };

        // ── 에셋 생성 헬퍼 ───────────────────────────────────────────

        private static void CreateAsset(ZoneDef def)
        {
            string assetPath = $"{OutputFolder}/{def.fileName}.asset";

            // 이미 존재하면 덮어쓰기 여부를 묻지 않고 건너뜀
            ObservationZone existing = AssetDatabase.LoadAssetAtPath<ObservationZone>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            ObservationZone so = ScriptableObject.CreateInstance<ObservationZone>();
            so.zoneId            = def.zoneId;
            so.zoneName          = def.zoneName;
            so.requiredLensLevel = def.requiredLensLevel;
            so.availableTypes    = def.availableTypes;
            so.rarityWeights     = def.rarityWeights;

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 폴더 보장 ────────────────────────────────────────────────

        private static void EnsureFolder(string folderPath)
        {
            string[] parts   = folderPath.Split('/');
            string   current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // ── 내부 정의용 데이터 구조체 ────────────────────────────────

        private struct ZoneDef
        {
            public string       fileName;
            public string       zoneId;
            public string       zoneName;
            public int          requiredLensLevel;
            public RecordType[] availableTypes;
            public float[]      rarityWeights;
        }
    }
}
