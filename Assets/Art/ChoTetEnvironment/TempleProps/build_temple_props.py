"""Headless Blender script: low-poly Vietnamese temple interactable props."""
import math
import os
import bpy
from mathutils import Vector

OUT_DIR = os.path.dirname(os.path.abspath(__file__))


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in list(bpy.data.meshes):
        bpy.data.meshes.remove(block)
    for block in list(bpy.data.materials):
        bpy.data.materials.remove(block)
    for block in list(bpy.data.objects):
        bpy.data.objects.remove(block, do_unlink=True)


def make_mat(name, color):
    m = bpy.data.materials.new(name=name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.55
    return m


def assign(obj, material):
    if obj.data.materials:
        obj.data.materials[0] = material
    else:
        obj.data.materials.append(material)


def cube(name, loc, scale, material, parent=None):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    assign(obj, material)
    if parent is not None:
        obj.parent = parent
    return obj


def cylinder(name, loc, radius, depth, material, parent=None, vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=loc
    )
    obj = bpy.context.active_object
    obj.name = name
    assign(obj, material)
    if parent is not None:
        obj.parent = parent
    return obj


def join_children(root_name):
    root = bpy.data.objects.get(root_name)
    meshes = [o for o in bpy.data.objects if o.parent == root and o.type == "MESH"]
    if not meshes:
        return root
    bpy.ops.object.select_all(action="DESELECT")
    for m in meshes:
        m.parent = None
        m.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = root_name
    if root is not None and root.name in bpy.data.objects:
        bpy.data.objects.remove(root, do_unlink=True)
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
    joined.location = (0.0, 0.0, 0.0)
    min_z = min((joined.matrix_world @ Vector(corner)).z for corner in joined.bound_box)
    joined.location.z -= min_z
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return joined


def export_fbx(obj, filename):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = os.path.join(OUT_DIR, filename)
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="FACE",
    )
    print("Exported", path)


def make_mats():
    return {
        "wood": make_mat("WoodDark", (0.28, 0.16, 0.08)),
        "gold": make_mat("Gold", (0.85, 0.62, 0.18)),
        "red": make_mat("LacquerRed", (0.72, 0.12, 0.1)),
        "cream": make_mat("Cream", (0.92, 0.86, 0.72)),
        "bronze": make_mat("Bronze", (0.45, 0.28, 0.12)),
        "stick": make_mat("StickWood", (0.55, 0.38, 0.18)),
    }


def build_offering_altar():
    clear_scene()
    mats = make_mats()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "TempleOfferingAltar"
    cube("AltarLegs", (0, 0, 0.28), (1.9, 0.75, 0.56), mats["wood"], root)
    cube("AltarTop", (0, 0, 0.62), (2.3, 1.0, 0.12), mats["gold"], root)
    cube("AltarCloth", (0, 0, 0.7), (2.1, 0.85, 0.05), mats["red"], root)
    for i, x in enumerate((-0.7, 0.0, 0.7)):
        cube(f"Tray_{i}", (x, 0, 0.78), (0.45, 0.45, 0.06), mats["cream"], root)
        cylinder(f"Fruit_{i}", (x, 0, 0.95), 0.13, 0.24, mats["red"], root, 10)
    cube("AltarPlaque", (0, -0.45, 1.0), (0.95, 0.08, 0.3), mats["gold"], root)
    joined = join_children("TempleOfferingAltar")
    export_fbx(joined, "TempleOfferingAltar.fbx")


def build_incense_burner():
    clear_scene()
    mats = make_mats()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "TempleIncenseBurner"
    cylinder("BurnerPedestal", (0, 0, 0.22), 0.38, 0.44, mats["wood"], root, 12)
    cylinder("BurnerBowl", (0, 0, 0.58), 0.58, 0.3, mats["bronze"], root, 14)
    cylinder("BurnerRim", (0, 0, 0.76), 0.62, 0.08, mats["gold"], root, 14)
    cylinder("AshBed", (0, 0, 0.66), 0.44, 0.08, mats["cream"], root, 12)
    for i in range(6):
        ang = i * (math.pi * 2.0 / 6.0)
        x = math.cos(ang) * 0.14
        y = math.sin(ang) * 0.14
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=6, radius=0.02, depth=0.58, location=(x, y, 1.0)
        )
        s = bpy.context.active_object
        s.name = f"IncenseStick_{i}"
        assign(s, mats["stick"])
        s.parent = root
        s.rotation_euler[0] = 0.1
    joined = join_children("TempleIncenseBurner")
    export_fbx(joined, "TempleIncenseBurner.fbx")


def build_fortune_tube():
    clear_scene()
    mats = make_mats()
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    root = bpy.context.active_object
    root.name = "TempleFortuneTube"
    cube("FortuneStand", (0, 0, 0.28), (0.75, 0.75, 0.56), mats["wood"], root)
    cylinder("FortuneBarrel", (0, 0, 0.9), 0.3, 0.75, mats["red"], root, 14)
    cylinder("FortuneRim", (0, 0, 1.3), 0.32, 0.08, mats["gold"], root, 14)
    for i in range(9):
        a = i * 0.6
        x = math.cos(a) * 0.11
        y = math.sin(a) * 0.11
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=5, radius=0.018, depth=0.72, location=(x, y, 1.55)
        )
        s = bpy.context.active_object
        s.name = f"FortuneStick_{i}"
        assign(s, mats["stick"])
        s.parent = root
        s.rotation_euler[0] = 0.12 * ((i % 3) - 1)
        s.rotation_euler[1] = 0.08 * ((i % 2) * 2 - 1)
    joined = join_children("TempleFortuneTube")
    export_fbx(joined, "TempleFortuneTube.fbx")


def main():
    build_offering_altar()
    build_incense_burner()
    build_fortune_tube()
    print("DONE")


if __name__ == "__main__":
    main()
