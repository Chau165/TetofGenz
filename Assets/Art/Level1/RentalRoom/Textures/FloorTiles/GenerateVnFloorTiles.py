"""
Generate seamless Vietnamese student-room ceramic floor tile textures.
Output: cream cement-tile albedo + subtle normal map (40 cm tiles, 8x8 per sheet = 3.2 m).
Run: python GenerateVnFloorTiles.py
"""
import math
import os
import random

from PIL import Image, ImageDraw

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = SCRIPT_DIR
TILE_PX = 64
TILES = 8
SIZE = TILE_PX * TILES
TILE_REAL_M = 0.40
SHEET_REAL_M = TILE_REAL_M * TILES

# Light cream / beige palette — low contrast, lived-in but clean.
BASE_A = (0xE4, 0xDC, 0xD0)
BASE_B = (0xDC, 0xD4, 0xC8)
BASE_C = (0xE8, 0xE0, 0xD4)
GROUT = (0xB8, 0xB0, 0xA4)
MOTIF = (0xC8, 0xBE, 0xAE)
MOTIF_SOFT = (0xD4, 0xCC, 0xC0)
WEAR = (0xC0, 0xB6, 0xAA)

random.seed(2018)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def tile_base_color(tx, ty):
    """Slight per-tile variation like aged factory batches."""
    n = (tx * 17 + ty * 31) % 5
    if n == 0:
        return BASE_B
    if n == 1:
        return BASE_C
    return BASE_A


def draw_grout(draw, tx, ty, inset=2):
    x0, y0 = tx * TILE_PX, ty * TILE_PX
    x1, y1 = x0 + TILE_PX - 1, y0 + TILE_PX - 1
    draw.rectangle((x0, y0, x1, y1), fill=tile_base_color(tx, ty))
    draw.rectangle((x0, y0, x1, y1), outline=GROUT, width=2)
    if inset > 0:
        draw.rectangle((x0 + inset, y0 + inset, x1 - inset, y1 - inset), outline=GROUT, width=1)


def draw_vintage_motif(draw, cx, cy, radius, strength=1.0):
    """Classic VN cement-tile motif: 4-petal flower + diamond frame."""
    petals = 4
    for i in range(petals):
        ang = math.radians(45 + i * 90)
        px = cx + math.cos(ang) * radius * 0.42
        py = cy + math.sin(ang) * radius * 0.42
        r = int(radius * 0.34)
        col = lerp(MOTIF_SOFT, MOTIF, strength)
        draw.ellipse((px - r, py - r, px + r, py + r), fill=col)

    diamond = [
        (cx, cy - radius * 0.78),
        (cx + radius * 0.78, cy),
        (cx, cy + radius * 0.78),
        (cx - radius * 0.78, cy),
    ]
    draw.polygon(diamond, outline=lerp(MOTIF_SOFT, MOTIF, strength * 0.85), width=1)

    inner = [
        (cx, cy - radius * 0.42),
        (cx + radius * 0.42, cy),
        (cx, cy + radius * 0.42),
        (cx - radius * 0.42, cy),
    ]
    draw.polygon(inner, outline=lerp(MOTIF_SOFT, MOTIF, strength * 0.55), width=1)

    draw.ellipse(
        (cx - radius * 0.14, cy - radius * 0.14, cx + radius * 0.14, cy + radius * 0.14),
        fill=lerp(MOTIF_SOFT, MOTIF, strength * 0.35),
    )


def draw_corner_squares(draw, tx, ty):
    """Small square accents in tile corners — common in local tile sets."""
    x0, y0 = tx * TILE_PX, ty * TILE_PX
    pad = 9
    sq = 5
    corners = (
        (x0 + pad, y0 + pad),
        (x0 + TILE_PX - pad - sq, y0 + pad),
        (x0 + pad, y0 + TILE_PX - pad - sq),
        (x0 + TILE_PX - pad - sq, y0 + TILE_PX - pad - sq),
    )
    for c in corners:
        if (tx + ty) % 2 == 0:
            draw.rectangle((c[0], c[1], c[0] + sq, c[1] + sq), fill=MOTIF_SOFT)


def add_wear(img):
    px = img.load()
    for _ in range(SIZE * SIZE // 90):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        r, g, b = px[x, y]
        blend = random.uniform(0.04, 0.12)
        wr, wg, wb = WEAR
        px[x, y] = (
            int(r * (1 - blend) + wr * blend),
            int(g * (1 - blend) + wg * blend),
            int(b * (1 - blend) + wb * blend),
        )


def build_albedo():
    img = Image.new("RGB", (SIZE, SIZE), BASE_A)
    draw = ImageDraw.Draw(img)

    for ty in range(TILES):
        for tx in range(TILES):
            draw_grout(draw, tx, ty)
            if (tx + ty) % 2 == 0:
                draw_corner_squares(draw, tx, ty)

    # 2x2 tile blocks share one centred motif (typical cement-tile layout).
    for by in range(0, TILES, 2):
        for bx in range(0, TILES, 2):
            cx = (bx + 1) * TILE_PX
            cy = (by + 1) * TILE_PX
            strength = 0.55 + ((bx // 2 + by // 2) % 3) * 0.12
            draw_vintage_motif(draw, cx, cy, TILE_PX * 0.92, strength)

    add_wear(img)
    return img


def build_height_map():
    """Grout recessed, tile surface slightly raised."""
    h = Image.new("L", (SIZE, SIZE), 200)
    px = h.load()
    for y in range(SIZE):
        for x in range(SIZE):
            lx = x % TILE_PX
            ly = y % TILE_PX
            edge = min(lx, ly, TILE_PX - 1 - lx, TILE_PX - 1 - ly)
            if edge < 3:
                px[x, y] = 118
            elif edge < 5:
                px[x, y] = 150
            else:
                px[x, y] = 205 + ((x * 13 + y * 7) % 5)
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
            nx = -dx
            ny = -dy
            nz = 1.0
            length = math.sqrt(nx * nx + ny * ny + nz * nz)
            nx /= length
            ny /= length
            nz /= length
            npx[x, y] = (
                int((nx * 0.5 + 0.5) * 255),
                int((ny * 0.5 + 0.5) * 255),
                int((nz * 0.5 + 0.5) * 255),
            )
    return normal


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    albedo = build_albedo()
    albedo_path = os.path.join(OUT_DIR, "Floor_TileVN_Albedo.png")
    albedo.save(albedo_path, optimize=True)

    height = build_height_map()
    normal = height_to_normal(height)
    normal_path = os.path.join(OUT_DIR, "Floor_TileVN_Normal.png")
    normal.save(normal_path, optimize=True)

    print("Saved:", albedo_path)
    print("Saved:", normal_path)
    print(f"Sheet covers {SHEET_REAL_M}m x {SHEET_REAL_M}m ({TILES}x{TILES} tiles @ {TILE_REAL_M}m)")


if __name__ == "__main__":
    main()
