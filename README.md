# 港硕这一年

一款 Unity + C# 开发的中文 2D 单机养成游戏。主角来香港读一年制硕士，在学业、科研、工作机会和日常生活之间安排自己的时间。没有恋爱攻略系统。

当前版本为 **v1.0.0 可玩初版**：26 个两周回合、每回合 4 个行动、10 类行动、4 门分学期开放的课程、29 个事件条目、5 种主结局，以及可叠加的荣誉。支持新手引导、自动存档、继续游戏与重新开始。

![香港校园插画](Assets/Resources/Art/Harbour.png)

## 打开项目

1. 克隆本仓库，或下载源码并完整解压。
2. 用 Unity Hub 添加仓库根目录，即包含 `Assets`、`Packages`、`ProjectSettings` 的目录。
3. 使用 **Unity 2022.3.62f3c1** 打开，等待资源导入与包解析完成。项目依赖 uGUI 1.0.0 和 TextMeshPro 3.0.6。
4. 打开 `Assets/Scenes/HarbourYear.unity`，点击 Play。

中文字体、TextMeshPro 必需资源与游戏插画均已入库，不依赖玩家电脑上的字体。多人协作请使用相同 Unity 版本；升级编辑器前先沟通，并单独提交版本变化。

## 构建与试玩

Unity 菜单 **港硕这一年** 提供「准备场景与字体」「构建 macOS」「构建 WebGL」。构建对应平台前，在 Unity Hub 安装相应 Build Support 模块。

| 输出 | 位置 | 启动方式 |
| --- | --- | --- |
| macOS 原生版 | `Build/macOS/港硕这一年.app` | 双击 App 或根目录 `开始游戏.command` |
| Unity WebGL | `Build/WebGL` | Python 3 运行 `python3 Tools/preview.py` |

浏览器版通过本地 `http://127.0.0.1:8916` 运行，请保持终端开启。不要直接双击 `index.html`。原生版与浏览器版的存档独立，浏览器存档与浏览器及访问端口有关。

macOS 下也可执行：

```sh
./Tools/build.sh mac
./Tools/build.sh web
```

若 Unity 安装在其他位置，可为构建脚本设置 `UNITY_BIN` 为 Unity 可执行文件路径。Windows 开发者可以直接使用 Unity 编辑器菜单；当前没有 Windows 原生构建入口。

发行包尚未经过 Apple Developer ID 签名和公证。首次在其他 Mac 打开时可能被 Gatekeeper 拦截；确认包来自项目负责人后，可以在「系统设置 → 隐私与安全性」针对该 App 选择「仍要打开」。

## 代码结构

| 文件或目录 | 用途 |
| --- | --- |
| `Assets/Scripts/YearModel.cs` | 纯 C# 回合、课程、资源、申请和结局规则 |
| `Assets/Scripts/YearEvents.cs` | 生活事件、选项与后续回响 |
| `Assets/Scripts/YearGame.cs` | 游戏流程与界面控制 |
| `Assets/Scripts/NotebookUI.cs` | uGUI / TextMeshPro 界面组件 |
| `Assets/Scripts/YearSave.cs` | 本地 JSON 存档及备份 |
| `Assets/Editor` | 场景准备、构建、规则与运行检查 |
| `Assets/Resources` | 中文字体与插画 |
| `Tools` | 构建、字体处理、本地试玩与发行打包脚本 |
| `docs/paths` | 实际规则测试生成的完整结局路线 |

完整玩法与数值说明见 [README.txt](README.txt)，验证记录见 [docs/VALIDATION.txt](docs/VALIDATION.txt)。

## 验证与打包

macOS 可独立运行规则检查：

```sh
./Tools/test-rules.sh
```

脚本使用 Unity 自带的 Mono；如安装目录不同，可设置 `UNITY_ROOT` 为 Unity.app 内的 `Contents` 路径。规则检查覆盖整年路径、全部结局、重复行动、高压、逾期补交、资金兜底、申请结果与事件回响。修改界面或存档时，还应实际检查对应操作流程。

完成 macOS 与 WebGL 构建后，用 Python 3 生成发行包：

```sh
python3 Tools/package.py
python3 Tools/package_share.py
```

生成的 `Build/`、`release/` 与 Unity 缓存不进入 Git。发行包可单独分享或上传 GitHub Releases。

## 共同开发

采用 `main` 主分支，每项改动新建功能分支，通过 Pull Request 合并。所有 Unity 资源必须连同对应 `.meta` 一起提交，避免丢失 GUID 引用。具体约定见 [CONTRIBUTING.md](CONTRIBUTING.md)。

当前仓库未声明开源许可证；协作者访问权限由仓库所有者管理。字体保留 [SIL Open Font License](Assets/Resources/Fonts/OFL.txt)，TextMeshPro 资源保留随附许可证。游戏插画来源与生成提示词见 [docs/ART-DIRECTION.txt](docs/ART-DIRECTION.txt)。
