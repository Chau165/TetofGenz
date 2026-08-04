"""Generate low-poly inventory icon renders for Tet Holiday Adventure Level 1.

Colors match packable item materials from Level1SceneBuilder / ChoTet materials.
"""
import math
import os

import bpy
from mathutils import Vector

OUTPUT_DIR = r"c:\Game\TetHolidayAdventure\Assets\Art\Level1\InventoryIcons"
RESOLUTION = 256
ITEM_IDS = ("phone", "wallet", "bus_ticket", "id_card", "water_bottle", "tet_gift")

# Unity ChoTet material _BaseColor values (Assets/Settings/ChoTet/Materials).
GLASS_BLUE = (0.353, 0.545, 0.659, 1.0)       # Smartphone, WaterBottle
COIN_GOLD = (0.91, 0.74, 0.24, 1.0)             # Wallet
PRODUCE_PURPLE = (0.49, 0.31, 0.63, 1.0)        # Clothes -> bus_ticket
HOUSE_CREAM = (0.93, 0.83, 0.70, 1.0)           # Documents -> id_card
PRODUCE_ORANGE = (0.88, 0.50, 0.18, 1.0)        # Snacks -> tet_gift
ACCENT_RED = (0.620, 0.325, 0.220, 1.0)         # card details / snack label
DARK_BROWN = (0.373, 0.298, 0.247, 1.0)         # phone frame, bottle cap


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for item in list(block):
            if item.users == 0:
                block.remove(item)


def setup_render():
    scene = bpy.context.scene
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except Exception:
        scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = RESOLUTION
    scene.render.resolution_y = RESOLUTION
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.film_transparent = True
    scene.view_settings.view_transform = "Standard"
    scene.view_settings.look = "None"


def create_icon_material(name, rgba, roughness=0.42, metallic=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()

    output = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Base Color"].default_value = rgba
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Specular IOR Level"].default_value = 0.22
    bsdf.inputs["Metallic"].default_value = metallic
    links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
    return mat


def assign_material(obj, mat):
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)


def add_cube(name, location, scale):
    bpy.ops.mesh.primitive_cube_add(location=location, scale=scale)
    obj = bpy.context.active_object
    obj.name = name
    return obj


def add_cylinder(name, location, radius, depth, vertices=20):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location
    )
    obj = bpy.context.active_object
    obj.name = name
    return obj


def frame_objects(objects):
    if not objects:
        return

    min_v = Vector((1e9, 1e9, 1e9))
    max_v = Vector((-1e9, -1e9, -1e9))
    for obj in objects:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            min_v.x = min(min_v.x, world.x)
            min_v.y = min(min_v.y, world.y)
            min_v.z = min(min_v.z, world.z)
            max_v.x = max(max_v.x, world.x)
            max_v.y = max(max_v.y, world.y)
            max_v.z = max(max_v.z, world.z)

    center = (min_v + max_v) * 0.5
    size = max_v - min_v
    max_dim = max(size.x, size.y, size.z, 0.001)

    for obj in objects:
        obj.location -= center

    scale = 1.6 / max_dim
    for obj in objects:
        obj.scale *= scale
        obj.location *= scale

    bpy.context.view_layer.update()


def setup_camera_and_lights():
    cam_data = bpy.data.cameras.new("IconCamera")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = 2.2
    cam = bpy.data.objects.new("IconCamera", cam_data)
    bpy.context.collection.objects.link(cam)
    cam.location = (2.4, -2.4, 1.8)
    cam.rotation_euler = (math.radians(62), 0.0, math.radians(45))
    bpy.context.scene.camera = cam

    key_data = bpy.data.lights.new("KeyLight", type="AREA")
    key_data.energy = 300
    key_data.size = 3.0
    key = bpy.data.objects.new("KeyLight", key_data)
    bpy.context.collection.objects.link(key)
    key.location = (2.0, -1.5, 3.0)
    key.rotation_euler = (math.radians(50), 0.0, math.radians(35))

    fill_data = bpy.data.lights.new("FillLight", type="AREA")
    fill_data.energy = 130
    fill_data.size = 2.5
    fill = bpy.data.objects.new("FillLight", fill_data)
    bpy.context.collection.objects.link(fill)
    fill.location = (-2.0, 1.5, 1.5)
    fill.rotation_euler = (math.radians(55), 0.0, math.radians(-145))


