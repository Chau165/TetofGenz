"""
Low-poly open packing suitcase for Level 1 — student heading home for Tet.
Separate parts: bottom shell, hinged lid, hinge, handle, wheels, clothes inside.
Run: blender --background --python CreateOpenSuitcase.py
"""
import math
import os
import bpy
import bmesh
from mathutils import Matrix, Vector

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), ".."))
OUTPUT_FBX = os.path.join(ROOT, "OpenSuitcase.fbx")

# Student travel suitcase (metres). Opening faces -Z (toward player in Unity).
WIDTH = 0.62
DEPTH = 0.44
BASE_H = 0.165
WALL = 0.022
RIM_H = 0.014
LID_H = 0.050
LID_OPEN_DEG = 72.0
WHEEL_R = 0.032
WHEEL_W = 0.016
HINGE_R = 0.010


def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def make_material(name, color, roughness=0.55):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()
    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    return mat


def link_obj(name, mesh):
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj


def rotate_around_hinge(obj, hinge, angle_deg):
    rot = Matrix.Rotation(math.radians(angle_deg), 4, "X")
    obj.matrix_world = Matrix.Translation(hinge) @ rot @ Matrix.Translation(-hinge) @ obj.matrix_world


def apply_lid_open(lid_parts, hinge, angle_deg, initial_world):
    for i, part in enumerate(lid_parts):
        part.matrix_world = initial_world[i].copy()
    bpy.context.view_layer.update()
    for part in lid_parts:
        rotate_around_hinge(part, hinge, angle_deg)


def tune_lid_open_angle(lid_parts, hinge, preferred_deg=-72.0, floor_y=0.0):
    """Keep lid above floor — negative X opens toward player without clipping underground."""
    initial_world = [part.matrix_world.copy() for part in lid_parts]
    best = -abs(preferred_deg)
    for deg in range(int(abs(preferred_deg)), 15, -1):
        trial = -float(deg)
        apply_lid_open(lid_parts, hinge, trial, initial_world)
        bpy.context.view_layer.update()
        min_y = float("inf")
        for part in lid_parts:
            if part.type != "MESH":
                continue
            for corner in part.bound_box:
                min_y = min(min_y, (part.matrix_world @ Vector(corner)).y)
        if min_y >= floor_y - 0.001:
            best = trial
            break
    apply_lid_open(lid_parts, hinge, best, initial_world)
    print(f"Lid open angle: {best:.1f} deg, min lid Y after tune: ", end="")
    min_y = float("inf")
    for part in lid_parts:
        for corner in part.bound_box:
            min_y = min(min_y, (part.matrix_world @ Vector(corner)).y)
    print(f"{min_y:.4f}")
    return best


def shell_box(name, outer_w, outer_d, height, wall, mat_ext, mat_int=None):
    """Hollow tub open on top — outer walls + inner cavity + floor."""
    mesh = bpy.data.meshes.new(name + "Mesh")
    bm = bmesh.new()
    hw, hd = outer_w * 0.5, outer_d * 0.5
    ih = height - wall
    iw, id_ = outer_w - wall * 2.0, outer_d - wall * 2.0
    ihw, ihd = iw * 0.5, id_ * 0.5

    v = [
        bm.verts.new((-hw, 0.0, -hd)),
        bm.verts.new((hw, 0.0, -hd)),
        bm.verts.new((hw, 0.0, hd)),
        bm.verts.new((-hw, 0.0, hd)),
        bm.verts.new((-ihw, wall, -ihd)),
        bm.verts.new((ihw, wall, -ihd)),
        bm.verts.new((ihw, wall, ihd)),
        bm.verts.new((-ihw, wall, ihd)),
        bm.verts.new((-hw, height, -hd)),
        bm.verts.new((hw, height, -hd)),
        bm.verts.new((hw, height, hd)),
        bm.verts.new((-hw, height, hd)),
        bm.verts.new((-ihw, ih, -ihd)),
        bm.verts.new((ihw, ih, -ihd)),
        bm.verts.new((ihw, ih, ihd)),
        bm.verts.new((-ihw, ih, ihd)),
    ]
    bm.verts.ensure_lookup_table()

    def q(a, b, c, d):
        bm.faces.new((v[a], v[b], v[c], v[d]))

    q(0, 3, 2, 1)
    q(4, 5, 6, 7)
    q(0, 1, 9, 8)
    q(4, 8, 9, 5)
    q(2, 3, 11, 10)
    q(6, 10, 11, 7)
    q(3, 0, 8, 11)
    q(7, 11, 8, 4)
    q(1, 2, 10, 9)
    q(5, 9, 10, 6)

    bm.normal_update()
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = link_obj(name, mesh)
    if mat_ext:
        obj.data.materials.append(mat_ext)
    return obj


