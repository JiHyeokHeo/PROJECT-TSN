using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class SetMaterialColorTool : IUnityEditorTool
    {
        public string Name => "set_material_color";
        public string Description => "Changes a color property on a material asset. Default property is _BaseColor (URP).";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"materialPath\":{\"type\":\"string\",\"description\":\"Asset path to material\"},\"color\":{\"type\":\"object\",\"description\":\"{r,g,b,a} values 0-1\"},\"propertyName\":{\"type\":\"string\",\"description\":\"Shader property name (default: _BaseColor)\"}},\"required\":[\"materialPath\",\"color\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null || !args.TryGetValue("materialPath", out var pathVal) || !args.TryGetValue("color", out var colorVal))
                    return JsonConvert.SerializeObject(new { error = "materialPath and color are required" });

                var mat = AssetDatabase.LoadAssetAtPath<Material>(pathVal.ToString());
                if (mat == null) return JsonConvert.SerializeObject(new { error = $"Material not found: {pathVal}" });

                string propName = args.TryGetValue("propertyName", out var pnv) ? pnv.ToString() : "_BaseColor";

                var cd = JsonConvert.DeserializeObject<Dictionary<string, float>>(colorVal.ToString());
                if (cd == null) return JsonConvert.SerializeObject(new { error = "Invalid color format" });

                float r = cd.TryGetValue("r", out var cr) ? cr : 1f;
                float g = cd.TryGetValue("g", out var cg) ? cg : 1f;
                float b = cd.TryGetValue("b", out var cb) ? cb : 1f;
                float a = cd.TryGetValue("a", out var ca) ? ca : 1f;

                Undo.RecordObject(mat, "Set Material Color");
                mat.SetColor(propName, new Color(r, g, b, a));
                EditorUtility.SetDirty(mat);
                AssetDatabase.SaveAssetIfDirty(mat);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    materialName = mat.name,
                    propertyName = propName,
                    color = new { r, g, b, a }
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
