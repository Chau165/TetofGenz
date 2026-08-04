"""
Low-poly folded FPT polo shirt for Level 1 rental room bed prop.
Run via Blender MCP or: blender --background --python CreateFptBedShirt.py
"""
import math
import os
import bpy
import bmesh
from mathutils import Vector

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))
OUTPUT_FBX = os.path.join(ROOT, "FPT_BedShirt.fbx")
SHIRT_TEX = os.path.normpath(
    os.path.join(ROOT, "..", "..", "..", "Image", "Portrait", "FPT_Shirt.png")
)
LOGO_TEX = os.path.normpath(
    os.path.join(ROOT, "..", "..", "..", "Image", "Portrait", "FPT_Logo.png")
)

# Folded shirt footprint on bed (metres) — keep in sync with Level1WallArtBuilder.
SHIRT_WIDTH = 0.34
SHIRT_LENGTH = 0.42
SHIRT_THICKNESS = 0.012


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def load_image(path):
    if not os.path.isfile(path):
        return None
    return bpy.data.images.load(path, check_existing=True)


def make_material(name, color, image=None, alpha_blend=False):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()

    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Roughness"].default_value = 0.72
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])

    if image is not None:
        tex = nodes.new("ShaderNodeTexImage")
        tex.image = image
        links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
        if alpha_blend and image.channels == 4:
            mat.blend_method = "BLEND"
            links.new(tex.outputs["Alpha"], bsdf.inputs["Alpha"])
    else:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)

    return mat


def create_folded_body():
    mesh = bpy.data.meshes.new("FPT_ShirtBodyMesh")
    obj = bpy.data.objects.new("FPT_ShirtBody", mesh)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj

    bm = bmesh.new()
    cols, rows = 6, 5
    verts = []
    hw = SHIRT_WIDTH * 0.5
    hl = SHIRT_LENGTH * 0.5

    for r in range(rows + 1):
        row = []
        for c in range(cols + 1):
            u = c / cols
            v = r / rows
            x = (u - 0.5) * SHIRT_WIDTH
            z = (v - 0.5) * SHIRT_LENGTH
            fold = math.sin(v * math.pi) * 0.008 + math.sin(u * math.pi * 2.0) * 0.003
            row.append(bm.verts.new((x, fold, z)))
        verts.append(row)

    for r in range(rows):
        for c in range(cols):
            bm.faces.new([verts[r][c], verts[r + 1][c], verts[r + 1][c + 1], verts[r][c + 1]])

    bm.normal_update()
    bm.to_mesh(mesh)
    bm.free()

    solid = obj.modifiers.new("Solidify", "SOLIDIFY")
    solid.thickness = SHIRT_THICKNESS
    solid.offset = 1.0
    bpy.ops.object.modifier_apply(modifier=solid.name)
    return obj


def create_sleeve(name, x_sign):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x_sign * 0.19, 0.006, 0.04))
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (0.11, 0.009, 0.14)
    bpy.ops.object.transform_apply(scale=True)
    obj.rotation_euler[2] = math.radians(x_sign * 18.0)
    bpy.ops.object.transform_apply(rotation=True)
    return obj


def create_logo_patch(logo_img):
    bpy.ops.mesh.primitive_plane_add(size=1.0, location=(0.05, SHIRT_THICKNESS + 0.0015, 0.02))
    obj = bpy.context.active_object
    obj.name = "FPT_LogoPatch"
    obj.scale = (0.075, 0.045, 1.0)
    bpy.ops.object.transform_apply(scale=True)
    obj.rotation_euler[0] = math.pi / 2.0
    bpy.ops.object.transform_apply(rotation=True)

    if logo_img is not None:
        mat = make_material("FPT_LogoMat", (1, 1, 1), logo_img, alpha_blend=True)
        obj.data.materials.append(mat)
    return obj


def join_parts(parts):
    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def prepare_for_unity(obj):
    obj.rotation_euler[0] = math.pi / 2.0
    bpy.ops.object.transform_apply(rotation=True)
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
    bbox = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    min_y = min(v.y for v in bbox)
    obj.location.y = -min_y
    bpy.ops.object.transform_apply(location=True)


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
        path_mode="COPY",
        embed_textures=True,
    )
    print("Exported:", OUTPUT_FBX)


def main():
    clear_scene()
    shirt_img = load_image(SHIRT_TEX)
    logo_img = load_image(LOGO_TEX)

    body = create_folded_body()
    left = create_sleeve("FPT_SleeveLeft", -1)
    right = create_sleeve("FPT_SleeveRight", 1)

    shirt_mat = make_material("FPT_ShirtOrange", (0.92, 0.42, 0.08), shirt_img, alpha_blend=True)
    body.data.materials.append(shirt_mat)
    left.data.materials.append(shirt_mat)
    right.data.materials.append(shirt_mat)

    logo = create_logo_patch(logo_img)
    merged = join_parts([body, left, right, logo])
    merged.name = "FPT_BedShirt"
    prepare_for_unity(merged)
    export_fbx(merged)
    print("FPT bed shirt ready")


if __name__ == "__main__":
    main()
