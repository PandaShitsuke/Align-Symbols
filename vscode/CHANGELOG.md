# Changelog

## 0.9.0

- Call arguments with chained subscripts (`name[s1][s2]`) are now split at the first
  `[`, so the name is padded to align the first subscript while the rest of the
  bracket chain stays glued (e.g. `ch_kelvin[site_no][0]`).

## 0.8.0

- Call arguments that are themselves `name[sub]` (e.g. `vinp_kelvin[site_no]`) now
  pad only the name so the inner `[sub]` aligns; plain arguments stay cell-aligned.

## 0.7.0

- The nested-access chain now also treats `[` as an accessor, so array
  assignments like `global_x[a][b] = local_x[b];` align the identifier before each
  `[` (keeping the bracket chain glued). Chained subscripts are no longer mis-handled
  by the simple `member[sub] = value` form.

## 0.6.0

- Call alignment now recognises a leading assignment (`LHS = Func(args);`) and
  aligns the "=" as well as the call, so declaration lines like
  `CParam *x = StsGetParam(...)` get the "=" pushed to a common column.

## 0.5.0

- Added a general "same-shape" aligner: any run of consecutive lines that share
  the same symbol skeleton is aligned column-by-column (all symbols/operators),
  inserting whitespace only. Falls back after the specialised patterns.

## 0.4.0

- Added nested member-access call chains: `OBJ1->Method1(a, b, OBJ2.Method2(c, d));`
  aligns every receiver (identifier before `->` / `.`) per its ordinal.

## 0.3.0

- Added support for method/function call tables: `obj.Func(arg1, arg2, ...);`
  with the object name aligned and each argument column aligned.

## 0.2.0

- Generalised alignment engine: also supports `member[sub] = value; ...` tables
  (with an optional leading block comment) and additional operators (`|=`, `&=`,
  `+=`, `...`), plus `|`-separated bitfield sums.

## 0.1.0

- Initial release: per-column symbol alignment for chained bitfield assignments.
