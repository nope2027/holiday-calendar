"""폴더 하나를 zip 으로 묶음 — 한글 이름이 Windows에서 깨지지 않게 UTF-8 표시를 붙임"""
import os, sys, zipfile

src, out = os.path.abspath(sys.argv[1]), sys.argv[2]
base = os.path.dirname(src)
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    for root, dirs, files in os.walk(src):
        dirs.sort()
        rel = os.path.relpath(root, base).replace(os.sep, '/')
        z.write(root, rel + '/')
        for f in sorted(files):
            z.write(os.path.join(root, f), rel + '/' + f)
for i in zipfile.ZipFile(out).infolist():
    print('%9d  %s' % (i.file_size, i.filename))
