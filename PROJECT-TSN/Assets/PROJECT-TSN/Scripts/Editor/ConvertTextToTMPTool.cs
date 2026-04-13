using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TST.Editor
{
    public static class ConvertTextToTMPTool
    {
        [MenuItem("TST/Convert Popup_Inventory Text → TMP")]
        public static void ConvertInventoryPopupTexts()
        {
            var root = GameObject.Find("UI.Popup_Inventory");
            if (root == null)
            {
                Debug.LogError("[ConvertTextToTMPTool] UI.Popup_Inventory not found in scene.");
                return;
            }

            int count = 0;
            foreach (var legacyText in root.GetComponentsInChildren<Text>(true))
            {
                ConvertOne(legacyText);
                count++;
            }

            Debug.Log($"[ConvertTextToTMPTool] Converted {count} Text → TextMeshProUGUI.");
            EditorUtility.SetDirty(root);
        }

        [MenuItem("TST/Fix Popup_Inventory ScrollView Content")]
        public static void FixScrollContent()
        {
            var root = GameObject.Find("UI.Popup_Inventory");
            if (root == null)
            {
                Debug.LogError("[ConvertTextToTMPTool] UI.Popup_Inventory not found in scene.");
                return;
            }

            int count = 0;
            foreach (var vlg in root.GetComponentsInChildren<VerticalLayoutGroup>(true))
            {
                var go = vlg.gameObject;

                // childControlHeight 켜기
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;

                // ContentSizeFitter 없으면 추가
                var csf = go.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = go.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                EditorUtility.SetDirty(go);
                Debug.Log($"  Fixed Content: {go.name}");
                count++;
            }

            Debug.Log($"[ConvertTextToTMPTool] Fixed {count} Content objects.");
        }

        private static void ConvertOne(Text src)
        {
            GameObject go = src.gameObject;

            // 기존 값 저장
            string   content   = src.text;
            int      fontSize  = src.fontSize;
            Color    color     = src.color;
            TextAnchor alignment = src.alignment;
            bool     raycast   = src.raycastTarget;

            // Text 제거
            Object.DestroyImmediate(src);

            // TextMeshProUGUI 추가
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = content;
            tmp.fontSize      = fontSize;
            tmp.color         = color;
            tmp.raycastTarget = raycast;

            // alignment 변환
            switch (alignment)
            {
                case TextAnchor.UpperLeft:    tmp.alignment = TextAlignmentOptions.TopLeft;    break;
                case TextAnchor.UpperCenter:  tmp.alignment = TextAlignmentOptions.Top;        break;
                case TextAnchor.UpperRight:   tmp.alignment = TextAlignmentOptions.TopRight;   break;
                case TextAnchor.MiddleLeft:   tmp.alignment = TextAlignmentOptions.Left;       break;
                case TextAnchor.MiddleCenter: tmp.alignment = TextAlignmentOptions.Center;     break;
                case TextAnchor.MiddleRight:  tmp.alignment = TextAlignmentOptions.Right;      break;
                case TextAnchor.LowerLeft:    tmp.alignment = TextAlignmentOptions.BottomLeft; break;
                case TextAnchor.LowerCenter:  tmp.alignment = TextAlignmentOptions.Bottom;     break;
                case TextAnchor.LowerRight:   tmp.alignment = TextAlignmentOptions.BottomRight;break;
                default:                      tmp.alignment = TextAlignmentOptions.Center;     break;
            }

            Debug.Log($"  Converted: {go.name}");
        }
    }
}
