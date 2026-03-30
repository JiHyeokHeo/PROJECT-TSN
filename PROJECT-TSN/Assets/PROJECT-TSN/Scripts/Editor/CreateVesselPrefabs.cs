using UnityEditor;
using UnityEngine;

namespace TST.Editor
{
    /// <summary>
    /// 관측선(Vessel) 프리팹 생성 메뉴.
    /// Tools > TST > Create Vessel Prefabs 를 실행하면
    /// Assets/PROJECT-TSN/Prefabs/Fishing/ 아래에 두 개의 프리팹을 만듭니다.
    ///   - Vessel.prefab          : 관측선 본체 + 스프라이트 + 발밑 포인터
    ///   - VesselCamera.prefab    : 45° 쿼터뷰 추적 카메라
    /// </summary>
    public static class CreateVesselPrefabs
    {
        private const string PrefabFolder = "Assets/PROJECT-TSN/Prefabs/Fishing";

        [MenuItem("Tools/TST/Create Vessel Prefabs")]
        public static void Create()
        {
            EnsureFolder(PrefabFolder);
            CreateVesselPrefab();
            CreateVesselCameraPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TST] Vessel 프리팹 생성 완료 → " + PrefabFolder);
        }

        // ── Vessel.prefab ─────────────────────────────────────────────

        private static void CreateVesselPrefab()
        {
            // ── Root ─────────────────────────────────────────────────
            var root = new GameObject("Vessel");
            root.AddComponent<VesselController>();

            // ── Sprite 자식 ──────────────────────────────────────────
            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(root.transform, false);

            var sr = spriteGo.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 10;

            spriteGo.AddComponent<VesselSpriteController>();

            // ── Pointer 자식 (발밑 원형 포인터) ─────────────────────
            var pointerGo = new GameObject("DirectionPointer");
            pointerGo.transform.SetParent(root.transform, false);
            // 바닥면에 눕힘 (XZ 평면)
            pointerGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // 발밑 위치 — Y를 살짝 올려 Z-fighting 방지
            pointerGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);

            var pointerSr = pointerGo.AddComponent<SpriteRenderer>();
            pointerSr.sortingLayerName = "Default";
            pointerSr.sortingOrder     = 9;

            pointerGo.AddComponent<VesselDirectionPointer>();

            // ── 프리팹 저장 ──────────────────────────────────────────
            string path = PrefabFolder + "/Vessel.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log("[TST] Vessel.prefab 저장: " + path);
        }

        // ── VesselCamera.prefab ───────────────────────────────────────

        private static void CreateVesselCameraPrefab()
        {
            var camGo = new GameObject("VesselCamera");

            var cam            = camGo.AddComponent<Camera>();
            cam.clearFlags     = CameraClearFlags.Skybox;
            cam.fieldOfView    = 60f;
            cam.nearClipPlane  = 0.3f;
            cam.farClipPlane   = 200f;

            // 초기 45° 시점 회전 / 위치는 VesselCameraController가 런타임에 설정
            camGo.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

            var ctrl = camGo.AddComponent<VesselCameraController>();

            // 프리팹 저장
            string path = PrefabFolder + "/VesselCamera.prefab";
            PrefabUtility.SaveAsPrefabAsset(camGo, path);
            Object.DestroyImmediate(camGo);

            Debug.Log("[TST] VesselCamera.prefab 저장: " + path);
        }

        // ── 유틸 ─────────────────────────────────────────────────────

        private static void EnsureFolder(string folderPath)
        {
            // "Assets/A/B/C" 를 분해해 단계별로 CreateFolder
            string[] parts  = folderPath.Split('/');
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
