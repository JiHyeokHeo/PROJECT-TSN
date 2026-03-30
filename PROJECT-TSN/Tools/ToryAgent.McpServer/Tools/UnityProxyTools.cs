using System.Text.Json.Nodes;
using ToryAgent.McpServer.Application;

namespace ToryAgent.McpServer.Tools;

// ── Scene Info ──────────────────────────────────────────────────────────────

public sealed class GetSceneInfoProxyTool : UnityBridgeProxyTool
{
    public GetSceneInfoProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "get_scene_info";
    public override string Description => "Returns information about the currently open Unity scene (name, path, isDirty, rootCount).";
    public override JsonObject InputSchema => new() { ["type"] = "object", ["properties"] = new JsonObject(), ["additionalProperties"] = false };
}

public sealed class GetSceneHierarchyProxyTool : UnityBridgeProxyTool
{
    public GetSceneHierarchyProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "get_scene_hierarchy";
    public override string Description => "Returns the full GameObject hierarchy of the current Unity scene as a tree.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["maxDepth"] = new JsonObject { ["type"] = "number", ["description"] = "Max depth to traverse (default: 10)" }
        }
    };
}

public sealed class SaveSceneProxyTool : UnityBridgeProxyTool
{
    public SaveSceneProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "save_scene";
    public override string Description => "Saves the currently active Unity scene to disk.";
    public override JsonObject InputSchema => new() { ["type"] = "object", ["properties"] = new JsonObject() };
}

// ── GameObject Query ─────────────────────────────────────────────────────────

public sealed class GetGameObjectDetailsProxyTool : UnityBridgeProxyTool
{
    public GetGameObjectDetailsProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "get_gameobject_details";
    public override string Description => "Returns detailed info about a GameObject (components, transform, tag, layer). Provide instanceId or path.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number", ["description"] = "GameObject InstanceID" },
            ["path"] = new JsonObject { ["type"] = "string", ["description"] = "Hierarchy path e.g. Root/Child/Target" }
        }
    };
}

public sealed class FindGameObjectsProxyTool : UnityBridgeProxyTool
{
    public FindGameObjectsProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "find_gameobjects";
    public override string Description => "Finds GameObjects in the scene by name or tag.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string", ["description"] = "Partial or full name to search" },
            ["tag"] = new JsonObject { ["type"] = "string", ["description"] = "Tag to filter by" },
            ["includeInactive"] = new JsonObject { ["type"] = "boolean", ["description"] = "Include inactive objects (default: true)" }
        }
    };
}

public sealed class GetComponentsProxyTool : UnityBridgeProxyTool
{
    public GetComponentsProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "get_components";
    public override string Description => "Returns all components on a GameObject with their property values.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number", ["description"] = "GameObject InstanceID" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

// ── GameObject Create / Modify ───────────────────────────────────────────────

public sealed class CreateGameObjectProxyTool : UnityBridgeProxyTool
{
    public CreateGameObjectProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_gameobject";
    public override string Description => "Creates a new empty GameObject in the scene. Returns instanceId.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string", ["description"] = "Name of the new GameObject" },
            ["parentInstanceId"] = new JsonObject { ["type"] = "number", ["description"] = "InstanceID of parent (optional)" },
            ["position"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y,z}" },
            ["rotation"] = new JsonObject { ["type"] = "object", ["description"] = "Euler angles {x,y,z}" },
            ["tag"] = new JsonObject { ["type"] = "string" },
            ["layer"] = new JsonObject { ["type"] = "number" }
        },
        ["required"] = new JsonArray("name")
    };
}

public sealed class CreatePrimitiveProxyTool : UnityBridgeProxyTool
{
    public CreatePrimitiveProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_primitive";
    public override string Description => "Creates a Unity primitive (Cube, Sphere, Capsule, Cylinder, Plane, Quad) in the scene.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["primitiveType"] = new JsonObject { ["type"] = "string", ["description"] = "Cube | Sphere | Capsule | Cylinder | Plane | Quad" },
            ["name"] = new JsonObject { ["type"] = "string" },
            ["position"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y,z}" },
            ["scale"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y,z}" },
            ["parentInstanceId"] = new JsonObject { ["type"] = "number" }
        },
        ["required"] = new JsonArray("primitiveType")
    };
}

public sealed class SetTransformProxyTool : UnityBridgeProxyTool
{
    public SetTransformProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_transform";
    public override string Description => "Sets position, rotation, and/or scale of a GameObject.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["position"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y,z}" },
            ["rotation"] = new JsonObject { ["type"] = "object", ["description"] = "Euler angles {x,y,z}" },
            ["scale"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y,z}" },
            ["useLocalSpace"] = new JsonObject { ["type"] = "boolean", ["description"] = "Use local space (default: true)" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

public sealed class DeleteGameObjectProxyTool : UnityBridgeProxyTool
{
    public DeleteGameObjectProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "delete_gameobject";
    public override string Description => "Deletes a GameObject from the scene (supports Undo).";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

public sealed class SetGameObjectActiveProxyTool : UnityBridgeProxyTool
{
    public SetGameObjectActiveProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_gameobject_active";
    public override string Description => "Sets the active state of a GameObject.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["active"] = new JsonObject { ["type"] = "boolean" }
        },
        ["required"] = new JsonArray("instanceId", "active")
    };
}

