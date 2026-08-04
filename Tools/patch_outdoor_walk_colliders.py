#!/usr/bin/env python3
"""Patch Level1_PackingRoom.unity with Level1_OutdoorWalkColliders (no Unity Editor required)."""

from __future__ import annotations

import re
from pathlib import Path

SCENE = Path(__file__).resolve().parents[1] / "Assets/Scenes/Level1_PackingRoom.unity"
OUTDOOR_TRANSFORM_ID = 1641976927

PLAY_MIN_X = -12.8
PLAY_MAX_X = 12.8
FLOOR_START_Z = -3.85
PLAY_END_Z = -27.4
ALLEY_ROAD_HALF = 2.38
ALLEY_CURB = 0.16
ALLEY_CURB_HEIGHT = 0.12
WALK_FLOOR_THICKNESS = 0.55
WALK_SURFACE_Y = 0.02
WALL_HEIGHT = 2.65
CURB_OVERLAP = 0.12
BUS_STOP_APPROACH_MAX_X = 1.15

WEST_INNER = -(ALLEY_ROAD_HALF + ALLEY_CURB)
EAST_INNER = ALLEY_ROAD_HALF + ALLEY_CURB
FLOOR_CENTER_Z = (FLOOR_START_Z + PLAY_END_Z) * 0.5
FLOOR_DEPTH_Z = PLAY_END_Z - FLOOR_START_Z + 0.8
SIDEWALK_FLOOR_Y = WALK_SURFACE_Y - WALK_FLOOR_THICKNESS * 0.5
ROAD_FLOOR_Y = (WALK_SURFACE_Y - ALLEY_CURB_HEIGHT) - WALK_FLOOR_THICKNESS * 0.5
CURB_STRIP_Y = (SIDEWALK_FLOOR_Y + ROAD_FLOOR_Y) * 0.5
CURB_STRIP_HEIGHT = ALLEY_CURB_HEIGHT + WALK_FLOOR_THICKNESS
CURB_STRIP_WIDTH = ALLEY_CURB + CURB_OVERLAP * 2

BUS_PARK_X = -2.35
BUS_PAD_HALF_X = 4.6
LANE_SOUTH_EDGE_Z = -28.8 + 4.24 + 0.14
SIDEWALK_NEAR_Z = LANE_SOUTH_EDGE_Z + 0.55 + 0.08
SIDEWALK_FAR_Z = SIDEWALK_NEAR_Z + 2.35
SIDEWALK_CENTER_Z = (SIDEWALK_NEAR_Z + SIDEWALK_FAR_Z) * 0.5

# Closed storefront only.
CLOSED_FACADES = [
    ("AmenityFacade_ConvenienceStoreFacade", 4.66, -8.81, 0.22, 4.2),
]

# Approximate open-shop aprons (name, centerX, centerZ, depthX, widthZ).
OPEN_APRONS = [
    ("OpenApron_PF_PawnShop", -5.2, -8.81, 3.0, 4.0),
    ("OpenApron_PF_TheGioiMobile_Store", -5.5, -16.4, 3.0, 4.0),
    ("OpenApron_PF_Vietnamese_CarWash", 7.4, -16.23, 3.2, 4.0),
    ("OpenApron_PF_ComTamShop", 8.5, -21.22, 3.2, 4.2),
    ("OpenApron_PF_Pharmacy", -6.8, -21.37, 3.0, 4.2),
]


def next_ids(text: str, count: int) -> list[int]:
    used = {int(m.group(1)) for m in re.finditer(r"&(\d+)", text) if int(m.group(1)) < 2_000_000_000}
    start = max(used) + 1
    return list(range(start, start + count))


def remove_walk_colliders_root(text: str) -> str:
    while True:
        root_match = re.search(
            r"--- !u!1 &(\d+)\nGameObject:.*?m_Name: Level1_OutdoorWalkColliders\n",
            text,
            re.DOTALL,
        )
        if not root_match:
            break

        root_go = root_match.group(1)
        tr_match = re.search(
            rf"--- !u!4 &(\d+)\nTransform:.*?m_GameObject: {{fileID: {root_go}}}",
            text,
            re.DOTALL,
        )
        if not tr_match:
            break

        root_tr = tr_match.group(1)
        text = text.replace(f"  - {{fileID: {root_tr}}}\n", "")

        child_ids = {int(m.group(1)) for m in re.finditer(r"  - \{fileID: (\d+)\}", tr_match.group(0))}
        remove_ids = child_ids | {int(root_go), int(root_tr)}

        for obj_id in sorted(remove_ids, reverse=True):
            block = re.search(
                rf"--- !u!\d+ &{obj_id}\n.*?(?=\n--- !u!|\Z)",
                text,
                re.DOTALL,
            )
            if block:
                text = text[: block.start()] + text[block.end() :]

    return text


