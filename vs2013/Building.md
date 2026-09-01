# Symbol Align — Visual Studio 2013 add-in

一个 VS 2013 扩展：**工具 → “Align Selected Code”，或在编辑器里右键 → “Align Selected Code”**，对齐当前选中的代码（按符号分列）。

> 注意：这是 **Visual Studio 2013（IDE）** 扩展，用 C# + VS 2013 SDK 编译，和 VS Code 扩展无关。

## 编译前需要

- Visual Studio **2013**（已安装）。
- **Visual Studio 2013 SDK**（编译 VSIX 扩展必需）。
  - 如果没装：打开“Visual Studio 2013 安装程序” → “修改”，勾选 **“Visual Studio 2013 SDK”** 安装；或单独下载 VS 2013 SDK。
- .NET Framework 4.5.1（VS 2013 自带）。

## 编译与运行

1. 用 **VS 2013** 打开 `AlignSymbols.csproj`。
2. 若提示需要 `Microsoft.VsSDK.targets`，确认已装 VS 2013 SDK。
3. 直接按 **F5**（调试）：
   - 会启动一个**实验实例（Experimental instance）**的 VS 2013。
   - 在实验实例里，菜单 **工具(Tools) → Align Selected Code**，以及在代码窗口**右键 → Align Selected Code** 都出现。
4. 用实验实例打开你的代码，选中要对其齐的块，点 **工具 → Align Selected Code**，或**右键 → Align Selected Code**。

## 生成并安装 .vsix 给正式 VS 2013

1. 菜单 **生成(Build) → 生成解决方案**。
2. 生成后在 `bin\Debug\`（或 `bin\Release\`）里有 `AlignSymbols.vsix`。
3. 双击该 `.vsix`，或运行：
   ```
   AlignSymbols.vsix /quiet
   ```
   安装到正式 VS 2013。

## 说明

- 命令读取当前文档**选中文本**，用 `Aligner.AlignText` 对齐后**替换选中区**。只插入空格，不改 token。
- 两个入口（工具菜单、编辑器右键菜单）共用同一个命令；无选中文本时命令为无操作。
- 对齐规则与仓库里的 VS Code 版一致（位段移位求和、`member[sub]=value` 表、方法/函数调用、嵌套成员访问链、数组下标、通用同骨架列对齐、数字右对齐）。
- 如果 `AlignSymbols.csproj` 的**程序集引用路径**在你的机器上报错，把 `$(DevEnvDir)PublicAssemblies\...` 换成你 VS 2013 实际的程序集目录（通常是 `C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\PublicAssemblies\`）。该目录在装了 SDK 后即存在。

## 文件

```
vs2013/
├─ AlignSymbols.csproj              # VS 2013 工程
├─ source.extension.vsixmanifest    # VSIX 清单(目标 VS 12.0)
├─ Menus.vsct                       # 工具菜单命令
├─ AlignSymbolsPackage.cs           # 包入口
├─ AlignSymbolsCommand.cs           # “对齐选中代码”命令
├─ Aligner.cs                       # 对齐算法(C# 移植)
├─ Properties/AssemblyInfo.cs
└─ Building.md                      # 本文档
```