def rim_strip(name, outer_w, outer_d, y_base, height, mat):
    """Thin lip around top edge."""
    mesh = bpy.data.meshes.new(name + "Mesh")
    bm = bmesh.new()
    hw, hd = outer_w * 0.5, outer_d * 0.5
    iw, id_ = outer_w - WALL * 1.6, outer_d - WALL * 1.6
    ihw, ihd = iw * 0.5, id_ * 0.5
    y0, y1 = y_base, y_base + height

    def ring(y):
        return [
            bm.verts.new((-hw, y, -hd)),
            bm.verts.new((hw, y, -hd)),
            bm.verts.new((hw, y, hd)),
            bm.verts.new((-hw, y, hd)),
            bm.verts.new((-ihw, y, -ihd)),
            bm.verts.new((ihw, y, -ihd)),
            bm.verts.new((ihw, y, ihd)),
            bm.verts.new((-ihw, y, ihd)),
        ]

    b = ring(y0)
    t = ring(y1)
    bm.verts.ensure_lookup_table()

    def wall(a0, a1, b0, b1):
        bm.faces.new((b[a0], b[a1], t[a1], t[a0]))

    for a, b_ in ((0, 1), (1, 2), (2, 3), (3, 0)):
        wall(a, b_, 0, 1)
    bm.faces.new((b[4], b[5], b[6], b[7]))
    bm.faces.new((t[7], t[6], t[5], t[4]))

    bm.normal_update()
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = link_obj(name, mesh)
    if mat:
        obj.data.materials.append(mat)
    return obj


def lid_shell(name, outer_w, outer_d, shell_h, wall, y_bottom, mat_ext):
    """Lid tray open on bottom — built in closed pose on top of base."""
    mesh = bpy.data.meshes.new(name + "Mesh")
    bm = bmesh.new()
    hw, hd = outer_w * 0.5, outer_d * 0.5
    ihw = (outer_w - wall * 2.0) * 0.5
    ihd = (outer_d - wall * 2.0) * 0.5
    y0, y1 = y_bottom, y_bottom + shell_h
    yi = y1 - wall

    top = [
        bm.verts.new((-hw, y1, -hd)),
        bm.verts.new((hw, y1, -hd)),
        bm.verts.new((hw, y1, hd)),
        bm.verts.new((-hw, y1, hd)),
    ]
    top_in = [
        bm.verts.new((-ihw, yi, -ihd)),
        bm.verts.new((ihw, yi, -ihd)),
        bm.verts.new((ihw, yi, ihd)),
        bm.verts.new((-ihw, yi, ihd)),
    ]
    bot = [
        bm.verts.new((-hw, y0, -hd)),
        bm.verts.new((hw, y0, -hd)),
        bm.verts.new((hw, y0, hd)),
        bm.verts.new((-hw, y0, hd)),
    ]
    bm.verts.ensure_lookup_table()

    bm.faces.new(top)
    bm.faces.new(list(reversed(top_in)))

    sides = (
        (top[0], top[1], bot[1], bot[0]),
        (top[1], top[2], bot[2], bot[1]),
        (top[2], top[3], bot[3], bot[2]),
        (top[3], top[0], bot[0], bot[3]),
    )
    for face in sides:
        bm.faces.new(face)

    inner_sides = (
        (top_in[0], top_in[3], top_in[2], top_in[1]),
        (top_in[0], top_in[1], bot[1], bot[0]),
        (top_in[1], top_in[2], bot[2], bot[1]),
        (top_in[2], top_in[3], bot[3], bot[2]),
        (top_in[3], top_in[0], bot[0], bot[3]),
    )
    for face in inner_sides:
        try:
            bm.faces.new(face)
        except ValueError:
            pass

    bm.normal_update()
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = link_obj(name, mesh)
    if mat_ext:
        obj.data.materials.append(mat_ext)
    return obj


def lid_liner(name, outer_w, outer_d, shell_h, wall, y_bottom, mat_int):
    """Inner lining panel visible when lid is open."""
    mesh = bpy.data.meshes.new(name + "Mesh")
    bm = bmesh.new()
    ihw = (outer_w - wall * 2.4) * 0.5
    ihd = (outer_d - wall * 2.4) * 0.5
    y = y_bottom + wall * 1.2
    v = [
        bm.verts.new((-ihw, y, -ihd)),
        bm.verts.new((ihw, y, -ihd)),
        bm.verts.new((ihw, y, ihd)),
        bm.verts.new((-ihw, y, ihd)),
    ]
    bm.faces.new(v)
    bm.normal_update()
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = link_obj(name, mesh)
    if mat_int:
        obj.data.materials.append(mat_int)
    return obj


