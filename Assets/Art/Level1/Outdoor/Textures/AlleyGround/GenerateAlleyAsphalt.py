"""
Generate seamless worn Vietnamese residential-alley asphalt.
Medium-dark gray, rough aggregate, faded patches, subtle cracks, mild tire wear.
No lane markings, arrows, or crosswalks.
Run: python GenerateAlleyAsphalt.py
"""
import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SIZE = 1024
SHEET_REAL_M = 4.0

BASE = (0x42, 0x44, 0x46)
DARK = (0x34, 0x36, 0x38)
LIGHT_PATCH = (0x58, 0x5A, 0x5C)
PATCH_MID = (0x4C, 0x4E, 0x50)
AGGREGATE_LIGHT = (0x62, 0x64, 0x66)
AGGREGATE_DARK = (0x38, 0x3A, 0x3C)
GRIME = (0x2A, 0x2C, 0x2E)
CRACK = (0x2E, 0x30, 0x32)
TIRE_WEAR = (0x30, 0x32, 0x34)

random.seed(88081)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def noise_field(seed):
    rng = random.Random(seed)
    return [[rng.random() for _ in range(SIZE)] for _ in range(SIZE)]


def smooth_field(field, passes=2):
    w = len(field[0])
    h = len(field)
    for _ in range(passes):
        nxt = [[0.0 for _ in range(w)] for _ in range(h)]
        for y in range(h):
            for x in range(w):
                acc = 0.0
                for oy in (-1, 0, 1):
                    for ox in (-1, 0, 1):
                        acc += field[(y + oy) % h][(x + ox) % w]
                nxt[y][x] = acc / 9.0
        field = nxt
    return field


def draw_subtle_cracks(draw, px):
    for _ in range(14):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        length = random.randint(18, 70)
        angle = random.uniform(0, math.pi * 2)
        points = []
        for step in range(length):
            px_x = int(x + math.cos(angle) * step + random.uniform(-0.8, 0.8)) % SIZE
            px_y = int(y + math.sin(angle) * step + random.uniform(-0.8, 0.8)) % SIZE
            points.append((px_x, px_y))
            r, g, b = px[px_x, px_y]
            px[px_x, px_y] = lerp((r, g, b), CRACK, random.uniform(0.12, 0.28))
        if len(points) > 1:
            draw.line(points, fill=CRACK, width=1)


def add_tire_wear(px):
    # Mild longitudinal wear bands — subtle, no painted markings
    for y in range(SIZE):
        for x in range(SIZE):
            u = x / SIZE
            v = y / SIZE
            band = math.exp(-((u - 0.35) ** 2) / 0.018) + math.exp(-((u - 0.65) ** 2) / 0.018)
            band *= 0.5 + 0.5 * math.sin(v * math.pi * 6.0)
            if band < 0.15:
                continue
            r, g, b = px[x, y]
            px[x, y] = lerp((r, g, b), TIRE_WEAR, band * 0.12)


def build_albedo():
    img = Image.new("RGB", (SIZE, SIZE), BASE)
    px = img.load()

    low = smooth_field(noise_field(1), passes=3)
    mid = smooth_field(noise_field(2), passes=2)

    for y in range(SIZE):
        for x in range(SIZE):
            t = low[y][x]
            col = lerp(DARK, BASE, t * 0.82 + 0.10)
            if mid[y][x] > 0.74:
                col = lerp(col, LIGHT_PATCH, (mid[y][x] - 0.74) * 1.8)
            elif mid[y][x] < 0.26:
                col = lerp(GRIME, col, (0.26 - mid[y][x]) * 1.6)
            px[x, y] = col

    draw = ImageDraw.Draw(img)

    # Small repaired asphalt patches
    for _ in range(7):
        cx = random.randint(0, SIZE - 1)
        cy = random.randint(0, SIZE - 1)
        rx = random.randint(28, 90)
        ry = random.randint(22, 75)
        patch_col = lerp(PATCH_MID, LIGHT_PATCH, random.uniform(0.15, 0.65))
        draw.ellipse((cx - rx, cy - ry, cx + rx, cy + ry), fill=patch_col)

    img = img.filter(ImageFilter.GaussianBlur(radius=0.9))
    px = img.load()

    # Aggregate speckle
    for _ in range(SIZE * 4):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        r, g, b = px[x, y]
        speck = AGGREGATE_LIGHT if random.random() > 0.55 else AGGREGATE_DARK
        blend = random.uniform(0.10, 0.28)
        px[x, y] = lerp((r, g, b), speck, blend)

    draw = ImageDraw.Draw(img)
    draw_subtle_cracks(draw, px)

    # Faded grime streaks
    for _ in range(SIZE // 22):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        length = random.randint(24, 90)
        angle = random.uniform(-0.4, 0.4) + math.pi * 0.5
        for step in range(length):
            px_x = int(x + math.cos(angle) * step) % SIZE
            px_y = int(y + math.sin(angle) * step) % SIZE
            r, g, b = px[px_x, px_y]
            px[px_x, px_y] = lerp((r, g, b), GRIME, random.uniform(0.06, 0.14))

    add_tire_wear(px)
    return img


def build_height_map(albedo):
    gray = albedo.convert("L")
    gray = gray.filter(ImageFilter.GaussianBlur(radius=0.6))
    px = gray.load()
    h = Image.new("L", (SIZE, SIZE))
    hpx = h.load()
    for y in range(SIZE):
        for x in range(SIZE):
            v = px[x, y]
            hpx[x, y] = int(115 + (v / 255.0) * 95)
    return h


def height_to_normal(height_img, strength=2.2):
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
    albedo_path = os.path.join(SCRIPT_DIR, "Alley_Asphalt_Albedo.png")
    albedo.save(albedo_path, optimize=True)

    height = build_height_map(albedo)
    normal = height_to_normal(height)
    normal_path = os.path.join(SCRIPT_DIR, "Alley_Asphalt_Normal.png")
    normal.save(normal_path, optimize=True)

    print("Saved:", albedo_path)
    print("Saved:", normal_path)
    print(f"Sheet {SHEET_REAL_M}m x {SHEET_REAL_M}m")


if __name__ == "__main__":
    main()
