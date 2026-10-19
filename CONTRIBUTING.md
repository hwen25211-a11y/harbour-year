# 共同开发约定

先按 [README](README.md) 打开并运行项目。此仓库保存源码和必要资源；缓存、用户设置、存档、构建包及本地凭据不进入版本控制。

## 一次改动的流程

1. 更新本地 `main`，新建功能分支，例如 `feature/course-feedback` 或 `fix/save-recovery`。
2. 完成一项范围明确的改动，运行与改动有关的检查。
3. 提交并推送分支，创建 Pull Request，说明改动内容和验证结果。
4. 由另一位协作者查看后合并到 `main`。避免直接在主分支上进行多人并行修改。

分支和评审属于团队约定；是否启用强制分支保护由仓库所有者决定。

## Unity 资源

- 提交 `Assets` 中资源时，同时提交相应 `.meta`；移动或改名优先在 Unity 编辑器里完成。
- 保持 Visible Meta Files 与 Force Text，当前项目已启用这两项设置。
- 更换 Unity 版本、更新依赖或重建字体应单独提交，便于检查产生的资源变化。
- 同时编辑场景、字体图集等大资源前先沟通，避免不易处理的冲突。

## 按改动选择验证

- 规则、课程、申请、事件和结局：运行 `Tools/test-rules.sh`，检查受影响的整年路线及边界情况。
- 界面：在编辑器或构建中检查可读性、按钮状态、滚动、弹窗和不同窗口大小。
- 存档：检查新游戏、保存、退出、继续，以及事件和总结阶段的恢复。
- 字体或文案：检查新增汉字是否显示完整；按 `Assets/Resources/Fonts/FONT-SOURCE.txt` 更新字体。

Windows 开发者可在 Unity 中调用 `HarbourYear.Tests.YearChecks.Run()` 执行规则检查；现有 shell 启动脚本面向 macOS。

## 提交内容

提交前检查 `git diff --staged`。不提交 `Library`、`Temp`、`Logs`、`UserSettings`、`Build`、`release`、账号密码、访问令牌或签名证书。不要把测试用存档加入仓库。

在 Pull Request 中记录需要重建的发行包。源码更新不会自动更新别人已经下载的可玩版。