def create_hinge(accent_mat, hinge_y, hinge_z):
    parts = []
    xs = (-WIDTH * 0.22, 0.0, WIDTH * 0.22)
    for i, x in enumerate(xs):
        bpy.ops.mesh.primitive_cylinder_add(
            radius=HINGE_R,
            depth=WIDTH * 0.14,
            location=(x, hinge_y + HINGE_R, hinge_z),
            rotation=(0.0, math.radians(90.0), 0.0),
        )
        knuckle = bpy.context.active_object
        knuckle.name = f"Suitcase_Hinge_{i + 1}"
        if accent_mat:
            knuckle.data.materials.append(accent_mat)
        parts.append(knuckle)

    bpy.ops.mesh.primitive_cylinder_add(
        radius=HINGE_R * 0.72,
        depth=WIDTH - 0.06,
        location=(0.0, hinge_y + HINGE_R * 0.5, hinge_z + 0.008),
        rotation=(0.0, math.radians(90.0), 0.0),
    )
    bar = bpy.context.active_object
    bar.name = "Suitcase_Hinge_Bar"
    if accent_mat:
        bar.data.materials.append(accent_mat)
    parts.append(bar)
    return parts


def create_handle(accent_mat, front_z):
    """U-handle on front (-Z) face."""
    parts = []
    bpy.ops.mesh.primitive_torus_add(
        major_radius=0.058,
        minor_radius=0.008,
        location=(0.0, BASE_H * 0.62, front_z),
        rotation=(math.radians(90.0), 0.0, 0.0),
    )
    handle = bpy.context.active_object
    handle.name = "Suitcase_Handle"
    handle.scale = (1.0, 0.48, 1.0)
    bpy.ops.object.transform_apply(scale=True)
    if accent_mat:
        handle.data.materials.append(accent_mat)
    parts.append(handle)

    for sx in (-0.048, 0.048):
        bpy.ops.mesh.primitive_cylinder_add(
            radius=0.007,
            depth=0.028,
            location=(sx, BASE_H * 0.52, front_z + 0.012),
            rotation=(math.radians(18.0), 0.0, 0.0),
        )
        post = bpy.context.active_object
        post.name = f"Suitcase_HandlePost_{'L' if sx < 0 else 'R'}"
        if accent_mat:
            post.data.materials.append(accent_mat)
        parts.append(post)
    return parts


def create_wheels(accent_mat):
    wheels = []
    back_z = DEPTH * 0.5 - 0.04
    for i, x in enumerate((-WIDTH * 0.28, WIDTH * 0.28)):
        bpy.ops.mesh.primitive_cylinder_add(
            radius=WHEEL_R,
            depth=WHEEL_W,
            location=(x, WHEEL_R, back_z),
            rotation=(0.0, 0.0, math.radians(90.0)),
        )
        wheel = bpy.context.active_object
        wheel.name = f"Suitcase_Wheel_{i + 1}"
        if accent_mat:
            wheel.data.materials.append(accent_mat)
        wheels.append(wheel)
    return wheels


def folded_cloth(name, size, location, rotation_euler, mat):
    sx, sy, sz = size
    mesh = bpy.data.meshes.new(name + "Mesh")
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bm.to_mesh(mesh)
    bm.free()
    obj = link_obj(name, mesh)
    obj.scale = (sx * 0.5, sy * 0.5, sz * 0.5)
    obj.location = location
    obj.rotation_euler = rotation_euler
    if mat:
        obj.data.materials.append(mat)
    return obj


def towel_roll(name, location, mat):
    bpy.ops.mesh.primitive_cylinder_add(
        radius=0.038,
        depth=0.17,
        location=location,
        rotation=(0.0, 0.0, math.radians(90.0)),
    )
    obj = bpy.context.active_object
    obj.name = name
    if mat:
        obj.data.materials.append(mat)
    return obj


def create_clothes(cloth_mats):
    floor_y = WALL + 0.004
    inset_x = WIDTH * 0.5 - WALL - 0.06
    inset_z = DEPTH * 0.5 - WALL - 0.05

    shirt1 = folded_cloth(
        "Cloth_Shirt_1",
        (0.20, 0.032, 0.16),
        (-0.07, floor_y + 0.016, -0.02),
        (0.0, math.radians(8.0), 0.0),
        cloth_mats["shirt_blue"],
    )
    shirt2 = folded_cloth(
        "Cloth_Shirt_2",
        (0.18, 0.028, 0.15),
        (0.09, floor_y + 0.03, -0.04),
        (0.0, math.radians(-12.0), 0.0),
        cloth_mats["shirt_cream"],
    )
    pants = folded_cloth(
        "Cloth_Pants",
        (0.22, 0.038, 0.17),
        (-0.04, floor_y + 0.05, 0.06),
        (0.0, math.radians(5.0), 0.0),
        cloth_mats["pants"],
    )
    towel = towel_roll(
        "Cloth_Towel",
        (0.11, floor_y + 0.038, 0.08),
        cloth_mats["towel"],
    )
    return [shirt1, shirt2, pants, towel]


