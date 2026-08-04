"""
Seamless weathered plaster-over-brick wall for HCMC alley boundary (west side).
Run: python GenerateAlleyPlasterWall.py
"""
import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SIZE = 512
SHEET_M = 2.0

BASE_A = (0xD4, 0xD0, 0xC8)
BASE_B = (0xC8, 0xC4, 0xBC)
BASE_C = (0xDC, 0xD8, 0xD0)
STAIN = (0x9A, 0x96, 0x90)
MOSS = (0x7A, 0x82, 0x6E)
PATCH = (0xBE, 0xBA, 0xB2)
CRACK = (0x8A, 0x86, 0x80)
BRICK = (0xB0, 0x98, 0x88)

random.seed(4524)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def build_albedo():
    img = Image.new("RGB", (SIZE, SIZE), BASE_B)
    px = img.load()

    for y in range(SIZE):
        for x in range(SIZE):
            n = math.sin(x * 0.09) * math.cos(y * 0.07) + random.uniform(-0.08, 0.08)
            base = lerp(BASE_A, BASE_C, (n + 1) * 0.5)
            if (x // 64 + y // 48) % 3 == 0:
                base = lerp(base, BRICK, 0.06)
            px[x, y] = base

    draw = ImageDraw.Draw(img)

    # Subtle brick coursing
    for y in range(0, SIZE, 26):
        draw.line((0, y, SIZE, y), fill=lerp(BRICK, BASE_B, 0.55), width=1)
    for y in range(13, SIZE, 26):
        offset = 32 if (y // 26) % 2 else 0
        for x in range(offset, SIZE, 64):
            draw.line((x, y, x, y + 12), fill=lerp(BRICK, BASE_B, 0.5), width=1)

    # Cement repair patches
    for _ in range(5):
        px0 = random.randint(20, SIZE - 90)
        py0 = random.randint(40, SIZE - 120)
        pw = random.randint(40, 110)
        ph = random.randint(35, 90)
        draw.rectangle((px0, py0, px0 + pw, py0 + ph), fill=PATCH)
        draw.rectangle((px0, py0, px0 + pw, py0 + ph), outline=lerp(PATCH, CRACK, 0.4), width=1)

    # Hairline cracks
    for _ in range(14):
        x0, y0 = random.randint(0, SIZE - 1), random.randint(0, SIZE - 1)
        length = random.randint(18, 80)
        angle = random.uniform(-0.4, 0.4)
        x1 = int(x0 + math.cos(angle) * length)
        y1 = int(y0 + math.sin(angle) * length)
        draw.line((x0, y0, x1, y1), fill=CRACK, width=1)

    # Water stains from top / ground splash
    for _ in range(6):
        sx = random.randint(0, SIZE)
        for dy in range(0, random.randint(60, 140)):
            y = SIZE - 1 - dy
            spread = dy // 8
            for dx in range(-spread, spread + 1):
                xx = (sx + dx) % SIZE
                t = dy / 140.0
                col = lerp(px[xx, y], STAIN, 0.12 + t * 0.18)
                px[xx, y] = col

    # Moss near bottom
    for y in range(SIZE - 70, SIZE):
        t = (y - (SIZE - 70)) / 70.0
        for x in range(SIZE):
            if random.random() < 0.08 + t * 0.22:
                px[x, y] = lerp(px[x, y], MOSS, 0.25 + t * 0.35)

    # Paint peeling specks
    for _ in range(120):
        x = random.randint(0, SIZE - 1)
        y = random.randint(0, SIZE - 1)
        px[x, y] = lerp(px[x, y], BASE_C, 0.35)

    return img.filter(ImageFilter.GaussianBlur(radius=0.4))


def build_normal(albedo):
    gray = albedo.convert("L")
    px = gray.load()
    out = Image.new("RGB", (SIZE, SIZE))
    opx = out.load()
    strength = 2.2
    for y in range(SIZE):
        for x in range(SIZE):
            xl = px[(x - 1) % SIZE, y]
            xr = px[(x + 1) % SIZE, y]
            yu = px[x, (y - 1) % SIZE]
            yd = px[x, (y + 1) % SIZE]
            dx = (xl - xr) / 255.0 * strength
            dy = (yu - yd) / 255.0 * strength
            nx = int((dx * 0.5 + 0.5) * 255)
            ny = int((dy * 0.5 + 0.5) * 255)
            opx[x, y] = (nx, ny, 255)
    return out


def main():
    albedo = build_albedo()
    normal = build_normal(albedo)
    albedo.save(os.path.join(SCRIPT_DIR, "Alley_PlasterWall_Albedo.png"), optimize=True)
    normal.save(os.path.join(SCRIPT_DIR, "Alley_PlasterWall_Normal.png"), optimize=True)
    print("Saved plaster wall textures", SIZE, "sheet_m", SHEET_M)


if __name__ == "__main__":
    main()
