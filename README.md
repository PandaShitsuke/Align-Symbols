# AlignSymbols

**AlignSymbols** turns blocks of consecutive, structurally similar C/C++-style lines into a readable table by aligning symbols (`=`, `(`, `)`, `[`, `]`, `.`, `->`, `<<`, `+`, `|`, `,`, `;`) onto shared columns. It only **inserts whitespace** — it never reorders, renames or changes any token — so the semantics stay identical, and it only touches structure-identical consecutive lines.

**AlignSymbols** 把连续、结构相似的 C / C++ 代码行按符号（`=`、`(`、`)`、`[`、`]`、`.`、`->`、`<<`、`+`、`|`、`,`、`;`）分列对齐成可读的表格。它**只插入空格**——绝不重排、改名或改动任何 token——因此语义不变，并且只处理结构相同的连续行。

This repo contains **two products** that share the same alignment engine:

本仓库包含**两个产品**，共用同一套对齐引擎：

- **`align_symbols_vscode/`** — **VS Code extension** (package `align_symbols_vscode`; recommended, feature-complete) · VS Code 扩展（包名 `align_symbols_vscode`，推荐、功能最全）
- **`align_symbols_vs2013/`** — **Visual Studio 2013 extension** (package `align_symbols_vs2013`; for STS8300 / ATE test stations running VS 2013) · VS 2013 扩展（包名 `align_symbols_vs2013`，用于 STS8300 / ATE 机台的 VS 2013 环境）

---

## Features / 功能

- **Bitfield / shift-sum** — `LHS = LHS = ... = (member[sub] << shift) + ... ;` aligns the `=` and the `( member [sub] << shift )` cells plus the `+` / `|` separators.
- **Default / value table** — `[/* address */] member[sub] OP value; member[sub] OP value; ... ;` aligns each member so `[` / `]` line up and right-aligns numeric values.
- **Method / function call** — `[LHS =] obj.Func(arg1, arg2, ...);` aligns a leading `LHS =`, the object (so `.` / `->` / `(` align) and the arguments.
- **Nested member-access chain** — `OBJ1->Method1(a, b, OBJ2.Method2(c, d));` aligns every identifier before a `->` / `.` and treats `[` as an accessor so array assignments align their bracket columns.
- **Call argument `name[sub]`** — pads only the name so the inner `[sub]` aligns; chained `name[s1][s2]` is split at the first `[` so the rest of the bracket chain stays glued.
- **General fallback** — any run of consecutive lines with an identical symbol skeleton is aligned column-by-column; identifiers are left-aligned and **numeric literals are right-aligned**.
- **Safe & targeted** — attached operators (`.`, `->`, `(`, `[`, `]`, `::`) stay glued; `=`, `+`, `,`, `;`, `)` get a space around them; non-matching lines (control flow, function headers, mixed shapes) are left untouched.

--

- **位段 / 移位求和** —— `LHS = LHS = ... = (member[sub] << shift) + ... ;`，对齐 `=`、`( member [sub] << shift )` 各列以及 `+` / `|` 分隔符。
- **默认值 / 寄存器表** —— `[/* 地址 */] member[sub] OP value; member[sub] OP value; ... ;`，让每个 `member` 补齐使 `[` / `]` 成列，并让数值右对齐。
- **方法 / 函数调用** —— `[LHS =] obj.Func(arg1, arg2, ...);`，对齐前导的 `LHS =`、对象名（使 `.` / `->` / `(` 成列）以及各实参。
- **嵌套成员访问链** —— `OBJ1->Method1(a, b, OBJ2.Method2(c, d));`，按序对齐每个 `->` / `.` 前的标识符；同时把 `[` 当作访问符，数组赋值也会对齐下标列。
- **调用参数 `name[sub]`** —— 只填充名字使内部 `[sub]` 对齐；链式 `name[s1][s2]` 在首个 `[` 处拆分，其余括号链保持粘连。
- **通用兜底** —— 任何“符号骨架相同”的连续行都会按列对齐；标识符左对齐，**纯数字右对齐**。
- **安全且克制** —— 紧贴运算符（`.`、`->`、`(`、`[`、`]`、`::`）与前词之间不留空格；`=`、`+`、`,`、`;`、`)` 前后留一个空格；不匹配的行（控制流、函数头、混合结构）保持原样。

