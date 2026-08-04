"""Blurred Vietnamese apartment exterior view for study window. Run: python GenerateStudyWindowExterior.py"""
import os
from PIL import Image, ImageDraw, ImageFilter

OUT = os.path.join(os.path.dirname(__file__), "StudyWindowExterior.png")
W, H = 384, 512


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def main():
    img = Image.new("RGB", (W, H), (0x6A, 0xA8, 0xE8))
    px = img.load()
    for y in range(H):
        t = y / (H - 1)
        col = lerp((0x78, 0xB8, 0xF0), (0xC8, 0xE4, 0xF8), t * 0.85)
        for x in range(W):
            px[x, y] = col

    draw = ImageDraw.Draw(img)
    # soft clouds
    for cx, cy, rw, rh in ((90, 88, 70, 22), (250, 110, 90, 26), (170, 60, 55, 18)):
        draw.ellipse((cx - rw, cy - rh, cx + rw, cy + rh), fill=(0xF4, 0xF8, 0xFF))
        draw.ellipse((cx - rw * 0.6, cy - rh * 1.2, cx + rw * 0.5, cy + rh * 0.4), fill=(0xF8, 0xFB, 0xFF))

    # distant apartment blocks (muted)
    buildings = (
        (30, 250, 95, 220, (0x9A, 0x96, 0x90)),
        (120, 280, 80, 190, (0xA8, 0xA4, 0x9C)),
        (210, 260, 110, 210, (0x8E, 0x8C, 0x88)),
        (300, 300, 70, 170, (0xB0, 0xAA, 0xA2)),
    )
    for x, y, bw, bh, col in buildings:
        draw.rectangle((x, y, x + bw, y + bh), fill=col)
        for row in range(y + 18, y + bh - 8, 22):
            for colx in range(x + 8, x + bw - 8, 18):
                draw.rectangle((colx, row, colx + 10, row + 12), fill=lerp(col, (0xD8, 0xE8, 0xF8), 0.35))

    # green tree crowns
    for tx, ty, r in ((55, 360, 34), (185, 390, 40), (320, 370, 30)):
        draw.ellipse((tx - r, ty - r, tx + r, ty + r), fill=(0x5C, 0x9A, 0x62))
        draw.rectangle((tx - 6, ty, tx + 6, ty + 45), fill=(0x6A, 0x52, 0x3C))

    img = img.filter(ImageFilter.GaussianBlur(radius=2.2))
    img.save(OUT, optimize=True)
    print("Saved", OUT)


if __name__ == "__main__":
    main()
