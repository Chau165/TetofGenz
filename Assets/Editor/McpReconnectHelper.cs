#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;

/// <summary>Forces the AI Game Developer MCP bridge to reconnect after domain reload.</summary>
[InitializeOnLoad]
static class McpReconnectHelper
{
    const string MenuPath = "Tools/Cho Tet/Reconnect MCP";
    const string PluginTypeName = "com.IvanMurzak.Unity.MCP.Editor.UnityMcpPluginEditor, com.IvanMurzak.Unity.MCP.Editor";

    static McpReconnectHelper()
    {
        EditorApplication.delayCall += () => _ = ReconnectAsync(silent: true);
    }

    public static void ReconnectFromMenu()
    {
        _ = ReconnectAsync(silent: false);
    }

    static async Task ReconnectAsync(bool silent)
    {
        Type pluginType = Type.GetType(PluginTypeName);
        if (pluginType == null)
        {
            if (!silent)
            {
                EditorUtility.DisplayDialog(
                    "MCP Reconnect",
                    "MCP package is not loaded in this Unity project.",
                    "OK");
            }

            return;
        }

        PropertyInfo keepConnected = pluginType.GetProperty("KeepConnected", BindingFlags.Public | BindingFlags.Static);
        keepConnected?.SetValue(null, true);

        PropertyInfo instanceProperty = pluginType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        object instance = instanceProperty?.GetValue(null);
        instance?.GetType().GetMethod("BuildMcpPluginIfNeeded", BindingFlags.Public | BindingFlags.Instance)
            ?.Invoke(instance, null);

        MethodInfo connect = pluginType.GetMethod("ConnectIfNeeded", BindingFlags.Public | BindingFlags.Static);
        object connectTask = connect?.Invoke(null, null);
        bool connected = false;
        if (connectTask is Task<bool> boolTask)
        {
            connected = await boolTask;
        }
        else if (connectTask is Task task)
        {
            await task;
            connected = true;
        }

        if (!silent)
        {
            EditorUtility.DisplayDialog(
                "MCP Reconnect",
                connected
                    ? "MCP connected on http://localhost:28711"
                    : "MCP reconnect attempted. Open Window > AI Game Developer — MCP if still disconnected.",
                "OK");
        }
    }
}
#endif
