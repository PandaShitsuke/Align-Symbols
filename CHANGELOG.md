# Changelog / 更新日志

This repo contains two products that share the same column-alignment rules: the **VS Code extension** (`align_symbols_vscode`) and the **Visual Studio 2013 extension** (`align_symbols_vs2013`). Their version numbers stay in sync for the same feature set.

本仓库包含两个共用同一套“按符号分列对齐”规则的产品：**VS Code 扩展**（`align_symbols_vscode`）与 **Visual Studio 2013 扩展**（`align_symbols_vs2013`）。两者针对相同功能集时版本号保持一致。

> The VS2013 extension only exists from **0.9.0** onwards; earlier versions ship the VS Code extension only.
> VS2013 扩展自 **0.9.0** 起才存在；更早的版本仅提供 VS Code 扩展。

---

## 0.9.0

### VS Code extension / VS Code 扩展

- 中文：扩展 ID 更名为 `align_symbols_vscode`（安装后在 VS Code 中显示的扩展名不变）；调用参数为链式下标（`name[s1][s2]`）时，在首个 `[` 处拆分，使名字补齐以对齐第一个下标，其余下标保持粘连（例如 `ch_kelvin[site_no][0]`）。
- English: The extension id was renamed to `align_symbols_vscode` (the display name shown in VS Code is unchanged). Call arguments with chained subscripts (`name[s1][s2]`) are now split at the first `[`, so the name is padded to align the first subscript while the rest of the bracket chain stays glued (e.g. `ch_kelvin[site_no][0]`).

### VS2013 extension / VS2013 扩展

- 中文：首个与 VS Code 同版本的 **0.9.0** 版本；命令文本由 `AlignSelectedCode` 改为 **`Align Selected Code`**（增加空格提升可读性，版本号不升级）。
- English: First **0.9.0** release aligned with the VS Code version; the command text was changed from `AlignSelectedCode` to **`Align Selected Code`** (spaces added for readability, version not bumped).

## 0.8.0

### VS Code extension / VS Code 扩展

- 中文：调用参数本身为 `name[sub]`（如 `vinp_kelvin[site_no]`）时，只填充名字使内部 `[sub]` 对齐；普通参数保持按单元对齐。
- English: Call arguments that are themselves `name[sub]` (e.g. `vinp_kelvin[site_no]`) now pad only the name so the inner `[sub]` aligns; plain arguments stay cell-aligned.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.7.0

### VS Code extension / VS Code 扩展

- 中文：嵌套访问链现在也把 `[` 当作访问符，因此数组赋值 `global_x[a][b] = local_x[b];` 会对齐每个 `[` 前的标识符（保持括号链粘连）。链式下标不再被简单的 `member[sub] = value` 形式误处理。
- English: The nested-access chain now also treats `[` as an accessor, so array assignments like `global_x[a][b] = local_x[b];` align the identifier before each `[` (keeping the bracket chain glued). Chained subscripts are no longer mis-handled by the simple `member[sub] = value` form.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.6.0

### VS Code extension / VS Code 扩展

- 中文：调用对齐现在识别前置赋值（`LHS = Func(args);`），同时对齐 `=` 与调用，使诸如 `CParam *x = StsGetParam(...)` 的声明行把 `=` 推到同一列。
- English: Call alignment now recognises a leading assignment (`LHS = Func(args);`) and aligns the "=" as well as the call, so declaration lines like `CParam *x = StsGetParam(...)` get the "=" pushed to a common column.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.5.0

### VS Code extension / VS Code 扩展

- 中文：新增通用“同构”对齐器：任意连续若干行若共享同一符号骨架，则逐列对齐（所有符号/运算符），只插入空格。在专用模式之后作为回退。
- English: Added a general "same-shape" aligner: any run of consecutive lines that share the same symbol skeleton is aligned column-by-column (all symbols/operators), inserting whitespace only. Falls back after the specialised patterns.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.4.0

### VS Code extension / VS Code 扩展

- 中文：新增嵌套成员访问调用链：`OBJ1->Method1(a, b, OBJ2.Method2(c, d));`，按序数对齐每个接收者（`->` / `.` 前的标识符）。
- English: Added nested member-access call chains: `OBJ1->Method1(a, b, OBJ2.Method2(c, d));`, aligning every receiver (identifier before `->` / `.`) per its ordinal.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.3.0

### VS Code extension / VS Code 扩展

- 中文：新增方法/函数调用表支持：`obj.Func(arg1, arg2, ...);`，对齐对象名与各参数列。
- English: Added support for method/function call tables: `obj.Func(arg1, arg2, ...);`, aligning the object name and each argument column.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.2.0

### VS Code extension / VS Code 扩展

- 中文：通用化对齐引擎：支持 `member[sub] = value; ...` 表（可带前导块注释）、更多运算符（`|=`、`&=`、`+=`、`...`）以及用 `|` 分隔的位运算和。
- English: Generalised alignment engine: also supports `member[sub] = value; ...` tables (with an optional leading block comment) and additional operators (`|=`, `&=`, `+=`, `...`), plus `|`-separated bitfield sums.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).

## 0.1.0

### VS Code extension / VS Code 扩展

- 中文：初始发布：链式 bitfield 赋值按列对齐。
- English: Initial release: per-column symbol alignment for chained bitfield assignments.

### VS2013 extension / VS2013 扩展

- 尚未发布（仅 VS Code 版）。 / Not released yet (VS Code edition only).
