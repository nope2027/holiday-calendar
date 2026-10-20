"""휴장 캘린더 Windows 패키지 폴더를 zip 으로 묶음 — 어떤 Windows·언어에서도 탐색기 '모두 압축 풀기'가 되게

- zip 안의 이름은 모두 영문(ASCII)만: 한글 이름은 Windows 탐색기가 읽는 방식(코드페이지)에 따라
  '압축(ZIP) 폴더가 잘못되었습니다' 오류가 날 수 있어서 아예 쓰지 않음 (한글이 있으면 멈춤)
- 폴더 안의 내용이 zip 맨 위에 들어감 → 탐색기로 풀면 zip 이름과 같은 폴더 안에 바로 파일이 생김
- 폴더 항목 없이 파일만, Windows(MS-DOS) 방식 속성
사용: python make_zip.py <패키지 폴더> <만들 zip>
"""
import os, sys, time, zipfile

src, out = os.path.abspath(sys.argv[1]), sys.argv[2]
items = []
for root, dirs, files in os.walk(src):
    dirs.sort()
    for f in sorted(files):
        full = os.path.join(root, f)
        items.append((full, os.path.relpath(full, src).replace(os.sep, '/')))

bad = [rel for _, rel in items if not rel.isascii() or len(rel.encode()) >= 200]
if bad:
    sys.exit('zip 안 이름은 영문(ASCII)·짧게만: ' + ', '.join(bad))

stamp = time.localtime(int(os.environ.get('SOURCE_DATE_EPOCH', time.time())))[:6]
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as z:
    for full, rel in items:
        zi = zipfile.ZipInfo(rel, date_time=stamp)
        zi.create_system = 0                 # MS-DOS · Windows
        zi.external_attr = 0x20              # 보관(archive) 속성
        zi.compress_type = zipfile.ZIP_DEFLATED
        with open(full, 'rb') as fh:
            z.writestr(zi, fh.read(), compresslevel=9)

with zipfile.ZipFile(out) as z:
    assert z.testzip() is None
    for i in z.infolist():
        assert not (i.flag_bits & 0x800) and i.filename.isascii()
        print('%9d  %s' % (i.file_size, i.filename))
