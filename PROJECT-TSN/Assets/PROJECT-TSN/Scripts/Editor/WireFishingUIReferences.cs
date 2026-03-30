using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TST.Editor
{
    /// <summary>
    /// 낚시 UI 프리팹의 SerializeField 참조를 씬 계층에서 자동으로 연결합니다.
    /// Tools > TST > Wire Fishing UI References
    /// </summary>
    public static class WireFishingUIReferences
    {
        [MenuItem("Tools/TST/Wire Fishing UI References")]
        public static void Wire()
        {
            WireFishingHudUI();
            WireFocusMinigameUI();
            EditorUtility.SetDirty(GameObject.Find("FishingHudUI"));
            EditorUtility.SetDirty(GameObject.Find("FocusMinigameUI"));
            Debug.Log("[TST] Fishing UI 참조 와이어링 완료.");
        }

        // ── FishingHudUI ─────────────────────────────────────────────

        private static void WireFishingHudUI()
        {
            var go = GameObject.Find("FishingHudUI");
            if (go == null) { Debug.LogWarning("[TST] FishingHudUI 오브젝트를 찾을 수 없습니다."); return; }

            var hud = go.GetComponent<FishingHudUI>();
            if (hud == null) { Debug.LogWarning("[TST] FishingHudUI 컴포넌트가 없습니다."); return; }

            var so = new SerializedObject(hud);

            // HelmImage
            var helmGo = FindInChildren(go, "HelmImage");
            if (helmGo != null)
                so.FindProperty("helmImage").objectReferenceValue = helmGo.GetComponent<Image>();

            // DurabilityBar
            var durGo = FindInChildren(go, "DurabilityBar");
            if (durGo != null)
            {
                var img = durGo.GetComponent<Image>();
                so.FindProperty("durabilityBar").objectReferenceValue = img;

                // Image type = Filled, fillMethod = Horizontal
                img.type       = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                EditorUtility.SetDirty(img);
            }

            // SpeedBar
            var spdGo = FindInChildren(go, "SpeedBar");
            if (spdGo != null)
            {
                var img = spdGo.GetComponent<Image>();
                so.FindProperty("speedBar").objectReferenceValue = img;

                img.type       = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                EditorUtility.SetDirty(img);
            }

            so.ApplyModifiedProperties();
            Debug.Log("[TST] FishingHudUI 와이어링 완료.");
        }

        // ── FocusMinigameUI ──────────────────────────────────────────

        private static void WireFocusMinigameUI()
        {
            var go = GameObject.Find("FocusMinigameUI");
            if (go == null) { Debug.LogWarning("[TST] FocusMinigameUI 오브젝트를 찾을 수 없습니다."); return; }

            var ui = go.GetComponent<FocusMinigameUI>();
            if (ui == null) { Debug.LogWarning("[TST] FocusMinigameUI 컴포넌트가 없습니다."); return; }

            var so = new SerializedObject(ui);

            // arcPivot → ArcBar
            var arcGo = FindInChildren(go, "ArcBar");
            if (arcGo != null)
                so.FindProperty("arcPivot").objectReferenceValue = arcGo.GetComponent<RectTransform>();

            // greenZoneRect → GreenZone
            var greenGo = FindInChildren(go, "GreenZone");
            if (greenGo != null)
                so.FindProperty("greenZoneRect").objectReferenceValue = greenGo.GetComponent<RectTransform>();

            // pointerRect → Pointer
            var ptrGo = FindInChildren(go, "Pointer");
            if (ptrGo != null)
                so.FindProperty("pointerRect").objectReferenceValue = ptrGo.GetComponent<RectTransform>();

            // resultOverlay → ResultOverlay
            var resultGo = FindInChildren(go, "ResultOverlay");
            if (resultGo != null)
                so.FindProperty("resultOverlay").objectReferenceValue = resultGo.GetComponent<Image>();

            so.ApplyModifiedProperties();
            Debug.Log("[TST] FocusMinigameUI 와이어링 완료.");
        }

        // ── 유틸 ─────────────────────────────────────────────────────

        private static GameObject FindInChildren(GameObject root, string childName)
        {
            var t = FindDeep(root.transform, childName);
            if (t == null) Debug.LogWarning($"[TST] '{childName}' 를 {root.name} 하위에서 찾을 수 없습니다.");
            return t != null ? t.gameObject : null;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