---

## Examples / 示例

In each pair the first block is **before** and the second is **after**. 每组示例中，上为**对齐前**，下为**对齐后**。

### 1. Bitfield / shift-sum assignment · 位段 / 移位求和赋值

```c
Fuse_Data_Write[0x39][site_no] = Fuse_Data[site_no][0x39] = (curr.trim_key[site_no] << 4) + (curr.boost_ocp_trim[site_no] << 0);
Fuse_Data_Write[0x3C][site_no] = Fuse_Data[site_no][0x3C] = (curr.boost_rcomp[site_no] << 5) + (curr.boost_ccomp_0[site_no] << 2) + (curr.com_sda_dly[site_no] << 0);

Fuse_Data_Write[0x39][site_no] = Fuse_Data[site_no][0x39] = (curr.trim_key             [site_no] << 4) + (curr.boost_ocp_trim      [site_no] << 0) ;
Fuse_Data_Write[0x3C][site_no] = Fuse_Data[site_no][0x3C] = (curr.boost_rcomp          [site_no] << 5) + (curr.boost_ccomp_0       [site_no] << 2) + (curr.com_sda_dly         [site_no] << 0) ;
```

### 2. Default / value register table · 默认值 / 寄存器表

```c
/* 0x39 */ dflt.trim_key[site_no] = 10; dflt.boost_ocp_trim[site_no] = 00;
/* 0x3C */ dflt.boost_rcomp[site_no] = 00; dflt.boost_ccomp_0[site_no] = 02; dflt.com_sda_dly[site_no] = 00;

/* 0x39 */ dflt.trim_key             [site_no] = 10; dflt.boost_ocp_trim      [site_no] = 00;
/* 0x3C */ dflt.boost_rcomp          [site_no] = 00; dflt.boost_ccomp_0       [site_no] = 02; dflt.com_sda_dly         [site_no] = 00;
```

### 3. Method / function call · 方法 / 函数调用

```c
fxvi_SDA.TSet(FV, 0, FXVIe_10V, FXVIe_10MA, FXVIe_RELAY_ON);
fxvi_VIN_LED.TSet(FV, 0, FXVIe_10V, FXVIe_10MA, FXVIe_RELAY_ON);
acm_RESET.TSet(FV, 0, ACM200_10V, ACM200_10MA, ACM200_RELAY_ON);

fxvi_SDA    .TSet(FV, 0, FXVIe_10V , FXVIe_10MA , FXVIe_RELAY_ON );
fxvi_VIN_LED.TSet(FV, 0, FXVIe_10V , FXVIe_10MA , FXVIe_RELAY_ON );
acm_RESET   .TSet(FV, 0, ACM200_10V, ACM200_10MA, ACM200_RELAY_ON);
```

### 4. Nested member-access chain · 嵌套成员访问链

```c
PIN30_LX->SetTestResult(site_no, 0, fxvi_LX.GetMeasResult(site_no, MVRET));
PIN12_LDO2->SetTestResult(site_no, 0, fxvi_LDO2.GetMeasResult(site_no, MVRET));
PIN14_RESET->SetTestResult(site_no, 0, acm_RESET.GetMeasResult(site_no, MVRET));

PIN30_LX   ->SetTestResult(site_no, 0, fxvi_LX  .GetMeasResult(site_no, MVRET));
PIN12_LDO2 ->SetTestResult(site_no, 0, fxvi_LDO2.GetMeasResult(site_no, MVRET));
PIN14_RESET->SetTestResult(site_no, 0, acm_RESET.GetMeasResult(site_no, MVRET));
```

### 5. Call argument `name[sub]` inner alignment · 调用参数名`[下标]`内部对齐

