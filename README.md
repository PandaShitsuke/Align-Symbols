# Symbol Align (by column)

> 按符号分列对齐的 VS Code 扩展 · A VS Code extension that aligns similar C/C++ lines by symbol and column.

**Symbol Align** turns blocks of consecutive, structurally similar C/C++-style lines
into a readable table by pushing `=`, `(`, `)`, `[`, `]`, `.`, `->`, `<<`, `+`, `|`,
`,`, `;` onto shared columns. It only **inserts whitespace** — it never reorders,
renames or changes any token — so semantics are unchanged, and it only touches
structure-identical consecutive lines.

---

## Features / 功能

### English

- **Bitfield / shift-sum** — `LHS = LHS = ... = (member[sub] << shift) + ... ;`
  aligns the `=`, the `( member [sub] << shift )` cells, and the `+` / `|` separators.
- **Default / value table** — `[/* address */] member[sub] OP value; member[sub] OP value; ... ;`
  aligns each member so `[` `]` line up, and right-aligns numeric values.
- **Method / function call** — `[LHS =] obj.Func(arg1, arg2, ...);` aligns a leading
  `LHS =`, the object (so `.` / `->` / `(` align) and the arguments.
- **Nested member-access chain** — `OBJ1->Method1(a, b, OBJ2.Method2(c, d));` aligns every
  identifier before a `->` / `.`, and `[` is also treated as an accessor so array
  assignments like `global_x[a][b] = local_x[b];` align the bracket columns.
- **General fallback** — any run of consecutive lines with an identical symbol skeleton is
  aligned column-by-column; identifiers are left-aligned and **numeric literals are
  right-aligned**.
- Attached operators (`.`, `->`, `(`, `[`, `]`, `::`) stay glued to their neighbour;
  `=`, `+`, `,`, `;`, `)` get a space around them.
- Non-matching lines (control flow, function headers, mixed shapes) are left untouched.
- Only whitespace is inserted — never any token change.

### 中文

- **位段 / 移位求和** —— `LHS = LHS = ... = (member[sub] << shift) + ... ;`，对齐 `=`、
  `( member [sub] << shift )` 各列以及 `+` / `|` 分隔符。
- **默认值 / 寄存器表** —— `[/* 地址 */] member[sub] OP value; member[sub] OP value; ... ;`，
  让每个 `member` 补齐使 `[` `]` 成列，并让数值右对齐。
- **方法 / 函数调用** —— `[LHS =] obj.Func(arg1, arg2, ...);`，对齐前导的 `LHS =`、对象名
  （使 `.` / `->` / `(` 成列）以及各实参。
- **嵌套成员访问链** —— `OBJ1->Method1(a, b, OBJ2.Method2(c, d));`，按序对齐每个 `->` / `.`
  前的标识符；同时把 `[` 也当作访问符，因此 `global_x[a][b] = local_x[b];` 这类数组赋值
  也会对齐下标列。
- **通用兜底** —— 任何“符号骨架相同”的连续行都会按列对齐；标识符左对齐，**纯数字右对齐**。
- 紧贴运算符（`.`、`->`、`(`、`[`、`]`、`::`）与前词之间不留空格；`=`、`+`、`,`、`;`、`)`
  前后留一个空格。
- 不匹配的行（控制流、函数头、混合结构）保持原样。
- 只插入**空格**，绝不改动任何 token。

---

## Install / 安装

### From a `.vsix` package / 从 .vsix 包安装

```bash
code --install-extension release/align-symbols-0.9.0.vsix
```

Then reload the window (Command Palette → **Developer: Reload Window**) and press
`Ctrl+Alt+A`.

装好后按 `Ctrl+Shift+P` 执行 **Developer: Reload Window** 重载，然后按 `Ctrl+Alt+A`。

### From source (development) / 从源码（开发模式）

Open this folder in VS Code and press `F5` (or run the **Extension Development Host**),
or copy the folder into `%USERPROFILE%\.vscode\extensions\`.

在 VS Code 打开本目录按 `F5`（或运行 Extension Development Host），或把整个目录复制到
`%USERPROFILE%\.vscode\extensions\` 下。

---

## Usage / 使用

- Command Palette: **Symbol Align (by column)** → command `alignSymbols.align`
- Keybinding: `Ctrl+Alt+A`
- Right-click editor menu → **Symbol Align (by column)**

Select the lines you want to align; if nothing is selected, the whole file is used.

- 命令面板：**Symbol Align (by column)** → 命令 `alignSymbols.align`
- 快捷键：`Ctrl+Alt+A`
- 编辑器右键菜单 → **Symbol Align (by column)**

选中要对齐的行；未选中则对齐整个文件。

### Examples / 示例

```c
// before
CParam *LX_Leak = StsGetParam(funcindex, "LX_Leak");
CParam *VIN_LED_Leak = StsGetParam(funcindex, "VIN_LED_Leak");

// after
CParam *LX_Leak      = StsGetParam(funcindex, "LX_Leak"     );
CParam *VIN_LED_Leak = StsGetParam(funcindex, "VIN_LED_Leak");
```

```c
// before
int a = 1;
int bb = 22;
int ccc = 333;

// after
int a   =   1 ;
int bb  =  22 ;
int ccc = 333 ;
```

---

## How it aligns / 对齐规则

1. Each column is aligned to the widest token at that position (per cell / per row).
2. For member / accessor columns the identifier is padded so the following `.`, `->`,
   `[`, `(` or operator lines up.
3. Numeric operands are right-aligned so `)` / `;` line up.
4. A row with N cells that is shorter than the longest row has its trailing `;` snapped
   to the N-th separator of the longest row, so short rows gain no trailing whitespace.
5. Only whitespace is inserted; tokens are unchanged.

1. 每一列对齐到该列最宽的 token（按单元格 / 按行）。
2. 成员/访问符列：标识符补齐，使其后的 `.`、`->`、`[`、`(` 或运算符对齐。
3. 数字操作数右对齐，使 `)` / `;` 成列。
4. 短行（单元格更少的行）会把它末尾的 `;` 对齐到最长行对应的分隔符列，因此短行不会产生尾部空白。
5. 只插入空格，token 不变。

---

## Project structure / 项目结构

```
align-symbols/
├─ package.json        # VS Code extension manifest
├─ extension.js        # extension entry (command registration)
├─ aligner.js          # alignment engine (core logic)
├─ README.md           # this file
├─ CHANGELOG.md        # release notes
├─ LICENSE             # MIT
├─ .gitignore          # ignores node_modules / *.vsix
├─ .gitattributes      # LF line endings
├─ release/            # built .vsix packages
├─ scripts/
│  └─ build_vsix.py    # builds the .vsix from root source into release/
└─ test/
   ├─ regression.js    # runs the alignment regression suite
   └─ cases/           # <name>_src.txt inputs + <name>_expected.txt outputs
```

---

## Development / 开发

```bash
# build the .vsix (writes release/align-symbols-<version>.vsix)
python scripts/build_vsix.py

# run the regression suite (8 cases)
node test/regression.js
```

The extension is plain CommonJS — there is no compiler / bundler step; edits to
`aligner.js` / `extension.js` take effect after reloading the debug host.

（扩展为纯 CommonJS，无编译/打包步骤；改动 `aligner.js` / `extension.js` 后重载调试宿主即可。）

---

## License / 许可

MIT — see [LICENSE](LICENSE).
