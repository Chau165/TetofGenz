"""
Generate seamless HCMC-style terrazzo sidewalk tiles.
Light gray square precast tiles with subtle embossed radial fan pattern,
thin grout grooves, per-tile color variation, and mild grime in grooves.
Run: python GenerateAlleySidewalkTiles.py
"""
import math
import os
import random

from PIL import Image, ImageDraw

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
TILE_PX = 72
TILES = 8
SIZE = TILE_PX * TILES
TILE_REAL_M = 0.45
SHEET_REAL_M = TILE_REAL_M * TILES

# Light gray terrazzo palette (warm-neutral, not cold European granite)
BASE_A = (0xC4, 0xC6, 0xC8)
BASE_B = (0xBC, 0xBE, 0xC0)
BASE_C = (0xCC, 0xCE, 0xD0)
BASE_D = (0xC0, 0xC2, 0xC4)
GROUT = (0x8E, 0x90, 0x92)
GROUT_DIRT = (0x7A, 0x7C, 0x7E)
RADIAL_LIGHT = (0xD0, 0xD2, 0xD4)
RADIAL_SHADOW = (0xAE, 0xB0, 0xB2)
WEAR = (0xB4, 0xB6, 0xB8)

random.seed(42026)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def tile_base(tx, ty):
    n = (tx * 17 + ty * 31 + (tx * ty) % 7) % 4
    return (BASE_A, BASE_B, BASE_C, BASE_D)[n]


def draw_radial_fan(draw, x0, y0, x1, y1, base):
    cx = (x0 + x1) * 0.5
    cy = (y0 + y1) * 0.5
    radius = min(x1 - x0, y1 - y0) * 0.42
    rays = 12
    for i in range(rays):
        angle = (i / rays) * math.pi * 2.0 + random.uniform(-0.02, 0.02)
        ex = cx + math.cos(angle) * radius
        ey = cy + math.sin(angle) * radius
        col = lerp(base, RADIAL_SHADOW, 0.10 + (i % 3) * 0.02)
        draw.line((cx, cy, ex, ey), fill=col, width=1)

    rings = 4
    for r in range(1, rings + 1):
        rr = radius * (r / (rings + 0.5))
        bbox = (cx - rr, cy - rr, cx + rr, cy + rr)
        ring_col = lerp(base, RADIAL_LIGHT if r % 2 == 0 else RADIAL_SHADOW, 0.06 + r * 0.015)
        draw.arc(bbox, 0, 360, fill=ring_col, width=1)


def draw_tile(draw, tx, ty):
    x0, y0 = tx * TILE_PX, ty * TILE_PX
    x1, y1 = x0 + TILE_PX - 1, y0 + TILE_PX - 1
    base = tile_base(tx, ty)
    draw.rectangle((x0, y0, x1, y1), fill=base)

    inset = 4
    inner = (x0 + inset, y0 + inset, x1 - inset, y1 - inset)
    draw_radial_fan(draw, inner[0], inner[1], inner[2], inner[3], base)

    # Thin grout grooves between tiles
    draw.rectangle((x0, y0, x1, y1), outline=GROUT, width=2)
    for gx in range(x0, x1 + 1):
        if gx <= x0 + 1 or gx >= x1 - 1:
            draw.line((gx, y0, gx, y1), fill=GROUT_DIRT, width=1)
    for gy in range(y0, y1 + 1):
        if gy <= y0 + 1 or gy >= y1 - 1:
            draw.line((x0, gy, x1, gy), fill=GROUT_DIRT, width=1)


def add_grime_in_grooves(img):
    px = img.load()
    for y in range(SIZE):
        for x in range(SIZE):
            lx = x % TILE_PX
            ly = y % TILE_PX
            edge = min(lx, ly, TILE_PX - 1 - lx, TILE_PX - 1 - ly)
            if edge > 2:
                continue
            r, g, b = px[x, y]
            t = (3 - edge) / 3.0
            px[x, y] = lerp((r, g, b), GROUT_DIRT, 0.25 + t * 0.35)


def add_surface_noise(img):
    px = img.load()
    for _ in range(SIZE * SIZE // 90):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        lx = x % TILE_PX
        ly = y % TILE_PX
        edge = min(lx, ly, TILE_PX - 1 - lx, TILE_PX - 1 - ly)
        if edge < 3:
            continue
        r, g, b = px[x, y]
        delta = random.uniform(-0.04, 0.04)
        px[x, y] = (
            max(0, min(255, int(r * (1 - abs(delta)) + WEAR[0] * abs(delta)))),
            max(0, min(255, int(g * (1 - abs(delta)) + WEAR[1] * abs(delta)))),
            max(0, min(255, int(b * (1 - abs(delta)) + WEAR[2] * abs(delta)))),
        )


def build_albedo():
    img = Image.new("RGB", (SIZE, SIZE), BASE_A)
    draw = ImageDraw.Draw(img)
    for ty in range(TILES):
        for tx in range(TILES):
            draw_tile(draw, tx, ty)
    add_grime_in_grooves(img)
    add_surface_noise(img)
    return img


def build_height_map():
    h = Image.new("L", (SIZE, SIZE), 210)
    px = h.load()
    for y in range(SIZE):
        for x in range(SIZE):
            lx = x % TILE_PX
            ly = y % TILE_PX
            edge = min(lx, ly, TILE_PX - 1 - lx, TILE_PX - 1 - ly)
            if edge < 2:
                px[x, y] = 95
            elif edge < 4:
                px[x, y] = 130
            else:
                cx = lx - TILE_PX * 0.5
                cy = ly - TILE_PX * 0.5
                dist = math.sqrt(cx * cx + cy * cy)
                ring = int(dist * 0.55) % 5
                px[x, y] = 198 + ring * 3 + ((x * 9 + y * 13) % 4)
    return h


def height_to_normal(height_img, strength=1.6):
    hpx = height_img.load()
    w, ht = height_img.size
    normal = Image.new("RGB", (w, ht))
    npx = normal.load()

    def h_at(x, y):
        x = max(0, min(w - 1, x))
        y = max(0, min(ht - 1, y))
        return hpx[x, y] / 255.0

    for y in range(ht):
        for x in range(w):
            dx = (h_at(x + 1, y) - h_at(x - 1, y)) * strength
            dy = (h_at(x, y + 1) - h_at(x, y - 1)) * strength
            nx, ny, nz = -dx, -dy, 1.0
            length = math.sqrt(nx * nx + ny * ny + nz * nz)
            npx[x, y] = (
                int((nx / length * 0.5 + 0.5) * 255),
                int((ny / length * 0.5 + 0.5) * 255),
                int((nz / length * 0.5 + 0.5) * 255),
            )
    return normal


def main():
    os.makedirs(SCRIPT_DIR, exist_ok=True)
    albedo = build_albedo()
    albedo_path = os.path.join(SCRIPT_DIR, "Alley_Sidewalk_Albedo.png")
    albedo.save(albedo_path, optimize=True)

    height = build_height_map()
    normal = height_to_normal(height)
    normal_path = os.path.join(SCRIPT_DIR, "Alley_Sidewalk_Normal.png")
    normal.save(normal_path, optimize=True)

    print("Saved:", albedo_path)
    print("Saved:", normal_path)
    print(f"Sheet {SHEET_REAL_M}m x {SHEET_REAL_M}m ({TILES}x{TILES} @ {TILE_REAL_M}m)")


if __name__ == "__main__":
    main()
