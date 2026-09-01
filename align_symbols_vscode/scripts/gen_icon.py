import os
from PIL import Image, ImageDraw

SIZE = 128
img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
d = ImageDraw.Draw(img)

# rounded dark background
d.rounded_rectangle((4, 4, SIZE - 4, SIZE - 4), radius=26, fill=(22, 27, 34, 255))

# alignment bars: three full-width teal bars + one shorter, to read as "aligned columns"
bar_h = 8
bar_gap = 10
ys = [26, 44, 62, 88]
widths = [88, 88, 88, 56]
for y, w in zip(ys, widths):
    x0 = (SIZE - w) // 2
    x1 = x0 + w
    d.rounded_rectangle((x0, y, x1, y + bar_h), radius=4, fill=(78, 201, 176, 255))

out = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "icon.png"))
img.save(out)
print("wrote icon.png", img.size)
