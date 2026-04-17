using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TST
{
    public class UIManager : SingletonBase<UIManager>
    {
        public event Action<UIList, bool, UIBase> OnUIVisibilityChanged;

        public static T Show<T>(UIList uiPrefabName) where T : UIBase
        {
            if (Singleton.GetUI<T>(uiPrefabName, out T result))
            {
                Singleton.RegisterUIInstance(uiPrefabName, result);
                result.Show();

                if (result.IsVisibleCursor)
                {
                    InputSystem.Singleton.ChangeCursorVisibility(true);

                    if (Singleton.cursorVisibleUIs.Contains(result) == false)
                        Singleton.cursorVisibleUIs.Add(result);
                }

                return result;
            }

            return null;
        }

        public static T Hide<T>(UIList uiPrefabName) where T : UIBase
        {
            if (Singleton.GetUI<T>(uiPrefabName, out T result))
            {
                Singleton.RegisterUIInstance(uiPrefabName, result);
                result.Hide();

                if (result.IsVisibleCursor)
                {
                    Singleton.cursorVisibleUIs.Remove(result);
                    if (Singleton.cursorVisibleUIs.Count <= 0)
                    {
                        InputSystem.Singleton.ChangeCursorVisibility(false);
                    }
                }

                return result;
            }

            return null;
        }

        [field: SerializeField] public Camera UICamera { get; private set; } = null;
        public int ActiveCursorVisibleUIsCount => cursorVisibleUIs.Count;

        private Dictionary<UIList, UIBase> panels = new Dictionary<UIList, UIBase>();
        private Dictionary<UIList, UIBase> popups = new Dictionary<UIList, UIBase>();
        private Dictionary<UIBase, UIList> uiTypeByInstance = new Dictionary<UIBase, UIList>();

        private Transform panelRoot;
        private Transform popupRoot;

        private List<UIBase> cursorVisibleUIs = new List<UIBase>();

        private const string UI_PREFAB_PATH = "UI/Prefabs/";

        public void Initialize()
        {
            for (int index = (int)UIList.UI_PANEL_START + 1; index < (int)UIList.UI_PANEL_END; index++)
            {
                panels.Add((UIList)index, null);
            }

            for (int index = (int)UIList.UI_POPUP_START + 1; index < (int)UIList.UI_POPUP_END; index++)
            {
                popups.Add((UIList)index, null);
            }

            if (UICamera == null)
            {
                GameObject uiCameraGo = new GameObject("UICamera");
                uiCameraGo.transform.SetParent(transform);
                UICamera = uiCameraGo.AddComponent<Camera>();
                UICamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
                UICamera.clearFlags = CameraClearFlags.Depth;
                var cameraData = UICamera.GetUniversalAdditionalCameraData();
                cameraData.renderType = CameraRenderType.Overlay;
            }

            if (panelRoot == null)
            {
                GameObject panelGo = new GameObject("Panel Root");
                panelRoot = panelGo.transform;
                panelRoot.SetParent(transform);
            }

            if (popupRoot == null)
            {
                GameObject popupGo = new GameObject("Popup Root");
                popupRoot = popupGo.transform;
                popupRoot.SetParent(transform);
            }
        }

        public bool GetUI<T>(UIList uiPrefabName, out T result) where T : UIBase
        {
            result = GetUI<T>(uiPrefabName);
            return result != null;
        }

        public T GetUI<T>(UIList uiPrefabName, bool reload = false) where T : UIBase
        {
            Dictionary<UIList, UIBase> container = null;
            container = (int)uiPrefabName > (int)UIList.UI_PANEL_START && (int)uiPrefabName < (int)UIList.UI_PANEL_END ? panels : popups;


            if (!container.ContainsKey(uiPrefabName))
            {
                return null;
            }

            if (reload && container[uiPrefabName] != null)
            {
                Destroy(container[uiPrefabName].gameObject);
                container[uiPrefabName] = null;
            }

            if (!container[uiPrefabName])
            {
                GameObject uiPrefab = Resources.Load<GameObject>(UI_PREFAB_PATH + $"UI.{uiPrefabName}");
                GameObject uiGo = Instantiate(uiPrefab, container == panels ? panelRoot : popupRoot);
                T ui = uiGo.GetComponent<T>();
                container[uiPrefabName] = ui;
                if (container[uiPrefabName])
                {
                    container[uiPrefabName].gameObject.SetActive(false);
                }
            }

            RegisterUIInstance(uiPrefabName, container[uiPrefabName]);

            return (T)container[uiPrefabName];
        }

        public void HideAllUI()
        {
            foreach (var panel in panels)
            {
                if (panel.Value != null)
                {
                    panel.Value.Hide();
                }
            }

            foreach (var popup in popups)
            {
                if (popup.Value != null)
                {
                    popup.Value.Hide();
                }
            }
        }

        public static bool TryGetExisting(out UIManager uiManager)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            return uiManager != null;
        }

        internal static void NotifyVisibilityChangedFromUIBase(UIBase ui, bool isVisible)
        {
            if (ui == null)
                return;

            if (TryGetExisting(out UIManager uiManager))
            {
                uiManager.NotifyVisibilityChanged(ui, isVisible);
            }
        }

        private void RegisterUIInstance(UIList uiType, UIBase uiInstance)
        {
            if (uiInstance == null)
                return;

            uiTypeByInstance[uiInstance] = uiType;
        }

        private void NotifyVisibilityChanged(UIBase uiInstance, bool isVisible)
        {
            if (uiInstance == null)
                return;

            if (!TryResolveUIType(uiInstance, out UIList uiType))
                return;

            OnUIVisibilityChanged?.Invoke(uiType, isVisible, uiInstance);
        }

        private bool TryResolveUIType(UIBase uiInstance, out UIList uiType)
        {
            if (uiInstance == null)
            {
                uiType = default;
                return false;
            }

            if (uiTypeByInstance.TryGetValue(uiInstance, out uiType))
                return true;

            string normalizedName = NormalizeUIObjectName(uiInstance.gameObject.name);
            if (Enum.TryParse(normalizedName, true, out uiType))
            {
                uiTypeByInstance[uiInstance] = uiType;
                return true;
            }

            return false;
        }

        private static string NormalizeUIObjectName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
                return string.Empty;

            string normalizedName = rawName.Replace("(Clone)", string.Empty).Trim();
            if (normalizedName.StartsWith("UI.", StringComparison.Ordinal))
                normalizedName = normalizedName.Substring(3);

            return normalizedName;
        }
    }
}
