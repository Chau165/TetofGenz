"""
Generate HCMC-style alley entrance sign albedo (Hẻm 45 Bùi Đình Túy).
Run: python GenerateAlleyEntranceSign.py
"""
import math
import os

from PIL import Image, ImageDraw, ImageFont

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
W, H = 2800, 620
CONTENT_W = 1650
BLUE = (0x12, 0x2F, 0x6B)
WHITE = (0xF4, 0xF6, 0xF8)
WEAR = (0x1A, 0x38, 0x72)

FONT_BOLD = "C:/Windows/Fonts/arialbd.ttf"
FONT_REG = "C:/Windows/Fonts/arial.ttf"


def load_font(path, size):
    try:
        return ImageFont.truetype(path, size)
    except OSError:
        return ImageFont.load_default()


def draw_corner_flourish(draw, cx, cy, flip_x=False, flip_y=False):
    for i in range(3):
        r = 10 + i * 5
        x0, x1 = cx - r, cx + r
        y0, y1 = cy - r, cy + r
        if x1 < x0:
            x0, x1 = x1, x0
        if y1 < y0:
            y0, y1 = y1, y0
        start = 200 if not flip_x else 20
        end = 290 if not flip_x else 110
        draw.arc((x0, y0, x1, y1), start, end, fill=WHITE, width=2)


def draw_pin(draw, x, y, size=16):
    draw.ellipse((x - size * 0.35, y - size * 0.9, x + size * 0.35, y - size * 0.2), fill=WHITE)
    tri = [(x, y + size * 0.55), (x - size * 0.42, y - size * 0.05), (x + size * 0.42, y - size * 0.05)]
    draw.polygon(tri, fill=WHITE)
    draw.ellipse((x - size * 0.14, y - size * 0.62, x + size * 0.14, y - size * 0.34), fill=BLUE)


def add_wear(img):
    px = img.load()
    import random

    random.seed(45)
    for _ in range(W * H // 180):
        x = random.randint(0, W - 1)
        y = random.randint(0, H - 1)
        r, g, b = px[x, y]
        t = random.uniform(0.04, 0.12)
        px[x, y] = (
            int(r * (1 - t) + WEAR[0] * t),
            int(g * (1 - t) + WEAR[1] * t),
            int(b * (1 - t) + WEAR[2] * t),
        )


def build():
    img = Image.new("RGB", (W, H), BLUE)
    draw = ImageDraw.Draw(img)

    margin = 18
    inner = 30
    draw.rounded_rectangle((margin, margin, W - margin, H - margin), radius=6, outline=WHITE, width=4)
    draw.rounded_rectangle((inner, inner, W - inner, H - inner), radius=4, outline=WHITE, width=2)

    content_left = (W - CONTENT_W) // 2
    cx = content_left + CONTENT_W // 2

    draw_corner_flourish(draw, inner + 8, inner + 8, False, False)
    draw_corner_flourish(draw, W - inner - 8, inner + 8, True, False)
    draw_corner_flourish(draw, inner + 8, H - inner - 8, False, True)
    draw_corner_flourish(draw, W - inner - 8, H - inner - 8, True, True)

    font_top = load_font(FONT_BOLD, 54)
    font_num = load_font(FONT_BOLD, 150)
    font_street = load_font(FONT_BOLD, 72)
    font_addr = load_font(FONT_BOLD, 30)

    top = "HẺM"
    bbox = draw.textbbox((0, 0), top, font=font_top)
    tw = bbox[2] - bbox[0]
    draw.text((cx - tw * 0.5, 42), top, fill=WHITE, font=font_top)

    line_y = 98
    draw.line((cx - 320, line_y, cx - 90, line_y), fill=WHITE, width=2)
    draw.line((cx + 90, line_y, cx + 320, line_y), fill=WHITE, width=2)

    num = "45"
    street = "BÙI ĐÌNH TÚY"
    nb = draw.textbbox((0, 0), num, font=font_num)
    nw = nb[2] - nb[0]
    sb = draw.textbbox((0, 0), street, font=font_street)
    sw = sb[2] - sb[0]
    gap = 28
    total = nw + gap + sw
    x0 = cx - total * 0.5
    draw.text((x0, 118), num, fill=WHITE, font=font_num)
    draw.text((x0 + nw + gap, 168), street, fill=WHITE, font=font_street)

    div_y = 430
    draw.line((inner + 20, div_y, W - inner - 20, div_y), fill=WHITE, width=2)

    addr = "PHƯỜNG 24 - QUẬN BÌNH THẠNH - TP. HỒ CHÍ MINH"
    ab = draw.textbbox((0, 0), addr, font=font_addr)
    aw = ab[2] - ab[0]
    pin_x = cx - aw * 0.5 - 22
    draw_pin(draw, int(pin_x), 498, 18)
    draw.text((pin_x + 26, 472), addr, fill=WHITE, font=font_addr)

    add_wear(img)
    return img


def main():
    out = os.path.join(SCRIPT_DIR, "AlleyEntranceSign_Albedo.png")
    build().save(out, optimize=True)
    print("Saved:", out)


if __name__ == "__main__":
    main()
