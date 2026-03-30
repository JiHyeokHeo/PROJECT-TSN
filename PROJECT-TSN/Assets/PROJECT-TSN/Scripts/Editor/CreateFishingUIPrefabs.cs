using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TST.Editor
{
    /// <summary>
    /// 낚시(우주 관측) 페이즈 UI 프리팹 생성 메뉴.
    ///
    /// Tools > TST > Create Fishing UI Prefabs 를 실행하면 두 프리팹을 만듭니다.
    ///
    ///   Resources/UI/Prefabs/UI.Popup_FocusMinigame.prefab
    ///     Canvas (Screen Space Overlay)
    ///       └ Panel (배경 반투명)
    ///           └ ArcBar (원호 배경)
    ///               ├ GreenZone (성공 구간, 녹색)
    ///               └ Pointer   (포인터, 흰색 원)
    ///     + ResultOverlay (전체 화면 결과 연출용)
    ///     + FocusMinigameUI 컴포넌트 (arcPivot = ArcBar)
    ///
    ///   Resources/UI/Prefabs/UI.Panel_FishingHUD.prefab
    ///     Canvas (Screen Space Overlay)
    ///       └ BottomBar (하단 고정 앵커)
    ///           ├ HelmContainer
    ///           │   └ HelmImage (조타륜)
    ///           ├ DurabilityBar (빨강 fill)
    ///           └ SpeedBar      (파랑 fill)
    ///     + FishingHudUI 컴포넌트
    ///
    /// 주의: UIManager 는 Resources.Load("UI/Prefabs/UI.<UIList 열거형 이름>") 으로
    ///       프리팹을 로드합니다. 저장 경로가 이 규칙을 따릅니다.
    /// </summary>
    public static class CreateFishingUIPrefabs
    {
        // UIManager 상수 "UI/Prefabs/" 와 일치
        private const string ResourcesRoot = "Assets/Resources/UI/Prefabs";

        [MenuItem("Tools/TST/Create Fishing UI Prefabs")]
        public static void Create()
        {
            EnsureFolder(ResourcesRoot);

            CreateFocusMinigamePrefab();
            CreateFishingHudPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TST] 낚시 UI 프리팹 생성 완료 → " + ResourcesRoot);
        }

        // ── Popup_FocusMinigame ───────────────────────────────────────

        private static void CreateFocusMinigamePrefab()
        {
            // ── Root: Canvas ─────────────────────────────────────────
            var root = new GameObject("UI.Popup_FocusMinigame");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // ── Panel (배경) ──────────────────────────────────────────
            var panel = CreateUIObject("Panel", root.transform);
            StretchFull(panel);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.55f);

            // ── ArcBar (원호 배경) ────────────────────────────────────
            var arcBar = CreateUIObject("ArcBar", panel.transform);
            var arcBarRect = arcBar.GetComponent<RectTransform>();
            arcBarRect.anchorMin = new Vector2(0.5f, 0.5f);
            arcBarRect.anchorMax = new Vector2(0.5f, 0.5f);
            arcBarRect.sizeDelta = new Vector2(400f, 400f);
            arcBarRect.anchoredPosition = Vector2.zero;

            var arcImg = arcBar.AddComponent<Image>();
            arcImg.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);

            // ── GreenZone ─────────────────────────────────────────────
            var greenZone = CreateUIObject("GreenZone", arcBar.transform);
            var greenRect = greenZone.GetComponent<RectTransform>();
            // arc 위 상단 중앙에 초기 배치 (런타임에 FocusMinigameUI 가 갱신)
            greenRect.anchorMin = new Vector2(0.5f, 0.5f);
            greenRect.anchorMax = new Vector2(0.5f, 0.5f);
            greenRect.sizeDelta = new Vector2(56f, 16f);
            greenRect.anchoredPosition = new Vector2(0f, 180f);

            var greenImg = greenZone.AddComponent<Image>();
            greenImg.color = new Color(0.15f, 0.85f, 0.25f, 0.85f);

            // ── Pointer ───────────────────────────────────────────────
            var pointer = CreateUIObject("Pointer", arcBar.transform);
            var pointerRect = pointer.GetComponent<RectTransform>();
            pointerRect.anchorMin = new Vector2(0.5f, 0.5f);
            pointerRect.anchorMax = new Vector2(0.5f, 0.5f);
            pointerRect.sizeDelta = new Vector2(20f, 20f);
            pointerRect.anchoredPosition = new Vector2(0f, 180f);

            var pointerImg = pointer.AddComponent<Image>();
            pointerImg.color = Color.white;

            // ── ResultOverlay (전체 화면, 투명도 0) ──────────────────
            var resultOverlay = CreateUIObject("ResultOverlay", root.transform);
            StretchFull(resultOverlay);
            var overlayImg = resultOverlay.AddComponent<Image>();
            overlayImg.color = Color.clear;
            overlayImg.raycastTarget = false;

            // ── FocusMinigameUI 컴포넌트 ─────────────────────────────
            var uiComp = root.AddComponent<FocusMinigameUI>();

            // SerializedObject 로 [SerializeField] 와이어링
            var so = new SerializedObject(uiComp);
            so.FindProperty("greenZoneRect").objectReferenceValue  = greenZone.GetComponent<RectTransform>();
            so.FindProperty("pointerRect").objectReferenceValue    = pointerRect;
            so.FindProperty("arcPivot").objectReferenceValue       = arcBarRect;
            so.FindProperty("arcRadius").floatValue                = 180f;
            so.FindProperty("resultOverlay").objectReferenceValue  = overlayImg;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── 저장 ─────────────────────────────────────────────────
            string path = ResourcesRoot + "/UI.Popup_FocusMinigame.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log("[TST] UI.Popup_FocusMinigame.prefab 저장: " + path);
        }

        // ── Panel_FishingHUD ──────────────────────────────────────────

        private static void CreateFishingHudPrefab()
        {
            // ── Root: Canvas ─────────────────────────────────────────
            var root = new GameObject("UI.Panel_FishingHUD");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // ── BottomBar (하단 고정) ─────────────────────────────────
            var bottomBar = CreateUIObject("BottomBar", root.transform);
            var bbRect = bottomBar.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0f, 0f);
            bbRect.anchorMax = new Vector2(1f, 0f);
            bbRect.pivot     = new Vector2(0.5f, 0f);
            bbRect.offsetMin = new Vector2(0f, 0f);
            bbRect.offsetMax = new Vector2(0f, 120f);

            var bbImg = bottomBar.AddComponent<Image>();
            bbImg.color = new Color(0f, 0f, 0f, 0.6f);

            // ── HelmContainer ─────────────────────────────────────────
            var helmContainer = CreateUIObject("HelmContainer", bottomBar.transform);
            var hcRect = helmContainer.GetComponent<RectTransform>();
            hcRect.anchorMin = new Vector2(0.5f, 0.5f);
            hcRect.anchorMax = new Vector2(0.5f, 0.5f);
            hcRect.sizeDelta = new Vector2(90f, 90f);
            hcRect.anchoredPosition = new Vector2(0f, 0f);

            // ── HelmImage (조타륜) ────────────────────────────────────
            var helmGo = CreateUIObject("HelmImage", helmContainer.transform);
            var helmRect = helmGo.GetComponent<RectTransform>();
            helmRect.anchorMin = Vector2.zero;
            helmRect.anchorMax = Vector2.one;
            helmRect.offsetMin = Vector2.zero;
            helmRect.offsetMax = Vector2.zero;

            var helmImg = helmGo.AddComponent<Image>();
            helmImg.color = new Color(0.85f, 0.75f, 0.55f, 1f); // 황동 색상

            // ── DurabilityBar ─────────────────────────────────────────
            var durabilityBar = CreateUIObject("DurabilityBar", bottomBar.transform);
            var dbRect = durabilityBar.GetComponent<RectTransform>();
            dbRect.anchorMin = new Vector2(0f, 0.5f);
            dbRect.anchorMax = new Vector2(0f, 0.5f);
            dbRect.pivot     = new Vector2(0f, 0.5f);
            dbRect.sizeDelta = new Vector2(200f, 20f);
            dbRect.anchoredPosition = new Vector2(20f, 20f);

            var durImg = durabilityBar.AddComponent<Image>();
            durImg.color    = new Color(0.85f, 0.15f, 0.15f, 1f);
            durImg.type     = Image.Type.Filled;
            durImg.fillMethod  = Image.FillMethod.Horizontal;
            durImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            durImg.fillAmount  = 1f;

            // ── SpeedBar ──────────────────────────────────────────────
            var speedBar = CreateUIObject("SpeedBar", bottomBar.transform);
            var sbRect = speedBar.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0f, 0.5f);
            sbRect.anchorMax = new Vector2(0f, 0.5f);
            sbRect.pivot     = new Vector2(0f, 0.5f);
            sbRect.sizeDelta = new Vector2(200f, 20f);
            sbRect.anchoredPosition = new Vector2(20f, -10f);

            var speedImg = speedBar.AddComponent<Image>();
            speedImg.color    = new Color(0.15f, 0.45f, 0.85f, 1f);
            speedImg.type     = Image.Type.Filled;
            speedImg.fillMethod  = Image.FillMethod.Horizontal;
            speedImg.fillOrigin  = (int)Image.OriginHorizontal.Left;
            speedImg.fillAmount  = 0f;

            // ── FishingHudUI 컴포넌트 ────────────────────────────────
            var uiComp = root.AddComponent<FishingHudUI>();

            var so = new SerializedObject(uiComp);
            so.FindProperty("helmImage").objectReferenceValue      = helmImg;
            so.FindProperty("durabilityBar").objectReferenceValue  = durImg;
            so.FindProperty("speedBar").objectReferenceValue       = speedImg;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── 저장 ─────────────────────────────────────────────────
            string path = ResourcesRoot + "/UI.Panel_FishingHUD.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log("[TST] UI.Panel_FishingHUD.prefab 저장: " + path);
        }

        // ── 유틸리티 ─────────────────────────────────────────────────

        /// <summary>RectTransform 이 있는 자식 GameObject 생성.</summary>
        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        /// <summary>RectTransform 을 부모에 꽉 채우도록 앵커 설정.</summary>
        private static void StretchFull(GameObject go)
        {
            var rect     = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>경로를 단계별로 생성합니다 (이미 있으면 건너뜁니다).</summary>
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
