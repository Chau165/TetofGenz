"""Enable Blender MCP addon, start socket server, keep Blender alive (GUI or background)."""
import addon_utils
import bpy


def _enable_addon():
    try:
        addon_utils.enable("addon", default_set=True, persistent=True)
        print("[blender_mcp_boot] enabled addon")
        return True
    except Exception as exc:
        print("[blender_mcp_boot] enable failed", exc)
        return False


def _start_server():
    scene = bpy.context.scene
    if getattr(scene, "blendermcp_server_running", False):
        print("[blender_mcp_boot] already running on port", getattr(scene, "blendermcp_port", "?"))
        return True

    if not hasattr(bpy.ops, "blendermcp") or not hasattr(bpy.ops.blendermcp, "start_server"):
        print("[blender_mcp_boot] blendermcp operator missing")
        return False

    bpy.ops.blendermcp.start_server()
    running = getattr(scene, "blendermcp_server_running", False)
    port = getattr(scene, "blendermcp_port", "?")
    print("[blender_mcp_boot] running", running, "port", port)
    return running


def _keep_alive():
    # Keeps the main loop ticking so socket commands dispatch via bpy.app.timers.
    return 1.0


if not _enable_addon() or not _start_server():
    raise SystemExit(1)

bpy.app.timers.register(_keep_alive, persistent=True)