public sealed class SetGameObjectNameProxyTool : UnityBridgeProxyTool
{
    public SetGameObjectNameProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_gameobject_name";
    public override string Description => "Renames a GameObject.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["name"] = new JsonObject { ["type"] = "string" }
        },
        ["required"] = new JsonArray("instanceId", "name")
    };
}

public sealed class SetGameObjectParentProxyTool : UnityBridgeProxyTool
{
    public SetGameObjectParentProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_gameobject_parent";
    public override string Description => "Changes the parent of a GameObject. Set parentInstanceId to 0 to move to scene root.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["parentInstanceId"] = new JsonObject { ["type"] = "number", ["description"] = "0 to unparent to root" },
            ["worldPositionStays"] = new JsonObject { ["type"] = "boolean", ["description"] = "Keep world position (default: true)" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

public sealed class DuplicateGameObjectProxyTool : UnityBridgeProxyTool
{
    public DuplicateGameObjectProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "duplicate_gameobject";
    public override string Description => "Duplicates a GameObject in the scene.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

// ── Component ────────────────────────────────────────────────────────────────

public sealed class AddComponentProxyTool : UnityBridgeProxyTool
{
    public AddComponentProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "add_component";
    public override string Description => "Adds a component to a GameObject. Examples: Rigidbody, BoxCollider, Light, Camera, AudioSource.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["componentType"] = new JsonObject { ["type"] = "string", ["description"] = "Component type name e.g. Rigidbody" }
        },
        ["required"] = new JsonArray("instanceId", "componentType")
    };
}

public sealed class SetComponentPropertyProxyTool : UnityBridgeProxyTool
{
    public SetComponentPropertyProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_component_property";
    public override string Description => "Sets a property or field value on a component using reflection.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["componentType"] = new JsonObject { ["type"] = "string" },
            ["propertyName"] = new JsonObject { ["type"] = "string" },
            ["value"] = new JsonObject { ["description"] = "Value to set" }
        },
        ["required"] = new JsonArray("instanceId", "componentType", "propertyName", "value")
    };
}

// ── Material ─────────────────────────────────────────────────────────────────

public sealed class CreateMaterialProxyTool : UnityBridgeProxyTool
{
    public CreateMaterialProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_material";
    public override string Description => "Creates a new URP material asset and saves it to the project.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string" },
            ["savePath"] = new JsonObject { ["type"] = "string", ["description"] = "e.g. Assets/Materials/MyMat.mat" },
            ["shader"] = new JsonObject { ["type"] = "string", ["description"] = "Shader name (default: Universal Render Pipeline/Lit)" },
            ["color"] = new JsonObject { ["type"] = "object", ["description"] = "{r,g,b,a} values 0-1" },
            ["metallic"] = new JsonObject { ["type"] = "number" },
            ["smoothness"] = new JsonObject { ["type"] = "number" },
            ["emission"] = new JsonObject { ["type"] = "object", ["description"] = "{r,g,b} emission color" }
        },
        ["required"] = new JsonArray("name", "savePath")
    };
}

public sealed class AssignMaterialProxyTool : UnityBridgeProxyTool
{
    public AssignMaterialProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "assign_material";
    public override string Description => "Assigns a material asset to a Renderer on a GameObject.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["materialPath"] = new JsonObject { ["type"] = "string", ["description"] = "Asset path to material" },
            ["materialIndex"] = new JsonObject { ["type"] = "number", ["description"] = "Slot index (default: 0)" }
        },
        ["required"] = new JsonArray("instanceId", "materialPath")
    };
}

public sealed class SetMaterialColorProxyTool : UnityBridgeProxyTool
{
    public SetMaterialColorProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_material_color";
    public override string Description => "Changes a color property on a material asset (default property: _BaseColor).";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["materialPath"] = new JsonObject { ["type"] = "string" },
            ["color"] = new JsonObject { ["type"] = "object", ["description"] = "{r,g,b,a} values 0-1" },
            ["propertyName"] = new JsonObject { ["type"] = "string", ["description"] = "Shader property (default: _BaseColor)" }
        },
        ["required"] = new JsonArray("materialPath", "color")
    };
}

// ── UI ───────────────────────────────────────────────────────────────────────

public sealed class CreateCanvasProxyTool : UnityBridgeProxyTool
{
    public CreateCanvasProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_canvas";
    public override string Description => "Creates a Canvas GameObject with CanvasScaler and GraphicRaycaster.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject { ["type"] = "string" },
            ["renderMode"] = new JsonObject { ["type"] = "string", ["description"] = "ScreenSpaceOverlay | ScreenSpaceCamera | WorldSpace (default: ScreenSpaceOverlay)" }
        },
        ["additionalProperties"] = false
    };
}

