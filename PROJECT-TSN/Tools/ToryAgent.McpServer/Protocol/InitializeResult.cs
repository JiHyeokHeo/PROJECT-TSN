namespace ToryAgent.McpServer.Protocol;

public sealed class InitializeResult
{
    public string ProtocolVersion { get; set; } = "2025-03-26";
    public ServerCapabilities Capabilities { get; set; } = new();
    public ServerIdentity ServerInfo { get; set; } = new();
}

public sealed class ServerCapabilities
{
    public ToolCapability Tools { get; set; } = new();
}

public sealed class ToolCapability
{
    public bool ListChanged { get; set; } = false;
}

public sealed class ServerIdentity
{
    public string Name { get; set; } = "ToryAgent";
    public string Version { get; set; } = "0.1.0";
}