def build_phone():
    frame_mat = create_icon_material("PhoneFrame", DARK_BROWN, roughness=0.55)
    screen_mat = create_icon_material("PhoneScreen", GLASS_BLUE, roughness=0.25, metallic=0.05)
    body = add_cube("PhoneBody", (0, 0, 0), (0.42, 0.08, 0.78))
    screen = add_cube("PhoneScreen", (0, 0.09, 0.02), (0.34, 0.02, 0.62))
    assign_material(body, frame_mat)
    assign_material(screen, screen_mat)
    return [body, screen]


def build_wallet():
    mat = create_icon_material("WalletGold", COIN_GOLD, roughness=0.35, metallic=0.18)
    base = add_cube("WalletBase", (0, 0, 0), (0.72, 0.08, 0.48))
    flap = add_cube("WalletFlap", (0, 0.08, 0.18), (0.68, 0.03, 0.22))
    assign_material(base, mat)
    assign_material(flap, mat)
    flap.rotation_euler = (math.radians(18), 0, 0)
    return [base, flap]


def build_bus_ticket():
    # Clothes pile in scene uses Produce_Purple.
    mat = create_icon_material("ClothesPurple", PRODUCE_PURPLE, roughness=0.62)
    fold_a = add_cube("ClothesFoldA", (0, 0.02, 0.06), (0.72, 0.10, 0.36))
    fold_b = add_cube("ClothesFoldB", (0, 0.06, -0.04), (0.68, 0.08, 0.34))
    fold_c = add_cube("ClothesFoldC", (0, 0.10, 0.02), (0.64, 0.06, 0.30))
    assign_material(fold_a, mat)
    assign_material(fold_b, mat)
    assign_material(fold_c, mat)
    fold_b.rotation_euler = (math.radians(8), math.radians(12), 0)
    fold_c.rotation_euler = (math.radians(-6), math.radians(-10), 0)
    return [fold_a, fold_b, fold_c]


def build_id_card():
    card_mat = create_icon_material("IdCardCream", HOUSE_CREAM, roughness=0.58)
    detail_mat = create_icon_material("IdCardDetail", ACCENT_RED, roughness=0.5)
    card = add_cube("IdCard", (0, 0, 0), (0.78, 0.05, 0.5))
    stripe = add_cube("IdStripe", (-0.12, 0.06, 0), (0.18, 0.02, 0.42))
    photo = add_cube("IdPhoto", (0.22, 0.06, 0.08), (0.16, 0.02, 0.18))
    assign_material(card, card_mat)
    assign_material(stripe, detail_mat)
    assign_material(photo, detail_mat)
    return [card, stripe, photo]


def build_water_bottle():
    body_mat = create_icon_material("BottleBlue", GLASS_BLUE, roughness=0.28, metallic=0.04)
    cap_mat = create_icon_material("BottleCap", HOUSE_CREAM, roughness=0.45)
    body = add_cylinder("BottleBody", (0, 0, -0.05), 0.28, 1.0, vertices=18)
    neck = add_cylinder("BottleNeck", (0, 0, 0.58), 0.12, 0.22, vertices=16)
    cap = add_cylinder("BottleCap", (0, 0, 0.76), 0.14, 0.12, vertices=16)
    assign_material(body, body_mat)
    assign_material(neck, body_mat)
    assign_material(cap, cap_mat)
    return [body, neck, cap]


def build_tet_gift():
    # Snacks in scene use Produce_Orange (simple box on shelf).
    box_mat = create_icon_material("SnackOrange", PRODUCE_ORANGE, roughness=0.55)
    label_mat = create_icon_material("SnackLabel", HOUSE_CREAM, roughness=0.6)
    box = add_cube("SnackBox", (0, 0, 0), (0.62, 0.62, 0.62))
    label = add_cube("SnackLabel", (0, 0.32, 0.08), (0.34, 0.04, 0.22))
    assign_material(box, box_mat)
    assign_material(label, label_mat)
    return [box, label]


BUILDERS = {
    "phone": build_phone,
    "wallet": build_wallet,
    "bus_ticket": build_bus_ticket,
    "id_card": build_id_card,
    "water_bottle": build_water_bottle,
    "tet_gift": build_tet_gift,
}


def render_item(item_id):
    clear_scene()
    setup_render()
    objects = BUILDERS[item_id]()
    frame_objects(objects)
    setup_camera_and_lights()

    output_path = os.path.join(OUTPUT_DIR, item_id + ".png")
    bpy.context.scene.render.filepath = output_path
    bpy.ops.render.render(write_still=True)
    print("Rendered", output_path)


def main():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    for item_id in ITEM_IDS:
        render_item(item_id)
    print("Done. Generated", len(ITEM_IDS), "colored inventory icons.")


if __name__ == "__main__":
    main()
