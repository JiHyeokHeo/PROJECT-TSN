using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 필드 오브젝트 프리팹 생성 메뉴.
    /// Tools > TST > Create Fishing Field Prefabs 를 실행하면
    /// Assets/PROJECT-TSN/Prefabs/Fishing/ 아래에 세 개의 프리팹을 만듭니다.
    ///   - FishingGround.prefab  : 어장 — SpriteRenderer + CircleCollider2D + FishingGround
    ///   - Hazard.prefab         : 위험 요소 — SpriteRenderer + BoxCollider2D(isTrigger) + Hazard
    ///   - RecordArchive.prefab  : 기록 보관소 — SpriteRenderer + CircleCollider2D + RecordArchive
    /// </summary>
    public static class CreateFishingFieldPrefabs
    {
        private const string PrefabFolder = "Assets/PROJECT-TSN/Prefabs/Fishing";

        [MenuItem("Tools/TST/Create Fishing Field Prefabs")]
        public static void Create()
        {
            EnsureFolder(PrefabFolder);

            CreateFishingGroundPrefab();
            CreateHazardPrefab();
            CreateRecordArchivePrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TST] Fishing Field 프리팹 생성 완료 → " + PrefabFolder);
        }

        // ── FishingGround.prefab ──────────────────────────────────────

        private static void CreateFishingGroundPrefab()
        {
            string path = PrefabFolder + "/FishingGround.prefab";

            // 이미 존재하면 경고만 하고 덮어쓰지 않음
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.LogWarning("[TST] FishingGround.prefab 이 이미 존재합니다 — 건너뜁니다: " + path);
                return;
            }

            var root = new GameObject("FishingGround");

            // SpriteRenderer
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 5;

            // CircleCollider2D (상호작용 범위)
            var col = root.AddComponent<CircleCollider2D>();
            col.radius    = 0.5f;
            col.isTrigger = false;

            // FishingGround 스크립트
            root.AddComponent<FishingGround>();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log("[TST] FishingGround.prefab 저장: " + path);
        }

        // ── Hazard.prefab ─────────────────────────────────────────────

        private static void CreateHazardPrefab()
        {
            string path = PrefabFolder + "/Hazard.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.LogWarning("[TST] Hazard.prefab 이 이미 존재합니다 — 건너뜁니다: " + path);
                return;
            }

            var root = new GameObject("Hazard");

            // SpriteRenderer
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 5;

            // BoxCollider2D — isTrigger = true (OnTriggerEnter2D 사용)
            var col = root.AddComponent<BoxCollider2D>();
            col.size      = Vector2.one;
            col.isTrigger = true;

            // Hazard 스크립트
            root.AddComponent<Hazard>();

            // 관측선 충돌 감지를 위해 태그는 "Vessel"로 설정하는 쪽(관측선 프리팹)에서 담당.
            // 이 프리팹에는 별도 태그 불필요.

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log("[TST] Hazard.prefab 저장: " + path);
        }

        // ── RecordArchive.prefab ──────────────────────────────────────

        private static void CreateRecordArchivePrefab()
        {
            string path = PrefabFolder + "/RecordArchive.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.LogWarning("[TST] RecordArchive.prefab 이 이미 존재합니다 — 건너뜁니다: " + path);
                return;
            }

            var root = new GameObject("RecordArchive");

            // SpriteRenderer
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 5;

            // CircleCollider2D (상호작용 범위)
            var col = root.AddComponent<CircleCollider2D>();
            col.radius    = 0.6f;
            col.isTrigger = false;

            // RecordArchive 스크립트
            root.AddComponent<RecordArchive>();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log("[TST] RecordArchive.prefab 저장: " + path);
        }

        // ── 유틸 ─────────────────────────────────────────────────────

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
    }
}
