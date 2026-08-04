"""
Seamless green corrugated metal fence texture for HCMC alley boundary (east side).
Run: python GenerateAlleyCorrugatedFence.py
"""
import math
import os
import random

from PIL import Image, ImageDraw, ImageFilter

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SIZE = 512
SHEET_M = 1.2
RIBS = 18

GREEN_A = (0x3E, 0x6B, 0x52)
GREEN_B = (0x48, 0x74, 0x5A)
GREEN_C = (0x36, 0x62, 0x4A)
RUST = (0x8A, 0x5C, 0x3E)
RUST_DARK = (0x6E, 0x48, 0x30)
FADE = (0x58, 0x7E, 0x64)
SCRATCH = (0x62, 0x88, 0x6E)

random.seed(4588)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def rib_shade(x, rib_count):
    phase = (x / SIZE) * rib_count * math.pi * 2
    return 0.5 + 0.5 * math.sin(phase)


def build_albedo():
    img = Image.new("RGB", (SIZE, SIZE), GREEN_B)
    px = img.load()

    for y in range(SIZE):
        sun_fade = 0.85 + 0.15 * (y / SIZE)
        for x in range(SIZE):
            rib = rib_shade(x, RIBS)
            wave = math.sin(y * 0.11 + x * 0.02) * 0.04
            dimple = random.uniform(-0.03, 0.03) if random.random() < 0.04 else 0
            t = rib + wave + dimple
            if t > 0.62:
                col = lerp(GREEN_B, GREEN_C, (t - 0.5) * 1.4)
            else:
                col = lerp(GREEN_A, GREEN_B, t * 1.2)
            col = lerp(col, FADE, (1.0 - sun_fade) * 0.35)
            px[x, y] = col

    draw = ImageDraw.Draw(img)

    # Rust band near bottom
    for y in range(SIZE - 90, SIZE):
        t = (y - (SIZE - 90)) / 90.0
        for x in range(SIZE):
            if random.random() < 0.15 + t * 0.55:
                rust_col = RUST if random.random() > 0.35 else RUST_DARK
                px[x, y] = lerp(px[x, y], rust_col, 0.25 + t * 0.45)

    # Scratches along corrugation
    for _ in range(22):
        y = random.randint(10, SIZE - 20)
        x0 = random.randint(0, SIZE - 40)
        length = random.randint(20, 120)
        draw.line((x0, y, x0 + length, y + random.randint(-2, 2)), fill=SCRATCH, width=1)

    # Dents (darker oval smudges)
    for _ in range(8):
        cx = random.randint(30, SIZE - 30)
        cy = random.randint(40, SIZE - 60)
        rw = random.randint(8, 22)
        rh = random.randint(4, 10)
        draw.ellipse((cx - rw, cy - rh, cx + rw, cy + rh), fill=lerp(GREEN_C, GREEN_A, 0.45))

    return img.filter(ImageFilter.GaussianBlur(radius=0.35))


def build_normal(albedo):
    gray = albedo.convert("L")
    px = gray.load()
    out = Image.new("RGB", (SIZE, SIZE))
    opx = out.load()
    strength = 3.0
    for y in range(SIZE):
        for x in range(SIZE):
            xl = px[(x - 1) % SIZE, y]
            xr = px[(x + 1) % SIZE, y]
            yu = px[x, (y - 1) % SIZE]
            yd = px[x, (y + 1) % SIZE]
            dx = (xl - xr) / 255.0 * strength
            dy = (yu - yd) / 255.0 * strength
            opx[x, y] = (int((dx * 0.5 + 0.5) * 255), int((dy * 0.5 + 0.5) * 255), 255)
    return out


def main():
    albedo = build_albedo()
    normal = build_normal(albedo)
    albedo.save(os.path.join(SCRIPT_DIR, "Alley_CorrugatedFence_Albedo.png"), optimize=True)
    normal.save(os.path.join(SCRIPT_DIR, "Alley_CorrugatedFence_Normal.png"), optimize=True)
    print("Saved corrugated fence textures")


if __name__ == "__main__":
    main()
