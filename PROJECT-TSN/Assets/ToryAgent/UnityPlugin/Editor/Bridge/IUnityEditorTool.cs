namespace ToryAgent.UnityPlugin.Editor
{
    public interface IUnityEditorTool
    {
        string Name { get; }
        string Description { get; }
        string InputSchemaJson { get; }

        string Execute(string argumentsJson);
    }
}