```c
	fxvi_VINP.GetContactCheckResult(site_no, vinp_kelvin[site_no], R_KL[site_no]);
	acm_VCORE.GetContactCheckResult(site_no, vcore_kelvin[site_no], R_KL[site_no]);

	fxvi_VINP  .GetContactCheckResult(site_no, vinp_kelvin [site_no], R_KL[site_no]);
	acm_VCORE  .GetContactCheckResult(site_no, vcore_kelvin[site_no], R_KL[site_no]);
```

### 6. Chained subscript call argument · 链式下标实参

```c
	LX_Kelvin->SetTestResult(site_no, 0, lx_kelvin[site_no]);
	CH1_Kelvin->SetTestResult(site_no, 0, ch_kelvin[site_no][0]);
	VIN_LED_Kelvin->SetTestResult(site_no, 0, vin_led_kelvin[site_no]);

	LX_Kelvin     ->SetTestResult(site_no, 0, lx_kelvin     [site_no]);
	CH1_Kelvin    ->SetTestResult(site_no, 0, ch_kelvin     [site_no][0]);
	VIN_LED_Kelvin->SetTestResult(site_no, 0, vin_led_kelvin[site_no]);
```

### 7. Array-subscript accessor (`[`) · 数组下标访问符

```c
global_lx_leak[leak_flag][site_no] = lx_leak[site_no];
global_vin_led_leak[leak_flag][site_no] = vin_led_leak[site_no];

global_lx_leak     [leak_flag][site_no] = lx_leak     [site_no];
global_vin_led_leak[leak_flag][site_no] = vin_led_leak[site_no];
```

### 8. Leading assignment + string literal · 前置赋值 + 字符串字面量

```c
CParam *LX_Leak = StsGetParam(funcindex, "LX_Leak");
CParam *VIN_LED_Leak = StsGetParam(funcindex, "VIN_LED_Leak");

CParam *LX_Leak      = StsGetParam(funcindex, "LX_Leak"     );
CParam *VIN_LED_Leak = StsGetParam(funcindex, "VIN_LED_Leak");
```

### 9. General fallback + numeric right-align · 通用兜底 + 数字右对齐

```c
int a = 1;
int bb = 22;
int ccc = 333;

int a   =   1 ;
int bb  =  22 ;
int ccc = 333 ;
```

---

## Install / 安装

### VS Code extension · VS Code 扩展

From a `.vsix` package / 从 .vsix 包安装:

```bash
code --install-extension release/align_symbols_vscode-0.9.0.vsix
```

Reload the window (Command Palette → **Developer: Reload Window**), then press `Ctrl+Alt+A`, or use the Command Palette command **Symbol Align (by column)** / the right-click editor menu. Select lines to align; if nothing is selected, the whole file is used.

安装后按 `Ctrl+Shift+P` 执行 **Developer: Reload Window** 重载，然后按 `Ctrl+Alt+A`，或使用命令面板 **Symbol Align (by column)** / 编辑器右键菜单。选中要对齐的行；未选中则对齐整个文件。

### Visual Studio 2013 extension · VS 2013 扩展

Open `align_symbols_vs2013\align_symbols_vs2013.sln` in VS 2013 → **Build Solution** (requires the **Visual Studio 2013 SDK**) → install `release\align_symbols_vs2013-0.9.0.vsix` (or run `align_symbols_vs2013-0.9.0.vsix /quiet`). Then **restart VS 2013**, select a block, and use **Tools → Align Selected Code** or the editor **right-click → Align Selected Code**.

在 VS 2013 打开 `align_symbols_vs2013\align_symbols_vs2013.sln` → **生成解决方案**（需已装 **VS 2013 SDK**）→ 安装 `release\align_symbols_vs2013-0.9.0.vsix`（或 `align_symbols_vs2013-0.9.0.vsix /quiet`）。然后**重启 VS 2013**，选中块 → **工具 → Align Selected Code** 或编辑器**右键 → Align Selected Code**。

---

## How it aligns / 对齐规则

1. Each column is aligned to the widest token at that position (per cell / per row).
2. For member / accessor columns the identifier is padded so the following `.`, `->`, `[`, `(` or operator lines up.
3. Numeric operands are right-aligned so `)` / `;` line up.
4. A row with fewer cells than the longest row has its trailing `;` snapped to the matching separator column, so short rows gain no trailing whitespace.
5. Only whitespace is inserted; tokens are unchanged.

