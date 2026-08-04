#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Optionally auto-starts MCP for Unity when the package assembly is available.
/// Uses reflection so the project still compiles if the package failed to import.
/// </summary>
[InitializeOnLoad]
static class AutoStartMcpBridge
{
    const string HttpUrl = "http://127.0.0.1:8090";
    const string LocatorTypeName = "MCPForUnity.Editor.Services.MCPServiceLocator, MCPForUnity.Editor";
    const string HttpEndpointUtilityTypeName = "MCPForUnity.Editor.Helpers.HttpEndpointUtility, MCPForUnity.Editor";

    static AutoStartMcpBridge()
    {
        EditorApplication.delayCall += () => _ = TryStartMcpAsync();
    }

    static async Task TryStartMcpAsync()
    {
        try
        {
            Type locatorType = Type.GetType(LocatorTypeName);
            Type httpUtilityType = Type.GetType(HttpEndpointUtilityTypeName);
            if (locatorType == null || httpUtilityType == null)
            {
                return;
            }

            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);

            MethodInfo saveUrl = httpUtilityType.GetMethod("SaveLocalBaseUrl", BindingFlags.Public | BindingFlags.Static);
            saveUrl?.Invoke(null, new object[] { HttpUrl });

            object server = locatorType.GetProperty("Server", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            object bridge = locatorType.GetProperty("Bridge", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (server == null || bridge == null)
            {
                return;
            }

            MethodInfo isReachable = server.GetType().GetMethod("IsLocalHttpServerReachable");
            PropertyInfo isRunningProperty = bridge.GetType().GetProperty("IsRunning");
            MethodInfo startBridge = bridge.GetType().GetMethod("StartAsync");

            // Short retry only — long polling after every domain reload floods the console.
            for (int attempt = 0; attempt < 6; attempt++)
            {
                if (isRunningProperty?.GetValue(bridge) is bool isRunning && isRunning)
                {
                    return;
                }

                if (isReachable != null && isReachable.Invoke(server, null) is bool serverUp && serverUp
                    && startBridge != null)
                {
                    object task = startBridge.Invoke(bridge, null);
                    if (task is Task asyncTask)
                    {
                        await asyncTask;
                        return;
                    }
                }

                await Task.Delay(400);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AutoStartMcpBridge] " + ex.Message);
        }
    }
}
#endif
