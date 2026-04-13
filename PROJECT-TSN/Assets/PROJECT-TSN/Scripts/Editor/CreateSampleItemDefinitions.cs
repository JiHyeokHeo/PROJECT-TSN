using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// 샘플 ItemDefinitionSO 에셋 일괄 생성 메뉴.
    /// Tools > TST > Create Sample Item Definitions 실행 시
    /// Assets/PROJECT-TSN/Data/Items/ 아래에 12개 샘플 에셋을 생성합니다.
    /// 아이콘은 수동으로 연결하십시오.
    /// </summary>
    public static class CreateSampleItemDefinitions
    {
        private const string OutputFolder = "Assets/PROJECT-TSN/Data/Items";

        [MenuItem("Tools/TST/Create Sample Item Definitions")]
        public static void Create()
        {
            EnsureFolder(OutputFolder);

            foreach (var def in Definitions)
                CreateAsset(def);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TST] ItemDefinitionSO 샘플 에셋 생성 완료 ({Definitions.Length}개) → {OutputFolder}");
        }

        // ── 샘플 정의 12개 ───────────────────────────────────────────

        private static readonly ItemDef[] Definitions =
        {
            // ── CelestialBody · Common ───────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Star_Distant",
                itemId      = "item_star_distant",
                itemName    = "원거리 항성",
                recordType  = RecordType.CelestialBody,
                rarity      = Rarity.Common,
                description = "수백 광년 밖에서 희미하게 빛나는 주계열성. 별다른 이상 징후는 없으나 밤하늘을 채우는 무수한 점들 중 하나다."
            },
            new ItemDef
            {
                fileName    = "Item_Moon_Fragment",
                itemId      = "item_moon_fragment",
                itemName    = "위성 파편",
                recordType  = RecordType.CelestialBody,
                rarity      = Rarity.Common,
                description = "모행성 주위를 도는 소형 위성의 파편. 표면에서 흐릿한 반사광이 관측된다."
            },

            // ── CelestialBody · Uncommon ─────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Binary_Star",
                itemId      = "item_binary_star",
                itemName    = "쌍성계",
                recordType  = RecordType.CelestialBody,
                rarity      = Rarity.Uncommon,
                description = "두 항성이 공통 질량 중심을 공전하는 계. 서로의 중력이 빛 곡선에 미묘한 파문을 일으킨다."
            },
            new ItemDef
            {
                fileName    = "Item_Red_Giant",
                itemId      = "item_red_giant",
                itemName    = "적색 거성",
                recordType  = RecordType.CelestialBody,
                rarity      = Rarity.Uncommon,
                description = "핵연료를 거의 소진한 노화 항성. 대기층이 팽창하며 붉게 물든 빛이 망원경 렌즈를 가득 채웠다."
            },

            // ── CelestialBody · Rare ──────────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Neutron_Star",
                itemId      = "item_neutron_star",
                itemName    = "중성자별",
                recordType  = RecordType.CelestialBody,
                rarity      = Rarity.Rare,
                description = "초신성 폭발 후 남겨진 초고밀도 천체. 규칙적인 전파 맥동이 렌즈에 실려 손끝까지 전해지는 듯하다."
            },

            // ── Phenomenon · Common ──────────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Meteor_Shower",
                itemId      = "item_meteor_shower",
                itemName    = "유성우",
                recordType  = RecordType.Phenomenon,
                rarity      = Rarity.Common,
                description = "행성 공전 궤도가 혜성 잔해와 교차할 때 나타나는 빛의 줄기들. 찰나의 순간이지만 기록할 가치가 있다."
            },
            new ItemDef
            {
                fileName    = "Item_Aurora_Trace",
                itemId      = "item_aurora_trace",
                itemName    = "오로라 잔광",
                recordType  = RecordType.Phenomenon,
                rarity      = Rarity.Common,
                description = "자기권과 태양풍이 충돌하며 남긴 대기 발광 흔적. 색채가 짧게 번졌다 사라진다."
            },

            // ── Phenomenon · Uncommon ────────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Gravitational_Lens",
                itemId      = "item_gravitational_lens",
                itemName    = "중력 렌즈 왜곡",
                recordType  = RecordType.Phenomenon,
                rarity      = Rarity.Uncommon,
                description = "거대 질량체 주위에서 빛이 휘어지며 배경 천체의 상이 복수로 갈라지는 현상. 이론이 현실로 나타나는 순간이다."
            },

            // ── Phenomenon · Rare ─────────────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Pulsar_Burst",
                itemId      = "item_pulsar_burst",
                itemName    = "펄사 방출 폭발",
                recordType  = RecordType.Phenomenon,
                rarity      = Rarity.Rare,
                description = "중성자별이 짧은 시간에 과잉 에너지를 방출하는 순간. 기록지가 탄 듯한 잔열이 느껴질 것만 같다."
            },

            // ── CosmicTrace · Common ─────────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Dust_Cloud",
                itemId      = "item_dust_cloud",
                itemName    = "성간 먼지 구름",
                recordType  = RecordType.CosmicTrace,
                rarity      = Rarity.Common,
                description = "성간 공간에 흩어진 미세 먼지 입자의 집합체. 희미하게 산란된 빛이 존재를 드러낸다."
            },

            // ── CosmicTrace · Uncommon ───────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Dark_Matter_Veil",
                itemId      = "item_dark_matter_veil",
                itemName    = "암흑물질 베일",
                recordType  = RecordType.CosmicTrace,
                rarity      = Rarity.Uncommon,
                description = "빛을 내지도 흡수하지도 않으나 중력으로만 존재를 증명하는 무언가. 기록지에는 공백만 남는다."
            },

            // ── CosmicTrace · Legendary ──────────────────────────────
            new ItemDef
            {
                fileName    = "Item_Void_Whisper",
                itemId      = "item_void_whisper",
                itemName    = "공허의 속삭임",
                recordType  = RecordType.CosmicTrace,
                rarity      = Rarity.Legendary,
                description = "우주의 끝에서 들려오는 것 같은 미약한 신호. 해석할 수 없는 패턴이 반복된다. 이것을 기록한 관측자는 오래도록 잠들지 못했다고 한다."
            },
        };

        // ── 에셋 생성 헬퍼 ───────────────────────────────────────────

        private static void CreateAsset(ItemDef def)
        {
            string assetPath = $"{OutputFolder}/{def.fileName}.asset";

            ItemDefinitionSO existing = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(assetPath);
            if (existing != null)
            {
                Debug.Log($"[TST] 이미 존재하여 건너뜀: {assetPath}");
                return;
            }

            ItemDefinitionSO so = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            so.itemId      = def.itemId;
            so.itemName    = def.itemName;
            so.recordType  = def.recordType;
            so.rarity      = def.rarity;
            so.description = def.description;
            // icon은 수동 연결

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[TST] 생성: {assetPath}");
        }

        // ── 폴더 보장 ────────────────────────────────────────────────

        private static void EnsureFolder(string folderPath)
        {
            string[] parts   = folderPath.Split('/');
            string   current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // ── 내부 정의 구조체 ─────────────────────────────────────────

        private struct ItemDef
        {
            public string     fileName;
            public string     itemId;
            public string     itemName;
            public RecordType recordType;
            public Rarity     rarity;
            public string     description;
        }
    }
}