def pivot_to_floor_center(all_parts, anchor_parts):
    """Align using bottom shell / wheels only so open lid does not pull pivot below floor."""
    bpy.context.view_layer.update()
    min_y = float("inf")
    min_x = max_x = min_z = max_z = 0.0
    first = True
    for obj in anchor_parts:
        if obj is None or obj.type != "MESH":
            continue
        for corner in obj.bound_box:
            w = obj.matrix_world @ Vector(corner)
            min_y = min(min_y, w.y)
            if first:
                min_x = max_x = w.x
                min_z = max_z = w.z
                first = False
            else:
                min_x = min(min_x, w.x)
                max_x = max(max_x, w.x)
                min_z = min(min_z, w.z)
                max_z = max(max_z, w.z)

    if min_y == float("inf"):
        return
    cx = (min_x + max_x) * 0.5
    cz = (min_z + max_z) * 0.5
    offset = Vector((cx, min_y, cz))
    for obj in all_parts:
        if obj is not None:
            obj.location -= offset


def parent_to_root(objects):
    root = bpy.data.objects.new("OpenSuitcase", None)
    bpy.context.collection.objects.link(root)
    for obj in objects:
        if obj is None:
            continue
        world = obj.matrix_world.copy()
        obj.parent = root
        obj.matrix_world = world
    return root


def export_fbx(root):
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in root.children:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT_FBX,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"EMPTY", "MESH"},
        axis_forward="-Z",
        axis_up="Y",
        global_scale=1.0,
        mesh_smooth_type="FACE",
        path_mode="COPY",
        embed_textures=False,
    )
    print("Exported:", OUTPUT_FBX)


def main():
    clear_scene()

    body_mat = make_material("SuitcaseBody", (0.12, 0.18, 0.30), 0.46)
    interior_mat = make_material("SuitcaseInterior", (0.74, 0.71, 0.66), 0.84)
    accent_mat = make_material("SuitcaseAccent", (0.10, 0.10, 0.11), 0.32)
    cloth_mats = {
        "shirt_blue": make_material("ClothShirtBlue", (0.55, 0.68, 0.82), 0.88),
        "shirt_cream": make_material("ClothShirtCream", (0.92, 0.86, 0.74), 0.88),
        "pants": make_material("ClothPants", (0.35, 0.42, 0.52), 0.86),
        "towel": make_material("ClothTowel", (0.88, 0.62, 0.62), 0.90),
    }

    hinge_z = DEPTH * 0.5 - 0.01
    hinge_y = BASE_H
    front_z = -DEPTH * 0.5 + 0.01
    hinge = Vector((0.0, hinge_y, hinge_z))

    bottom = shell_box("Suitcase_BottomShell", WIDTH, DEPTH, BASE_H, WALL, body_mat)
    bottom_rim = rim_strip("Suitcase_BottomRim", WIDTH, DEPTH, BASE_H - RIM_H, RIM_H, body_mat)

    lid = lid_shell("Suitcase_LidShell", WIDTH - 0.02, DEPTH - 0.02, LID_H, WALL, BASE_H, body_mat)
    lid_rim = rim_strip("Suitcase_LidRim", WIDTH - 0.02, DEPTH - 0.02, BASE_H, RIM_H, body_mat)
    lid_liner_mat = lid_liner(
        "Suitcase_LidInterior", WIDTH - 0.02, DEPTH - 0.02, LID_H, WALL, BASE_H, interior_mat
    )
    lid_parts = [lid, lid_rim, lid_liner_mat]

    hinge_parts = create_hinge(accent_mat, hinge_y, hinge_z)
    handle_parts = create_handle(accent_mat, front_z)
    wheels = create_wheels(accent_mat)
    clothes = create_clothes(cloth_mats)

    anchor_parts = [bottom, bottom_rim, *wheels, *clothes]
    all_parts = [
        bottom, bottom_rim, lid, lid_rim, lid_liner_mat,
        *handle_parts, *hinge_parts, *wheels, *clothes,
    ]
    pivot_to_floor_center(all_parts, anchor_parts)

    bpy.context.view_layer.update()
    hinge_world = bottom.matrix_world @ Vector((0.0, BASE_H, hinge_z))
    hinge = Vector(hinge_world)
    tune_lid_open_angle(lid_parts, hinge, preferred_deg=-LID_OPEN_DEG, floor_y=0.0)
    root = parent_to_root(all_parts)
    export_fbx(root)
    print("Open suitcase with clothes ready.")


if __name__ == "__main__":
    main()
