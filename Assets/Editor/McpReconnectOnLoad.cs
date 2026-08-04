#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace ChoTet.Editor
{
    /// <summary>One-shot helper: reconnect Unity Editor to the local MCP server after domain reload.</summary>
    [InitializeOnLoad]
    internal static class McpReconnectOnLoad
    {
        static McpReconnectOnLoad()
        {
            EditorApplication.delayCall += TryConnect;
        }

        private static void TryConnect()
        {
            Type pluginType = Type.GetType(
                "com.IvanMurzak.Unity.MCP.Editor.UnityMcpPluginEditor, com.IvanMurzak.Unity.MCP.Editor");
            if (pluginType == null)
            {
                return;
            }

            MethodInfo connect = pluginType.GetMethod("ConnectIfNeeded", BindingFlags.Public | BindingFlags.Static);
            connect?.Invoke(null, null);
        }
    }
}
#endif
