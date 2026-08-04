"""
Wall-mounted FPT shirt using Meshy polo GLB + logo decal + wall nail.
Run: blender --background --python CreateFptWallHanger.py
"""
import os
import bpy
import math
from mathutils import Vector

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))
OUTPUT_FBX = os.path.join(ROOT, "FPT_WallHanger.fbx")
MESHY_POLO = os.path.normpath(
    os.path.join(ROOT, "..", "..", "..", "Meshy_AI_Monochrome_Polo_0708155937_generate.glb")
)

TARGET_HEIGHT = 0.72   # metres, hanging shirt height
LOGO_FORWARD = 0.012


def vertex_bounds(obj):
    verts = [obj.matrix_world @ v.co for v in obj.data.vertices]
    return (
        min(v.x for v in verts), min(v.y for v in verts), min(v.z for v in verts),
        max(v.x for v in verts), max(v.y for v in verts), max(v.z for v in verts),
    )


def link(obj):
    bpy.context.collection.objects.link(obj)
    return obj


def active(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    return obj


def apply_all(obj):
    active(obj)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


# ---------------------------------------------------------------------------
# Import Meshy polo and prepare for wall hang
# ---------------------------------------------------------------------------

def rotate_mesh_vertices(obj, rx=0.0, ry=0.0, rz=0.0):
    """Bake rotation into mesh data (needed for symmetric flat Meshy garments)."""
    from mathutils import Matrix, Euler
    rot = Euler((rx, ry, rz), "XYZ").to_matrix().to_4x4()
    mesh = obj.data
    for v in mesh.vertices:
        v.co = rot @ v.co
    mesh.update()
    obj.rotation_euler = (0.0, 0.0, 0.0)


def build_shirt_from_meshy():
    if not os.path.isfile(MESHY_POLO):
        raise FileNotFoundError("Missing Meshy polo: " + MESHY_POLO)

    bpy.ops.import_scene.gltf(filepath=MESHY_POLO)

    meshes = [o for o in bpy.context.selected_objects if o.type == "MESH"]
    if not meshes:
        meshes = [o for o in bpy.context.scene.objects if o.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh in Meshy GLB")

    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        m.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    shirt = bpy.context.active_object
    shirt.name = "TShirtOnHanger"

    # Meshy polo imports flat (spread in XZ). Bake +90° X into vertices so it hangs
    # vertically in the XY plane with thin depth along Z (wall mount).
    rotate_mesh_vertices(shirt, rx=math.pi / 2, ry=math.pi)
    apply_all(shirt)

    minx, miny, minz, maxx, maxy, maxz = vertex_bounds(shirt)
    height = maxy - miny
    if height < 0.001:
        height = 1.0
    scale = TARGET_HEIGHT / height
    shirt.scale = (scale, scale, scale)
    apply_all(shirt)

    # Cleanup mesh.
    active(shirt)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.remove_doubles(threshold=0.0001)
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.shade_smooth()

    return shirt


# ---------------------------------------------------------------------------
# Logo decal — placed on viewer's RIGHT chest dynamically from bounds
# ---------------------------------------------------------------------------

def build_logo(shirt):
    minx, miny, minz, maxx, maxy, maxz = vertex_bounds(shirt)
    width = maxx - minx
    height = maxy - miny

    # Viewer right chest ≈ 78% from left edge of shirt bounds.
    logo_cx = minx + width * 0.78
    logo_cy = miny + height * 0.68
    logo_w = width * 0.28
    logo_h = logo_w * (140.0 / 360.0)   # fpt_uni.png aspect ratio

    hw, hh = logo_w * 0.5, logo_h * 0.5
    mesh = bpy.data.meshes.new("FPT_LogoDecalMesh")
    mesh.from_pydata(
        [(-hw, -hh, 0), (-hw, hh, 0), (hw, hh, 0), (hw, -hh, 0)],
        [], [(0, 1, 2, 3)],
    )
    mesh.update()

    uv_layer = mesh.uv_layers.new(name="UVMap")
    poly = mesh.polygons[0]
    uvs = [(1.0, 0.0), (1.0, 1.0), (0.0, 1.0), (0.0, 0.0)]
    for i, li in enumerate(poly.loop_indices):
        uv_layer.data[li].uv = uvs[i]

    logo = bpy.data.objects.new("FPT_LogoDecal", mesh)
    link(logo)
    logo.parent = shirt
    # Front face is -Z; place logo slightly in front of shirt.
    logo.location = (logo_cx, logo_cy, minz - LOGO_FORWARD)
    return logo


# ---------------------------------------------------------------------------
# Simple wire hanger + wall nail
# ---------------------------------------------------------------------------

def make_box(name, sx, sy, sz, loc):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = (sx, sy, sz)
    apply_all(obj)
    return obj


def build_hanger(shirt):
    minx, miny, minz, maxx, maxy, maxz = vertex_bounds(shirt)
    cx = (minx + maxx) / 2
    top_y = maxy
    fz = minz - 0.008

    parts = [
        make_box("HBar", 0.26, 0.007, 0.007, (cx, top_y + 0.008, fz)),
        make_box("HLeft", 0.007, 0.12, 0.007, (cx + 0.11, top_y - 0.05, fz)),
        make_box("HRight", 0.007, 0.12, 0.007, (cx - 0.11, top_y - 0.05, fz)),
        make_box("HHook", 0.007, 0.09, 0.007, (cx, top_y + 0.055, fz)),
    ]
    parts[1].rotation_euler = (0, 0, math.radians(22))
    parts[2].rotation_euler = (0, 0, math.radians(-22))
    for p in parts[1:]:
        apply_all(p)

    bpy.ops.object.select_all(action="DESELECT")
    for p in parts:
        p.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    hanger = bpy.context.active_object
    hanger.name = "HangerWire"
    hanger.parent = shirt
    hanger.matrix_parent_inverse = shirt.matrix_world.inverted()
    return hanger


def build_wall_nail(root, shirt):
    minx, miny, minz, maxx, maxy, maxz = vertex_bounds(shirt)
    cx = (minx + maxx) / 2
    hang = Vector((cx, maxy + 0.11, maxz))

    bpy.ops.mesh.primitive_cylinder_add(
        vertices=20, radius=0.018, depth=0.007,
        location=hang + Vector((0, 0.004, 0)),
    )
    plate = bpy.context.active_object
    plate.rotation_euler = (math.pi / 2, 0, 0)
    apply_all(plate)

    bpy.ops.mesh.primitive_cylinder_add(
        vertices=10, radius=0.004, depth=0.032,
        location=hang + Vector((0, -(0.032 * 0.5 - 0.004), 0)),
    )
    shaft = bpy.context.active_object
    shaft.rotation_euler = (math.pi / 2, 0, 0)
    apply_all(shaft)

    bpy.ops.mesh.primitive_cylinder_add(
        vertices=10, radius=0.005, depth=0.020,
        location=hang + Vector((0, -(0.032 - 0.005), 0.012)),
    )
    peg = bpy.context.active_object
    peg.rotation_euler = (0.7, 0, 0)
    apply_all(peg)

    bpy.ops.object.select_all(action="DESELECT")
    for p in (plate, shaft, peg):
        p.select_set(True)
    bpy.context.view_layer.objects.active = plate
    bpy.ops.object.join()
    nail = bpy.context.active_object
    nail.name = "WallNail"
    nail.parent = root
    nail.location = root.matrix_world.inverted() @ hang
    return nail


def position_shirt(shirt, root):
    minx, miny, minz, maxx, maxy, maxz = vertex_bounds(shirt)
    shirt.location = Vector((
        -(minx + maxx) / 2,
        -(miny + maxy) / 2,
        0.014 - maxz,
    ))
    bpy.context.view_layer.update()
    shirt.parent = root
    shirt.matrix_parent_inverse = root.matrix_world.inverted()


def export_fbx(root):
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        object_types={"EMPTY", "MESH"},
    )


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.object.empty_add(type="PLAIN_AXES")
    root = bpy.context.active_object
    root.name = "FPT_WallHanger"

    shirt = build_shirt_from_meshy()
    build_hanger(shirt)
    build_logo(shirt)
    position_shirt(shirt, root)
    build_wall_nail(root, shirt)
    export_fbx(root)
    print("Exported", OUTPUT_FBX, "from", MESHY_POLO)


if __name__ == "__main__":
    main()