public sealed class CreateUIElementProxyTool : UnityBridgeProxyTool
{
    public CreateUIElementProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_ui_element";
    public override string Description => "Creates a UI element (Panel, Button, Text, Image, InputField, Slider, Toggle, ScrollView) under a Canvas or UI parent.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["elementType"] = new JsonObject { ["type"] = "string", ["description"] = "Panel | Button | Text | Image | InputField | Slider | Toggle | ScrollView" },
            ["name"] = new JsonObject { ["type"] = "string" },
            ["parentInstanceId"] = new JsonObject { ["type"] = "number", ["description"] = "InstanceId of parent Canvas or UI object" },
            ["anchoredPosition"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" },
            ["sizeDelta"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" }
        },
        ["required"] = new JsonArray("elementType", "parentInstanceId")
    };
}

public sealed class SetRectTransformProxyTool : UnityBridgeProxyTool
{
    public SetRectTransformProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_rect_transform";
    public override string Description => "Sets RectTransform properties (anchoredPosition, sizeDelta, anchorMin, anchorMax, pivot) on a UI element.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["anchoredPosition"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" },
            ["sizeDelta"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" },
            ["anchorMin"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" },
            ["anchorMax"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" },
            ["pivot"] = new JsonObject { ["type"] = "object", ["description"] = "{x,y}" }
        },
        ["required"] = new JsonArray("instanceId")
    };
}

public sealed class SetUITextProxyTool : UnityBridgeProxyTool
{
    public SetUITextProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_ui_text";
    public override string Description => "Sets text content, fontSize, and color on a UI Text or TMP_Text component.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["text"] = new JsonObject { ["type"] = "string" },
            ["fontSize"] = new JsonObject { ["type"] = "number" },
            ["color"] = new JsonObject { ["type"] = "object", ["description"] = "{r,g,b,a}" }
        },
        ["required"] = new JsonArray("instanceId", "text")
    };
}

public sealed class SetUIColorProxyTool : UnityBridgeProxyTool
{
    public SetUIColorProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_ui_color";
    public override string Description => "Sets the color on a UI Graphic component (Image, Text, RawImage, etc.).";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number" },
            ["color"] = new JsonObject { ["type"] = "object", ["description"] = "{r,g,b,a} values 0-1" }
        },
        ["required"] = new JsonArray("instanceId", "color")
    };
}

// ── GameObject Tag / Serialized Field ────────────────────────────────────────

public sealed class SetGameObjectTagProxyTool : UnityBridgeProxyTool
{
    public SetGameObjectTagProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_gameobject_tag";
    public override string Description => "Sets the Tag on a GameObject. The tag must already exist in the project.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number", ["description"] = "GameObject InstanceID" },
            ["tag"] = new JsonObject { ["type"] = "string", ["description"] = "Tag string to assign (must exist in project)" }
        },
        ["required"] = new JsonArray("instanceId", "tag"),
        ["additionalProperties"] = false
    };
}

public sealed class SetSerializedFieldProxyTool : UnityBridgeProxyTool
{
    public SetSerializedFieldProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "set_serialized_field";
    public override string Description => "Sets an object reference on a SerializeField (including private fields) via SerializedObject.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["instanceId"] = new JsonObject { ["type"] = "number", ["description"] = "InstanceID of the target GameObject" },
            ["componentType"] = new JsonObject { ["type"] = "string", ["description"] = "Component type name e.g. MyScript" },
            ["fieldName"] = new JsonObject { ["type"] = "string", ["description"] = "Serialized field name" },
            ["targetInstanceId"] = new JsonObject { ["type"] = "number", ["description"] = "InstanceID of the object to assign as the reference" }
        },
        ["required"] = new JsonArray("instanceId", "componentType", "fieldName", "targetInstanceId"),
        ["additionalProperties"] = false
    };
}

// ── Assets ───────────────────────────────────────────────────────────────────

public sealed class ListAssetsProxyTool : UnityBridgeProxyTool
{
    public ListAssetsProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "list_assets";
    public override string Description => "Lists assets in a Unity project folder. Use filter like t:Material, t:Prefab, t:Scene.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["path"] = new JsonObject { ["type"] = "string", ["description"] = "Asset folder path e.g. Assets/Materials" },
            ["filter"] = new JsonObject { ["type"] = "string", ["description"] = "Search filter e.g. t:Material" },
            ["recursive"] = new JsonObject { ["type"] = "boolean", ["description"] = "Include subfolders (default: true)" }
        },
        ["required"] = new JsonArray("path")
    };
}

public sealed class CreateFolderProxyTool : UnityBridgeProxyTool
{
    public CreateFolderProxyTool(UnityBridgeClient c) : base(c) { }
    public override string Name => "create_folder";
    public override string Description => "Creates a new folder in the Unity Asset database.";
    public override JsonObject InputSchema => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["parentPath"] = new JsonObject { ["type"] = "string", ["description"] = "Parent folder e.g. Assets/Materials" },
            ["folderName"] = new JsonObject { ["type"] = "string" }
        },
        ["required"] = new JsonArray("parentPath", "folderName")
    };
}