1. 每一列对齐到该列最宽的 token（按单元格 / 按行）。
2. 成员/访问符列：标识符补齐，使其后的 `.`、`->`、`[`、`(` 或运算符对齐。
3. 数字操作数右对齐，使 `)` / `;` 成列。
4. 单元格更少的行会把它末尾的 `;` 对齐到最长行对应的分隔符列，因此短行不会产生尾部空白。
5. 只插入空格，token 不变。

---

## Project structure / 项目结构

```
Align Symbols/
├─ README.md                 # 项目总览 / overview (this file)
├─ CHANGELOG.md              # 双语更新日志（两种版本）/ bilingual changelog (both editions)
├─ LICENSE                   # MIT（项目级，两个扩展共用）
├─ .gitignore / .gitattributes
├─ align_symbols_vscode/     # VS Code 扩展
│  ├─ package.json           # 扩展清单 / extension manifest
│  ├─ extension.js           # 扩展入口（命令注册）/ entry (command registration)
│  ├─ aligner.js             # 对齐引擎（核心）/ alignment engine
│  ├─ LICENSE icon.png
│  ├─ scripts/               # build_vsix.py gen_icon.py verify_vsix.py release.py
│  ├─ test/                  # regression.js + cases/（回归用例）/ regression suite
│  ├─ release/               # 构建出的 *.vsix
│  └─ .vscodeignore
└─ align_symbols_vs2013/     # VS 2013 扩展
   ├─ align_symbols_vs2013.sln
   ├─ align_symbols_vs2013/  # 工程源码（.csproj、.cs、.vsct、manifest…）
   └─ release/               # 构建出的 .vsix
```

> The project-level `README.md` and `CHANGELOG.md` at the repo root are intentionally packaged into the VS Code extension as well, so the extension detail page shows the same combined documentation.
> 仓库根目录的 `README.md` 与 `CHANGELOG.md` 同时被打包进 VS Code 扩展，因此扩展详情页会展示这套合并文档。

---

## Development / 开发

### VS Code extension

```bash
# build the .vsix (writes release/align_symbols_vscode-<version>.vsix)
python scripts/build_vsix.py

# regenerate the icon (128x128 PNG)
python scripts/gen_icon.py

# run the regression suite
node test/regression.js
```

The extension is plain CommonJS — there is no compiler / bundler step; edits take effect after reloading the debug host. 扩展为纯 CommonJS，无编译/打包步骤；改动后重载调试宿主即可。

### VS 2013 extension

Build with the VS 2013 toolchain (`MSBuild 12` + VS SDK) or in VS 2013 via the solution. `Aligner.cs` is a C# port of the same rules; the command writes back with `EditPoint.ReplaceText` to avoid VS 2013 deadlocks. 用 VS 2013 工具链（MSBuild 12 + VS SDK）或在 VS 2013 里通过解决方案生成。`Aligner.cs` 是同一规则的 C# 移植；命令用 `EditPoint.ReplaceText` 写回以避免 VS 2013 死锁。

---

## Publish to VS Code Marketplace / 上架 VS Code 商店

This is a valid VS Code extension (`package.json`, `README.md`, `LICENSE`, `CHANGELOG.md`, an icon and `.vscodeignore`), so it can be published. 这是一个标准合法的 VS Code 扩展，可以发布。

```bash
npm install -g @vscode/vsce
vsce create-publisher PandaShitsuke     # or reuse an existing publisher
vsce login PandaShitsuke                # with a Marketplace PAT (Marketplace > Manage scope)
vsce package
vsce publish
```

---

## Changelog / 更新日志

See [CHANGELOG.md](CHANGELOG.md) for per-version changes (VS Code and VS2013 described separately). 各版本变更（VS Code 与 VS2013 分开说明）见 [CHANGELOG.md](CHANGELOG.md)。

## License / 许可

MIT — see [LICENSE](LICENSE).
