using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// VesselCamera RenderTexture 구조 설정 에디터 스크립트.
    ///
    /// 수행 작업:
    ///   1. VesselView GameObject의 Image 컴포넌트를 RawImage로 교체
    ///   2. VesselView RectTransform을 stretch-all (LeftFrame 꽉 채우기)로 설정
    ///   3. VesselCameraController의 viewportFrame / displayImage 필드 자동 연결
    ///
    /// 메뉴: PROJECT-TSN > Setup VesselCamera RenderTexture
    /// 또는 컴파일 직후 자동 1회 실행 (EditorPrefs 플래그로 중복 방지)
    /// </summary>
    [InitializeOnLoad]
    public static class SetupVesselCameraRT
    {
        private const string DoneKey = "TST_VesselCameraRT_Done";

        static SetupVesselCameraRT()
        {
            // 컴파일 후 도메인 리로드 시 1회 자동 실행
            if (!EditorPrefs.GetBool(DoneKey, false))
            {
                // DelayCall로 씬이 완전히 로드된 뒤 실행
                EditorApplication.delayCall += AutoRun;
            }
        }

        private static void AutoRun()
        {
            EditorApplication.delayCall -= AutoRun;
            if (EditorPrefs.GetBool(DoneKey, false)) return;

            bool success = Run();
            if (success)
                EditorPrefs.SetBool(DoneKey, true);
        }

        [MenuItem("PROJECT-TSN/Setup VesselCamera RenderTexture")]
        public static void RunFromMenu()
        {
            // 메뉴에서 강제 재실행 시 플래그 초기화
            EditorPrefs.DeleteKey(DoneKey);
            Run();
        }

        /// <summary>씬 작업을 수행하고 성공 여부를 반환합니다.</summary>
        private static bool Run()
        {
            // ── 1. VesselView 찾기 ────────────────────────────────────
            GameObject vesselViewGO = GameObject.Find("VesselView");
            if (vesselViewGO == null)
            {
                Debug.LogError("[SetupVesselCameraRT] 'VesselView' GameObject를 씬에서 찾을 수 없습니다.");
                return false;
            }

            // ── 2. Image → RawImage 교체 ─────────────────────────────
            Image existingImage = vesselViewGO.GetComponent<Image>();
            if (existingImage != null)
            {
                Undo.DestroyObjectImmediate(existingImage);
                Debug.Log("[SetupVesselCameraRT] Image 컴포넌트 제거 완료.");
            }

            RawImage rawImage = vesselViewGO.GetComponent<RawImage>();
            if (rawImage == null)
            {
                rawImage = Undo.AddComponent<RawImage>(vesselViewGO);
                Debug.Log("[SetupVesselCameraRT] RawImage 컴포넌트 추가 완료.");
            }

            // 카메라 뷰 전용 — raycast 불필요
            rawImage.raycastTarget = false;

            // ── 3. RectTransform stretch-all 설정 ────────────────────
            RectTransform rt = vesselViewGO.GetComponent<RectTransform>();
            Undo.RecordObject(rt, "VesselView stretch-all");
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;
            rt.pivot            = new Vector2(0.5f, 0.5f);
            Debug.Log("[SetupVesselCameraRT] VesselView RectTransform stretch-all 설정 완료.");

            // ── 4. LeftFrame 찾기 ─────────────────────────────────────
            GameObject leftFrameGO = GameObject.Find("LeftFrame");
            if (leftFrameGO == null)
            {
                Debug.LogWarning("[SetupVesselCameraRT] 'LeftFrame'을 찾을 수 없습니다. Inspector 연결은 수동으로 해주세요.");
                MarkDirtyAndLog();
                return true;
            }
            RectTransform leftFrameRT = leftFrameGO.GetComponent<RectTransform>();

            // ── 5. VesselCamera 찾기 ──────────────────────────────────
            GameObject vesselCameraGO = GameObject.Find("VesselCamera");
            if (vesselCameraGO == null)
            {
                Debug.LogWarning("[SetupVesselCameraRT] 'VesselCamera'를 찾을 수 없습니다. Inspector 연결은 수동으로 해주세요.");
                MarkDirtyAndLog();
                return true;
            }

            VesselCameraController controller = vesselCameraGO.GetComponent<VesselCameraController>();
            if (controller == null)
            {
                Debug.LogWarning("[SetupVesselCameraRT] VesselCamera에 VesselCameraController가 없습니다.");
                MarkDirtyAndLog();
                return true;
            }

            // ── 6. SerializedObject로 viewportFrame / displayImage 연결
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("viewportFrame").objectReferenceValue = leftFrameRT;
            so.FindProperty("displayImage").objectReferenceValue  = rawImage;
            so.ApplyModifiedProperties();
            Debug.Log("[SetupVesselCameraRT] VesselCameraController 필드 연결 완료: viewportFrame=LeftFrame, displayImage=VesselView");

            MarkDirtyAndLog();
            return true;
        }

        private static void MarkDirtyAndLog()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[SetupVesselCameraRT] 완료. Ctrl+S 로 씬을 저장하세요.");
        }
    }
}