def box_yaml(go_id: int, tr_id: int, col_id: int, mesh_id: int, name: str,
             pos: tuple[float, float, float], scale: tuple[float, float, float],
             parent_tr_id: int) -> str:
    x, y, z = pos
    sx, sy, sz = scale
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {mesh_id}}}
  - component: {{fileID: {col_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: {y}, z: {z}}}
  m_LocalScale: {{x: {sx}, y: {sy}, z: {sz}}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {parent_tr_id}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!65 &{col_id}
BoxCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {{x: 1, y: 1, z: 1}}
  m_Center: {{x: 0, y: 0, z: 0}}
--- !u!33 &{mesh_id}
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Mesh: {{fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}}
"""


def main() -> None:
    text = SCENE.read_text(encoding="utf-8")
    text = remove_walk_colliders_root(text)

    boxes: list[tuple[str, tuple[float, float, float], tuple[float, float, float]]] = []

    west_end_x = WEST_INNER + CURB_OVERLAP
    west_width = west_end_x - PLAY_MIN_X
    west_cx = (PLAY_MIN_X + west_end_x) * 0.5
    boxes.append(("WalkFloor_West", (west_cx, SIDEWALK_FLOOR_Y, FLOOR_CENTER_Z), (west_width, WALK_FLOOR_THICKNESS, FLOOR_DEPTH_Z)))

    east_start_x = EAST_INNER - CURB_OVERLAP
    east_width = PLAY_MAX_X - east_start_x
    east_cx = (east_start_x + PLAY_MAX_X) * 0.5
    boxes.append(("WalkFloor_East", (east_cx, SIDEWALK_FLOOR_Y, FLOOR_CENTER_Z), (east_width, WALK_FLOOR_THICKNESS, FLOOR_DEPTH_Z)))

    road_w = ALLEY_ROAD_HALF * 2
    boxes.append(("WalkFloor_AlleyCenter", (0, ROAD_FLOOR_Y, FLOOR_CENTER_Z), (road_w, WALK_FLOOR_THICKNESS, FLOOR_DEPTH_Z)))

    west_strip_cx = (WEST_INNER + (-ALLEY_ROAD_HALF)) * 0.5
    boxes.append(("CurbTransition_West", (west_strip_cx, CURB_STRIP_Y, FLOOR_CENTER_Z), (CURB_STRIP_WIDTH, CURB_STRIP_HEIGHT, FLOOR_DEPTH_Z)))

    east_strip_cx = (EAST_INNER + ALLEY_ROAD_HALF) * 0.5
    boxes.append(("CurbTransition_East", (east_strip_cx, CURB_STRIP_Y, FLOOR_CENTER_Z), (CURB_STRIP_WIDTH, CURB_STRIP_HEIGHT, FLOOR_DEPTH_Z)))

    approach_north_z = SIDEWALK_FAR_Z + 0.85
    approach_south_z = SIDEWALK_NEAR_Z - 0.85
    approach_depth = abs(approach_north_z - approach_south_z)
    approach_center_z = (approach_north_z + approach_south_z) * 0.5
    approach_width = BUS_STOP_APPROACH_MAX_X - PLAY_MIN_X
    approach_center_x = (PLAY_MIN_X + BUS_STOP_APPROACH_MAX_X) * 0.5
    boxes.append(("WalkFloor_BusStopApproach", (approach_center_x, SIDEWALK_FLOOR_Y, approach_center_z),
                  (approach_width, WALK_FLOOR_THICKNESS, approach_depth)))

    pad_w = BUS_PAD_HALF_X * 2 + 2.6
    pad_d = abs(SIDEWALK_NEAR_Z - SIDEWALK_FAR_Z) + 1.2
    pad_x = BUS_PARK_X - 3.0
    boxes.append(("WalkFloor_BusStopPad", (pad_x, SIDEWALK_FLOOR_Y, SIDEWALK_CENTER_Z), (pad_w, WALK_FLOOR_THICKNESS, pad_d)))

    facade_y = WALK_SURFACE_Y + WALL_HEIGHT * 0.5
    for name, fx, fz, depth, width_z in CLOSED_FACADES:
        boxes.append((name, (fx, facade_y, fz), (depth, WALL_HEIGHT, width_z)))

    for name, cx, cz, depth, width_z in OPEN_APRONS:
        boxes.append((name, (cx, SIDEWALK_FLOOR_Y, cz), (depth, WALK_FLOOR_THICKNESS, width_z)))

    child_count = len(boxes)
    ids = next_ids(text, 2 + child_count * 4)
    root_go, root_tr = ids[0], ids[1]
    child_ids = ids[2:]

    child_transform_ids: list[int] = []
    yaml_chunks: list[str] = []

    for i, (name, pos, scale) in enumerate(boxes):
        go_id, tr_id, col_id, mesh_id = child_ids[i * 4 : i * 4 + 4]
        child_transform_ids.append(tr_id)
        yaml_chunks.append(box_yaml(go_id, tr_id, col_id, mesh_id, name, pos, scale, root_tr))

    children_lines = "\n".join(f"  - {{fileID: {tid}}}" for tid in child_transform_ids)
    root_yaml = f"""--- !u!1 &{root_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {root_tr}}}
  m_Layer: 0
  m_Name: Level1_OutdoorWalkColliders
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{root_tr}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {root_go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{children_lines}
  m_Father: {{fileID: {OUTDOOR_TRANSFORM_ID}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""

    outdoor_children_anchor = "  m_Children:\n  - {fileID: 1537288966}"
    if f"  - {{fileID: {root_tr}}}\n" not in text:
        text = text.replace(
            outdoor_children_anchor,
            f"  m_Children:\n  - {{fileID: {root_tr}}}\n  - {{fileID: 1537288966}}",
            1,
        )

    insert_at = text.rfind("--- !u!1 &1642361859")
    if insert_at == -1:
        insert_at = len(text)
    payload = root_yaml + "".join(yaml_chunks)
    text = text[:insert_at] + payload + text[insert_at:]

    SCENE.write_text(text, encoding="utf-8")
    print(f"Patched {SCENE.name}: Level1_OutdoorWalkColliders with {child_count} colliders.")


if __name__ == "__main__":
    main()
