# 港硕这一年

Unity + C# 开发的中文 2D 单机养成游戏。在香港读一年制硕士，安排课程、科研、工作和日常生活。没有恋爱攻略系统。

当前为 **v2.0.0 试玩版**。完整学年为第一学期 8 轮、寒假 1 轮、第二学期 8 轮；常规轮每两周 28 个半日格，寒假安排两项活动。

![学生的房间](Assets/Resources/Art/RoomV2.png)

## 本版玩法

- **角色与开局**：姓名、男女、青年／中年、各性别独立设计的前发 4 款和后发 4 款、各 4 套服装；经济条件与住房可选。外观不影响能力。
- **半日日程**：点击格子安排行动，一键填入课表、空格批量安排、交换两个格子。每门课每轮仅有两个固定上课时段，不能集中刷课。未解锁活动灰显并排在后面。
- **八门课程**：两学期各四门，四门项目实践型、两门综合考核型、两门考试主导型。独立保存到课、病假、旷课；合规出勤低于 70% 判 F。任务有质量、截止日期和提交状态，考试看本课实际学习。
- **疲劳与生活**：过度透支会暂停执行、登记病假并休养，取消相应日程。资金不足可使用无息应急生活金，继续游戏。房租、家具槽位、食物、赠食与二手市场均有实际效果。
- **小组项目**：每局六位同学，能力和形象固定保存。五个岗位分别分工，技能匹配、可靠度与默契影响项目质量。
- **寒假**：探亲、旅游、寻找实习，可以重复选择。探亲和旅游有剧情图；旅游让第二学期第一轮新增压力与疲劳减半。
- **人生志愿**：主界面同时提醒课业与求职／读博目标。第 11—15 轮实际投递，两个回合后回复；属性不会自动产生 offer。
- **明确的学年终章**：第 17 轮后进入毕业成绩单与结局页。先判断毕业，再尊重志愿；留港／回内地可就业或继续求职，博士需要实际录取。多个方向可切换选择，奖学金与优秀表现可以叠加。
- **存档与声音**：开场、引导、每步自动保存、执行中继续、重新开始；原创轻音乐与点击音可关闭。v1 存档保留，v2 使用独立存档。

## 开发与运行

1. Unity Hub 添加本仓库根目录。
2. 使用 **Unity 2022.3.62f3c1** 打开，等待导入完成。依赖 uGUI 1.0.0 和 TextMeshPro 3.0.6。
3. 打开 `Assets/Scenes/HarbourYear.unity`，点击 Play。

中文字体与美术资源已随源码提供；不依赖系统字体。角色姓名支持字体覆盖的常用汉字。

| 构建 | 输出 | 运行 |
| --- | --- | --- |
| macOS | `Build/macOS/港硕这一年.app` | 双击 App 或 `开始游戏.command` |
| WebGL | `Build/WebGL` | `python3 Tools/preview.py` 后访问本地地址 |

```sh
./Tools/build.sh mac
./Tools/build.sh web
./Tools/test-rules.sh
```

Unity 安装路径不同时，可设置 `UNITY_BIN`（构建）和 `UNITY_ROOT`（规则检查）。Windows 开发者可从 Unity 菜单「港硕这一年」构建；尚无 Windows 原生可玩包。WebGL 需要通过 HTTP 服务运行，不要直接双击 HTML。

macOS 分享版未经过 Apple Developer ID 签名与公证，首次打开可能需要在系统设置中针对该应用选择「仍要打开」。

## 规则与可调参数

`Assets/Scripts/YearModel.cs` 的 `Balance` 和 `Catalog` 集中保存试玩参数：评分权重、GPA 换算、初始资金、房租、商品、行动收益与申请门槛。这些参数用于打通试玩，不代表策划数值已最终批准。详细规则见 [README.txt](README.txt)。

毕业只要求八门全过、CGPA ≥2.00。病假的简化合规记法与 70% 门槛属于游戏设定，不代表任何高校的统一现实政策。

| 模块 | 文件 |
| --- | --- |
| 数据和试玩参数 | `YearModel.cs` |
| 日程、课程、申请与结局 | `YearRules.cs` |
| 住房、物品、市场与小组 | `YearLife.cs` |
| 事件与后续回响 | `YearEvents.cs` |
| uGUI/TMP 页面 | `YearGame.cs`、`Year*UI.cs`、`NotebookUI.cs` |
| 分层像素角色与原创声音 | `PixelPeople.cs`、`YearAudio.cs` |
| 版本隔离存档与备份 | `YearSave.cs` |
| 规则与界面检查 | `Assets/Editor/YearChecks.cs`、`YearSmoke.cs` |

## 验证与分享

规则检查会生成 `docs/paths/` 的完整路线与 `docs/rules-results.txt`。Unity 交互检查覆盖开局、课表、锁定按钮、页面、执行中恢复、寒假、病假和毕业结局。验证详情见 [docs/VALIDATION.txt](docs/VALIDATION.txt)。

```sh
python3 Tools/package.py
python3 Tools/package_share.py
```

打包会包含当前原生版、浏览器版和源码。`Build/`、`release/`、本地设计参考文档、缓存和凭据均不进入 Git。本地设计参考文档也不会进入源码分享包。

共同开发约定见 [CONTRIBUTING.md](CONTRIBUTING.md)。当前仓库未指定开源许可证；字体遵循 [SIL OFL](Assets/Resources/Fonts/OFL.txt)，TMP 保留随附许可证，美术提示词与原创声音说明见 `docs/ART-DIRECTION-V2.txt`。
