# Symbol Align (by column)

Aligns blocks of similar C/C++-style lines by symbol / column so that `=`, `(`, `)`,
`[`, `]`, `.`, `->`, `<<`, `+`, `|`, `,`, `;` line up on shared columns, turning the
block into a readable table.

It **only ever inserts whitespace** — it never reorders, renames or changes any token —
and it only touches **structure-identical consecutive lines**, so unrelated code
(control flow, function headers, mixed shapes) is left untouched.

## What it recognises

The specialised patterns below are tried first, then a general fallback catches any
other run of structure-identical consecutive lines.

### A) bitfield / shift-sum

`LHS = LHS = ... = (member[sub] << shift) + (member[sub] << shift) ... ;`

```c
T       = (x.k[site_no] << 3) | (y.abc[site_no] << 1);
LONGEST = (x.k[site_no] << 2) | (y.abc[site_no] << 0);
```

`=`, the `(` / member / `[` `]` / `<<` / `shift` and the `+` / `|` separators all align.

### B) default / value table

`[/* address */] member[sub] OP value; member[sub] OP value; ... ;` (OP can be `=`,
`|=`, `&=`, `+=`, `<<=`, …)

```c
/* 0x39 */ dflt.trim_key      [site_no] = 10; dflt.boost_ocp_trim[site_no] = 00;
/* 0x3A */ dflt.com_prebg_trim[site_no] =  0; dflt.com_bg_trim   [site_no] =  2;
```

The member is padded so `[` `]` line up; the operator is aligned; **numeric values are
right-aligned** so `;` lines up.

### C) method / function call (with optional leading assignment)

`[LHS =] obj.Func(arg1, arg2, ...);`

```c
CParam *LX_Leak      = StsGetParam(funcindex, "LX_Leak"     );
CParam *VIN_LED_Leak = StsGetParam(funcindex, "VIN_LED_Leak");
```

```c
fxvi_SDA.TSet(FV, 0, FXVIe_10V , FXVIe_10MA , FXVIe_RELAY_ON );
acm_CH1 .TSet(FV, 0, ACM200_10V, ACM200_10MA, ACM200_RELAY_ON);
```

The leading `LHS =` (if any) is aligned, the object is padded so `.` / `->` and
the call `(` align, and the arguments are aligned (numeric arguments right-aligned).

### D) nested member-access / array chain

`OBJ1->Method1(a, b, OBJ2.Method2(c, d));`

```c
PIN30_LX   ->SetTestResult(site_no, 0, fxvi_LX  .GetMeasResult(site_no, MVRET));
PIN14_RESET->SetTestResult(site_no, 0, acm_RESET.GetMeasResult(site_no, MVRET));
```

Every identifier before a `->` / `.` is padded per its ordinal, so both the outer
`->` and the inner `.` align. `[` is also treated as an accessor, so array
assignments align the identifier before each `[` while keeping the bracket chain glued:

```c
global_lx_leak     [leak_flag][site_no] = lx_leak     [site_no];
global_vin_led_leak[leak_flag][site_no] = vin_led_leak[site_no];
```

### General fallback

Any run of consecutive lines with an identical **symbol skeleton** (same operators and
punctuators in the same order) is aligned column-by-column. Identifiers are left-aligned,
**numeric literals are right-aligned**, and `.` / `->` / `(` / `[` / `]` / `::` stay glued
to their neighbour while `=` / `+` / `,` / `;` / `)` get a space around them.

```c
int a   =   1 ;
int bb  =  22 ;
int ccc = 333 ;
```

## How it aligns

1. Each column is aligned to the widest token at that position (per cell / per row).
2. For member / accessor columns the identifier is padded so the following `.`, `->`,
   `[`, `(` or operator lines up.
3. Numeric operands are right-aligned so `)` / `;` line up.
4. Shorter rows never gain trailing whitespace beyond what aligns a separator; a row
   with N cells snaps its trailing `;` to the N-th separator of the longest row.
5. Only whitespace is inserted; tokens (identifiers, numbers, strings, operators) are
   unchanged.

## Usage

- Command Palette: **Symbol Align (by column)** → command `alignSymbols.align`
- Keybinding: `Ctrl+Alt+A`
- Right-click → **Symbol Align (by column)**

Select the lines you want to align; if nothing is selected the whole file is used.

## Install (from .vsix)

```bash
code --install-extension align-symbols-0.6.0.vsix
```

Then reload the window (Command Palette → "Developer: Reload Window") and press
`Ctrl+Alt+A` in a C/C++ file.

## Dev / build

The extension is plain CommonJS (no build step required).

```bash
# package a new .vsix
vsce package
```

## License

MIT
