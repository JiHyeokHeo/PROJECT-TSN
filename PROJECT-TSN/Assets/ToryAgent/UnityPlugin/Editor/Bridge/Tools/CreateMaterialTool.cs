using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ToryAgent.UnityPlugin.Editor
{
    public sealed class CreateMaterialTool : IUnityEditorTool
    {
        public string Name => "create_material";
        public string Description => "Creates a new URP material asset and saves it to the specified path.";
        public string InputSchemaJson =>
            "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"Material name\"},\"savePath\":{\"type\":\"string\",\"description\":\"Asset path e.g. Assets/Materials/MyMat.mat\"},\"shader\":{\"type\":\"string\",\"description\":\"Shader name (default: Universal Render Pipeline/Lit)\"},\"color\":{\"type\":\"object\",\"description\":\"{r,g,b,a} values 0-1\"},\"metallic\":{\"type\":\"number\",\"description\":\"0-1\"},\"smoothness\":{\"type\":\"number\",\"description\":\"0-1\"},\"emission\":{\"type\":\"object\",\"description\":\"{r,g,b} emission color\"}},\"required\":[\"name\",\"savePath\"],\"additionalProperties\":false}";

        public string Execute(string argumentsJson)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Dictionary<string, object>>(argumentsJson ?? "{}");
                if (args == null) return JsonConvert.SerializeObject(new { error = "Invalid arguments" });

                string matName = args.TryGetValue("name", out var nv) ? nv.ToString() : "NewMaterial";
                string savePath = args.TryGetValue("savePath", out var sv) ? sv.ToString() : $"Assets/Materials/{matName}.mat";
                string shaderName = args.TryGetValue("shader", out var shv) ? shv.ToString() : "Universal Render Pipeline/Lit";

                if (!savePath.EndsWith(".mat")) savePath += ".mat";

                var shader = Shader.Find(shaderName);
                if (shader == null)
                    return JsonConvert.SerializeObject(new { error = $"Shader not found: {shaderName}" });

                var mat = new Material(shader) { name = matName };

                if (args.TryGetValue("color", out var colorVal))
                {
                    var cd = JsonConvert.DeserializeObject<Dictionary<string, float>>(colorVal.ToString());
                    if (cd != null)
                    {
                        float r = cd.TryGetValue("r", out var cr) ? cr : 1f;
                        float g = cd.TryGetValue("g", out var cg) ? cg : 1f;
                        float b = cd.TryGetValue("b", out var cb) ? cb : 1f;
                        float a = cd.TryGetValue("a", out var ca) ? ca : 1f;
                        mat.SetColor("_BaseColor", new Color(r, g, b, a));
                    }
                }

                if (args.TryGetValue("metallic", out var metalVal))
                    mat.SetFloat("_Metallic", Convert.ToSingle(metalVal));

                if (args.TryGetValue("smoothness", out var smoothVal))
                    mat.SetFloat("_Smoothness", Convert.ToSingle(smoothVal));

                if (args.TryGetValue("emission", out var emVal))
                {
                    var ed = JsonConvert.DeserializeObject<Dictionary<string, float>>(emVal.ToString());
                    if (ed != null)
                    {
                        float r = ed.TryGetValue("r", out var er) ? er : 0f;
                        float g = ed.TryGetValue("g", out var eg) ? eg : 0f;
                        float b = ed.TryGetValue("b", out var eb) ? eb : 0f;
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", new Color(r, g, b));
                    }
                }

                string dir = (Path.GetDirectoryName(savePath) ?? "Assets").Replace('\\', '/');
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string[] parts = dir.Split('/');
                    string current = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = current + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(current, parts[i]);
                        current = next;
                    }
                }

                AssetDatabase.CreateAsset(mat, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    assetPath = savePath,
                    shaderUsed = shaderName
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}
