"""Create a recipient-friendly bundle from the verified Unity release artifacts."""
from pathlib import Path
import hashlib
import json
import stat
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'release'
PREFIX = '港硕这一年-分享包-v3.0.0'
README = '''《港硕这一年》v3.0.0 · 分享试玩包

一名一年制硕士生的香港生活。v3 把网页版《港硕生活模拟器 · 轻松版四轮样例》完整搬进 Unity：
打开游戏先到海港夕阳的开始页面（新游戏／继续游戏／CG画廊／设置），玩第一学期前四轮（八周）。
每个阶段选一次倾向（学业成长／香港生活／工作体验），日常排程交给系统，也会替你留出休息；
事件插图、维港与麦理浩径的旅行照片、背景音乐和音效都在。原来的 17 轮完整学年（经典版）仍然保留。
中文单机养成游戏，不含恋爱攻略。试玩不需要安装 Unity。

先完整解压这个压缩包，再启动游戏；不要在压缩包窗口内直接运行。

Mac 用户
双击根目录的「开始游戏.command」。
也可以直接打开 Build/macOS/港硕这一年.app。

Windows 用户 / 浏览器试玩
此包没有 Windows 原生 exe，请使用内附 Unity WebGL 浏览器版。
电脑需有 Python 3；安装时应提供 py 命令或加入 PATH。
Windows 双击「浏览器试玩-Windows.bat」。
Mac 双击「浏览器试玩.command」（浏览器启动器同样需要 Python 3）。
启动后打开 http://127.0.0.1:8916，保持启动器终端窗口开启。
不要直接双击 WebGL/index.html：Unity 浏览器版需要通过本地 HTTP 服务运行。
若提示端口 8916 已被占用，先关闭之前启动的试玩服务，再启动本包。

怎么玩（轻松版）
1. 开始页面点「新游戏」，看一屏开场（可跳过），填名字、选形象，点「开始这段生活」。
2. 主页中间的主按钮会告诉你现在该做什么，一路点它就能玩；遇到事件、结果、旅行回顾和组队会停下。
3. 第 1、3 轮开始时选倾向；想指定某个半日做什么，用「日程 → 本轮安排 → 特别安排」或「详细日历」。
4. 声音默认关闭：开始页面右上角「开启声音」，或游戏里右上角齿轮 → 声音。
5. 第 4 轮结束时显示「四轮样例结束」（不是毕业）。

存档
游戏自动保存到当前电脑（浏览器版保存在当前浏览器）。关闭后从「继续游戏」接着玩。
「设置 → 存档」可以导出／导入；Mac 版导出到桌面。网页版轻松版导出的存档也能导入。
原生版与浏览器版存档互不共用。本包不含开发者的个人存档。

经典版
开始页面「设置 → 经典版」打开 v2 的 17 轮完整学年（课程、小组、生活、目标与毕业结局），存档与轻松版分开。

如果想看代码
「源码/HarbourYear-UnitySource.zip」是完整 Unity 项目源码。
解压后，用 Unity 2022.3 LTS 打开 HarbourYear-Unity 文件夹，
打开 Assets/Scenes/HarbourYear.unity，再点击 Play。
详细玩法与开发说明见「玩法与开发说明.txt」。

Windows 启动器未在 Windows 实机测试。插图、照片与音乐由组员提供，尚未经过最终视觉验收与真人试听。
字体授权在「许可证/字体-OFL.txt」。
'''
WINDOWS = r'''@echo off
cd /d "%~dp0"
py -3 --version >nul 2>&1
if not errorlevel 1 (
  py -3 Tools\preview.py
  goto finished
)
python --version >nul 2>&1
if not errorlevel 1 (
  python Tools\preview.py
  goto finished
)
echo Python 3 is required to run this local WebGL launcher.
echo Install Python 3 with the Python launcher or PATH option, then try again.
:finished
pause
'''


def build():
    subprocess.run([sys.executable, str(ROOT / 'Tools/package.py')], cwd=ROOT, check=True)
    items = []
    for folder in ('Build/macOS', 'Build/WebGL'):
        for p in sorted((ROOT / folder).rglob('*')):
            if p.is_file() and p.name != '.DS_Store':
                items.append((str(p.relative_to(ROOT)), p.read_bytes(), p.stat().st_mode))
    for src, dest in (
        ('开始游戏.command', '开始游戏.command'),
        ('浏览器试玩.command', '浏览器试玩.command'),
        ('Tools/preview.py', 'Tools/preview.py'),
        ('release/HarbourYear-UnitySource.zip', '源码/HarbourYear-UnitySource.zip'),
        ('README.txt', '玩法与开发说明.txt'),
        ('Assets/Resources/Fonts/OFL.txt', '许可证/字体-OFL.txt'),
    ):
        p = ROOT / src
        items.append((dest, p.read_bytes(), p.stat().st_mode))
    items.append(('先看这里.txt', README.encode('utf-8-sig'), stat.S_IFREG | 0o644))
    items.append(('浏览器试玩-Windows.bat', WINDOWS.replace('\n', '\r\n').encode('ascii'), stat.S_IFREG | 0o644))
    manifest = {name: {'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()} for name, data, _ in items}
    items.append(('文件校验.json', (json.dumps(manifest, ensure_ascii=False, indent=2)+'\n').encode(), stat.S_IFREG | 0o644))
    archive = OUT / (PREFIX + '.zip')
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for name, data, mode in items:
            info = zipfile.ZipInfo.from_file(ROOT / 'README.txt', PREFIX + '/' + name)
            info.external_attr = mode << 16
            info.compress_type = zipfile.ZIP_DEFLATED
            z.writestr(info, data)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None
        for name, data, _ in items:
            assert z.read(PREFIX + '/' + name) == data, name
        native = z.getinfo(PREFIX + '/Build/macOS/港硕这一年.app/Contents/MacOS/港硕这一年')
        assert (native.external_attr >> 16) & stat.S_IXUSR
        assert (z.getinfo(PREFIX + '/开始游戏.command').external_attr >> 16) & stat.S_IXUSR
        forbidden = ('/Library/', '/UserSettings/', '/Logs/', '/Temp/', '.DS_Store')
        assert not any(part in name for part in forbidden for name in z.namelist())
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    (OUT / (PREFIX + '.sha256')).write_text(digest + '  ' + archive.name + '\n')
    print(json.dumps({'archive': str(archive), 'files': len(items), 'bytes': archive.stat().st_size,
                      'MiB': round(archive.stat().st_size / 1024**2, 1), 'sha256': digest,
                      'crc': 'passed', 'file_byte_comparison': 'passed', 'executable_permissions': 'passed'},
                     ensure_ascii=False, indent=2))

if __name__ == '__main__':
    build()
