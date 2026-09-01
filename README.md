# AlignSymbols

这个仓库包含**两个独立但同源的“按符号分列对齐”插件**，用于把结构相似的代码对齐成可读的表格：

- **`align_symbols_vscode/`** — **VS Code 扩展**（包名 `align_symbols_vscode`，推荐、功能最全）
- **`align_symbols_vs2013/`** — **Visual Studio 2013 扩展**（包名 `align_symbols_vs2013`，用于 STS8300 / ATE 机台的 VS 2013 环境）

两者共用同一套对齐规则：识别“位段移位求和、`member[sub]=value` 表、方法/函数调用、嵌套成员访问链、数组下标、通用同骨架连续行”，只插入空格、不改写 token，数字右对齐。不匹配的行保持原样。

## 目录结构

```
Align Symbols/
├─ README.md                 # 本文件（项目总览）
├─ CHANGELOG.md              # 更新日志（VS Code 与 VS2013 两种版本）
├─ LICENSE                   # MIT（项目级，两个扩展共用）
├─ .gitignore / .gitattributes
├─ align_symbols_vscode/     # VS Code 扩展
│  ├─ package.json           # 扩展清单
│  ├─ extension.js           # 扩展入口（命令注册）
│  ├─ aligner.js             # 对齐引擎（核心）
│  ├─ README.md LICENSE icon.png
│  ├─ scripts/               # build_vsix.py gen_icon.py verify_vsix.py release.py
│  ├─ test/                  # regression.js + cases/（回归用例）
│  ├─ release/               # 构建出的 *.vsix
│  └─ .vscodeignore
└─ align_symbols_vs2013/     # VS 2013 扩展
   ├─ AlignSymbols.sln
   ├─ AlignSymbols/          # 工程源码（.csproj、.cs、.vsct、manifest…）
   ├─ README.md              # VS 2013 版说明
   └─ release/               # 构建出的 .vsix
```

## 快速开始

### VS Code 扩展
在 `align_symbols_vscode/` 目录：
```bash
# 运行回归
node test/regression.js
# 打包（生成 release/align-symbols-<version>.vsix）
python scripts/build_vsix.py
# 安装
code --install-extension release/align_symbols_vscode-0.9.0.vsix
```
使用：选中代码 → `Ctrl+Alt+A`（或命令面板 “Symbol Align (by column)”，右键菜单同名）。

### VS 2013 扩展
打开 `align_symbols_vs2013\align_symbols_vs2013.sln` → 生成 → 安装 `release\align_symbols_vs2013-0.9.0.vsix`。
使用：**工具 → Align Selected Code**，或编辑器**右键 → Align Selected Code**。

详见 `align_symbols_vscode/README.md` 与 `align_symbols_vs2013/README.md`。

## 更新日志

各版本的变更（VS Code 与 VS2013 分开说明）见 [CHANGELOG.md](CHANGELOG.md)。

## 许可
MIT — 见 [LICENSE](LICENSE)。
