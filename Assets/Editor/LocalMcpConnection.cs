using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

// Keep this project's local MCP bridge available after opening or recompiling.
[InitializeOnLoad]
internal static class LocalMcpConnection
{
    static LocalMcpConnection() => EditorApplication.delayCall += Connect;

    [MenuItem("Tools/Local MCP/Reconnect Unity")]
    private static async void Connect()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Connect;
            return;
        }

        try
        {
            EditorConfigurationCache.Instance.PinStdioForSession();
            if (!MCPServiceLocator.Bridge.IsRunning)
                await MCPServiceLocator.Bridge.StartAsync();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Local MCP connection: " + exception.GetBaseException().Message);
        }
    }
}
