using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TST.Editor
{
    /// <summary>
    /// Starry Night 패널 / 팝업 프리팹 일괄 생성 메뉴.
    /// Tools > TST > Create UI Prefabs
    ///
    /// 저장 경로: Assets/Resources/UI/Prefabs/UI.{UIList 이름}.prefab
    ///   UIManager.cs 의 UI_PREFAB_PATH = "UI/Prefabs/" 와 일치하는 Resources 경로.
    ///
    /// 생성 대상 (Panels):
    ///   UI.Panel_Title             — TitleUI
    ///   UI.Panel_Cutscene          — CutsceneController
    ///   UI.Panel_SaveScreen        — SaveScreenUI
    ///   UI.Panel_LoadScreen        — LoadScreenUI
    ///   UI.Panel_DreamKeySelection — DreamKeySelectionUI
    ///   UI.Panel_DreamVN           — DreamVNPanel
    ///
    /// 생성 대상 (Popups):
    ///   UI.Popup_Menu              — MenuPopupUI
    ///   UI.Popup_Options           — OptionsPopupUI
    ///   UI.Popup_DiceRoll          — DiceRollPopupUI
    ///   UI.Popup_Choice            — ChoicePopupUI
    ///   UI.Popup_NightTelescope    — NightTelescopePopupUI
    ///
    /// Inspector 와이어링:
    ///   각 컴포넌트의 [SerializeField] 필드는 프리팹 생성 후
    ///   Unity Inspector 에서 수동으로 연결하십시오.
    /// </summary>
    public static class CreateUIPrefabs
    {
        private const string ResourcesRoot = "Assets/Resources/UI/Prefabs";

        private const float ReferenceWidth  = 1920f;
        private const float ReferenceHeight = 1080f;

        private const int PanelSortingOrder = 100;
        private const int PopupSortingOrder = 200;

        [MenuItem("Tools/TST/Create UI Prefabs")]
        public static void Create()
        {
            EnsureFolder(ResourcesRoot);

            // Panels
            CreatePrefab<TitleUI>            ("Panel_Title",             PanelSortingOrder);
            CreatePrefab<CutsceneController> ("Panel_Cutscene",          PanelSortingOrder);
            CreatePrefab<SaveScreenUI>       ("Panel_SaveScreen",        PanelSortingOrder);
            CreatePrefab<LoadScreenUI>       ("Panel_LoadScreen",        PanelSortingOrder);
            CreatePrefab<DreamKeySelectionUI>("Panel_DreamKeySelection", PanelSortingOrder);
            CreatePrefab<DreamVNPanel>       ("Panel_DreamVN",           PanelSortingOrder);

            // Popups
            CreatePrefab<MenuPopupUI>           ("Popup_Menu",           PopupSortingOrder);
            CreatePrefab<OptionsPopupUI>        ("Popup_Options",        PopupSortingOrder);
            CreatePrefab<DiceRollPopupUI>       ("Popup_DiceRoll",       PopupSortingOrder);
            CreatePrefab<ChoicePopupUI>         ("Popup_Choice",         PopupSortingOrder);
            CreatePrefab<NightTelescopePopupUI> ("Popup_NightTelescope", PopupSortingOrder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TST] UI 프리팹 생성 완료 → " + ResourcesRoot);
        }

        /// <summary>
        /// Canvas + CanvasScaler + GraphicRaycaster 루트,
        /// 자식 Panel (Image + CanvasGroup) + T 컴포넌트를 추가한 뒤 저장합니다.
        /// 이미 같은 경로에 프리팹이 존재하면 건너뜁니다.
        /// </summary>
        private static void CreatePrefab<T>(string uiName, int sortingOrder)
            where T : UIBase
        {
            string assetName = "UI." + uiName;
            string path      = ResourcesRoot + "/" + assetName + ".prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log($"[TST] 스킵 (이미 존재): {path}");
                return;
            }

            // Root: Canvas
            var root   = new GameObject(assetName);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight  = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            // Child: Panel (투명 전체화면 배경)
            var panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);
            var rect       = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            panel.AddComponent<CanvasGroup>();

            // UI 컴포넌트 추가
            root.AddComponent<T>();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[TST] 생성: {path}  ({typeof(T).Name})");
        }

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
