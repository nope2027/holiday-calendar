"""icons/icon-512.png → app.ico (작은 크기는 BMP, 256은 PNG — 위젯 프로그램 아이콘)"""
import io, struct, sys
from PIL import Image

src = Image.open(sys.argv[1] if len(sys.argv) > 1 else 'icons/icon-512.png').convert('RGBA')
frames = []
for s in (16, 20, 24, 32, 40, 48, 64, 256):
    im = src.resize((s, s), Image.LANCZOS)
    b = io.BytesIO()
    if s == 256:
        im.save(b, 'PNG')
        data = b.getvalue()
    else:
        im.save(b, 'ICO', sizes=[(s, s)], bitmap_format='bmp')
        raw = b.getvalue()
        size, off = struct.unpack('<II', raw[14:22])
        data = raw[off:off + size]
    frames.append((s, data))
head = struct.pack('<HHH', 0, 1, len(frames))
pos = 6 + 16 * len(frames)
ents, body = b'', b''
for s, data in frames:
    ents += struct.pack('<BBBBHHII', s % 256, s % 256, 0, 0, 1, 32, len(data), pos + len(body))
    body += data
with open(sys.argv[2] if len(sys.argv) > 2 else 'app.ico', 'wb') as f:
    f.write(head + ents + body)
