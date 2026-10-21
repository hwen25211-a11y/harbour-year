"""LiteSmoke 截图为 PNG；转为 1280 宽的 JPG 以免撑大源码包：python3 Tools/shrink_screenshots.py"""
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[1]/'docs/screenshots'
for p in sorted(root.glob('v3-*.png')):
    im=Image.open(p).convert('RGB');im=im.resize((1280,round(im.height*1280/im.width)),Image.LANCZOS);im.save(p.with_suffix('.jpg'),quality=85);p.unlink()
print(len(list(root.glob('v3-*.jpg'))),'screenshots')
