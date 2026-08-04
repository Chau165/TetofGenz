"""
Low-poly terracotta flower pot for Level 1 outdoor flower corner.
Run in Blender MCP (execute_blender_code) or headless:
  blender --background --python CreateOutdoorFlowerPot.py
"""
import math
import os
import bpy
import bmesh
from mathutils import Vector

OUTPUT_FBX = os.path.normpath(
    os.path.join(os.path.dirname(__file__), "..", "OutdoorFlowerPot.fbx")
)

# Rim top Y at scale 1 — keep in sync with Level1OutdoorAmbientPropsBuilder.BlenderPotRimHeight
POT_RIM_HEIGHT = 0.31


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def create_pot_mesh():
    mesh = bpy.data.meshes.new("OutdoorFlowerPotMesh")
    obj = bpy.data.objects.new("OutdoorFlowerPot", mesh)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj

    bm = bmesh.new()
    segments = 16
    body_h = 0.26
    top_r = 0.16
    bottom_r = 0.10
    lip_r = 0.175
    lip_drop = 0.032

    bottom_verts = []
    mid_verts = []
    top_verts = []
    lip_outer = []
    lip_inner = []

    for i in range(segments):
        t = (i / segments) * math.tau
        ca, sa = math.cos(t), math.sin(t)
        bottom_verts.append(bm.verts.new((bottom_r * ca, 0.0, bottom_r * sa)))
        mid_verts.append(bm.verts.new(((bottom_r * 0.55 + top_r * 0.45) * ca, body_h * 0.52, (bottom_r * 0.55 + top_r * 0.45) * sa)))
        top_verts.append(bm.verts.new((top_r * ca, body_h, top_r * sa)))
        lip_outer.append(bm.verts.new((lip_r * ca, body_h + lip_drop * 0.25, lip_r * sa)))
        lip_inner.append(bm.verts.new((top_r * 0.98 * ca, body_h + lip_drop, top_r * 0.98 * sa)))

    bm.verts.ensure_lookup_table()

    for i in range(segments):
        n = (i + 1) % segments
        bm.faces.new([bottom_verts[i], bottom_verts[n], mid_verts[n], mid_verts[i]])
        bm.faces.new([mid_verts[i], mid_verts[n], top_verts[n], top_verts[i]])
        bm.faces.new([top_verts[i], top_verts[n], lip_outer[n], lip_outer[i]])
        bm.faces.new([top_verts[i], lip_outer[i], lip_inner[i]])
        bm.faces.new([top_verts[i], lip_inner[i], lip_inner[n], top_verts[n]])

    bottom_center = bm.verts.new((0.0, 0.0, 0.0))
    for i in range(segments):
        n = (i + 1) % segments
        bm.faces.new([bottom_center, bottom_verts[n], bottom_verts[i]])

    bm.normal_update()
    bm.to_mesh(mesh)
    bm.free()

    # Z-up mesh -> Y-up for Unity export
    obj.rotation_euler[0] = math.pi / 2.0
    bpy.ops.object.transform_apply(rotation=True)

    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
    bbox = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    min_y = min(v.y for v in bbox)
    obj.location.y = -min_y
    bpy.ops.object.transform_apply(location=True)

    mat = bpy.data.materials.new(name="Terracotta")
    if hasattr(mat, "use_nodes"):
        mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF") if mat.node_tree else None
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (0.80, 0.43, 0.25, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.78
    obj.data.materials.append(mat)
    return obj


def export_fbx(obj):
    os.makedirs(os.path.dirname(OUTPUT_FBX), exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH"},
        axis_forward="-Z",
        axis_up="Y",
        global_scale=1.0,
        mesh_smooth_type="FACE",
    )
    print("Exported:", OUTPUT_FBX)


def main():
    clear_scene()
    pot = create_pot_mesh()
    export_fbx(pot)
    print("Flower pot rim height ~%.2fm" % POT_RIM_HEIGHT)


if __name__ == "__main__":
    main()
