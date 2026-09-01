# Symbol Align — Visual Studio 2013 Add-in

一个 **Visual Studio 2013（IDE）扩展**：**工具 → “Align Selected Code”**，以及在**代码编辑器右键 → “Align Selected Code”**，对齐当前选中的代码（按符号分列）。用于 STS8300 / ATE 机台的 VS 2013 环境。

> 这是 **VS 2013 扩展**（C# + VS 2013 SDK），和 `vscode/` 里的 VS Code 扩展是两个不同产品；两个共用同一套对齐规则。

## 目录

```
vs2013-extension/
├─ AlignSymbols.sln
├─ AlignSymbols/
│  ├─ AlignSymbols.csproj          # VS 2013 工程（.NET 4.5 / VS SDK）
│  ├─ AlignSymbolsPackage.cs       # 包 + “对齐选中代码”命令（EditPoint 写回，避免死锁）
│  ├─ Aligner.cs                   # 对齐算法（C# 移植）
│  ├─ AlignSymbols.vsct            # 菜单：工具 + 右键
│  ├─ source.extension.vsixmanifest
│  └─ ...
├─ README.md                       # 本文件
└─ release/                        # 构建出的 AlignSymbols.vsix
```

## 编译与安装

1. 用 **VS 2013** 打开 `AlignSymbols.sln`。
2. **生成 → 生成解决方案**（需已装 **Visual Studio 2013 SDK**）。
3. 成功后运行 `bin\Debug\AlignSymbols.vsix` 双击安装，或：
   ```
   AlignSymbols.vsix /quiet
   ```
4. **重启 VS 2013**，打开代码 → 选中块 → **工具 → Align Selected Code** 或**右键 → Align Selected Code**。

## 说明 / 已知点

- 命令用 **`EditPoint.ReplaceText`** 写回（直接用 `TextSelection.Text =` 在 VS 2013 里不生效，会导致“没变化”）。
- `Aligner.AlignText` 只插入空格、不改 token；一次选**几十行**为宜，选整文件时 VS 2013 重解析会较慢。
- 对齐算法与 VS Code 版一致（见仓库根 README）。
