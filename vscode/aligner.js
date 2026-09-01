'use strict';

/*
 * Symbol Align (by column) - general
 *
 * Aligns blocks of "columnar" record/assignment lines by symbol and column so
 * that =, (, [, ], <<, +, ; line up per column position.
 *
 * Two line shapes are recognised (and merged in one engine):
 *
 *   A) bitfield shift-sum:
 *        LHS = LHS = ... = (member[sub] << shift) + (member[sub] << shift) ... ;
 *      e.g. Fuse_Data_Write[0x39][site_no] = Fuse_Data[site_no][0x39] = (curr.trim_key[site_no] << 4) + (curr.boost_ocp_trim[site_no] << 0);
 *
 *   B) default-value table (with an optional leading block comment for an address):
 *        [header comment] member[sub] = value; member[sub] = value; ... ;
 *      e.g. an address comment "0x39" followed by dflt.trim_key[site_no] = 10;
 *
 * Rules (same for both):
 *   1. *all* "=" in the leading lhs/header are pushed to a common column;
 *   2. for each *cell position* (1st, 2nd, ... member[...]) the member name is
 *      left-aligned to the widest member at that position, so "[" / "]" align;
 *   3. the operand after the operator is right-aligned so ")" / ";" align;
 *   4. a line with N cells that is shorter than the longest line has its
 *      trailing ";" snapped to the N-th separator ("+") of the longest line, so
 *      no trailing whitespace is added for short lines.
 *
 * Lines that do not match either shape are left untouched.
 */

// --- per-line parsing -----------------------------------------------------

function parseRow(line) {
  const m = /^(\s*)(.*?);\s*$/.exec(line);
  if (!m || m[2].trim().length === 0) return null;
  const indent = m[1];
  const body = m[2];
  const bit = parseBitRow(body, indent);
  if (bit) return bit;
  const asg = parseAssignRow(body, indent);
  if (asg) return asg;
  const call = parseCallRow(body, indent);
  if (call) return call;
  const chain = parseChainRow(body, indent);
  if (chain) return chain;
  return null;
}

// Form D: nested member-access call chain, e.g.
//   OBJ1->Method1(a, b, OBJ2.Method2(c, d));
// Also handles array-assignments like  NAME1[...] ... = NAME2[...] ...;
// Aligns every "receiver" (the identifier before -> / . / [) per its ordinal so
// the accessor columns line up.
function parseChainRow(body, indent) {
  const text = body.trim();
  const re = RECEIVER_RE;
  let m;
  const receivers = [];
  re.lastIndex = 0;
  while ((m = re.exec(text)) !== null) receivers.push({ id: m[1], op: m[2] });
  if (receivers.length < 2) return null;
  return { type: 'chain', indent, text, receivers };
}

// Form C: OBJ.FUNC(arg1, arg2, ...);   or   FUNC(arg1, ...);
function parseCallRow(body, indent) {
  const text = body.trim();
  const open = text.indexOf('(');
  if (open < 0) return null;
  const close = text.lastIndexOf(')');
  if (close < open) return null;
  const after = text.slice(close + 1).trim();
  if (after !== '') return null;
  let callee = text.slice(0, open).trim();
  if (!callee) return null;

  // Optional leading assignment: "LHS = CALLEE(...)" -> align "=" too.
  let lhs = null;
  const eq = callee.lastIndexOf('=');
  if (eq >= 0) {
    lhs = callee.slice(0, eq).trim();
    callee = callee.slice(eq + 1).trim();
    if (!lhs || !callee) return null;
  }

  // Split callee into object + method for `.` / `->` alignment.
  const dot = callee.lastIndexOf('.');
  const arrow = callee.lastIndexOf('->');
  let obj, method;
  if (arrow > dot) {
    obj = callee.slice(0, arrow);
    method = callee.slice(arrow);
  } else if (dot >= 0) {
    obj = callee.slice(0, dot);
    method = callee.slice(dot);
  } else {
    obj = callee;
    method = '';
  }
  if (!obj) return null;

  const inside = text.slice(open + 1, close);
  const args = inside.split(',').map((s) => s.trim()).filter((s) => s.length > 0);
  if (args.length === 0) return null;
  // keep it simple: reject nested calls / parens inside arguments
  if (args.some((a) => /[()]/.test(a))) return null;
  const cells = args.map((a) => {
    const i = a.indexOf('[');
    if (i >= 0) return { member: a.slice(0, i).trim(), chain: a.slice(i) };
    return { member: a, chain: '' };
  });
  return { type: 'call', indent, lhs, obj, method, cells, sep: ',' };
}

// Form A: LHS = LHS = (member[sub] << shift) sep (member[sub] << shift) ...
function parseBitRow(body, indent) {
  const text = body.trim();
  const paren = text.indexOf('(');
  if (paren < 0) return null;
  const expr = text.slice(paren).trim();

  // The part before "(" ends with the final "= <sp>" that ties the lhs to the
  // expression; drop that operator so the chained "=" can be re-derived.
  const pre = text.slice(0, paren).trim();
  const lastEq = pre.lastIndexOf('=');
  if (lastEq < 0) return null;
  const lhs = pre.slice(0, lastEq).trim();
  const lhsParts = lhs.split(' = ').map((s) => s.trim());
  if (lhsParts.length === 0 || lhsParts.some((s) => s.length === 0)) return null;

  const cells = [];
  const raws = [];
  const termRe = /\(([^()]*?)\)/g;
  let hit;
  while ((hit = termRe.exec(expr)) !== null) {
    const tm = /^\s*(.*?)\s*\[\s*([^\]]*)\s*\]\s*<<\s*([^\s()]+)\s*$/.exec(hit[1]);
    if (!tm || tm[1].trim().length === 0) return null;
    cells.push({ member: tm[1].trim(), sub: tm[2], operand: tm[3] });
    raws.push(hit[0]);
  }
  if (cells.length === 0) return null;

  const gaps = expr.split(/\([^()]*?\)/);
  if (gaps.length !== cells.length + 1) return null;
  if (gaps[0].trim() !== '' || gaps[cells.length].trim() !== '') return null;
  const sep = cells.length > 1 ? gaps[1].trim() : '+';
  if (!sep) return null;
  for (let i = 1; i < cells.length; i++) {
    if (gaps[i].trim() !== sep) return null;
  }
  return { type: 'bit', indent, lhsParts, cells, sep, headerSep: ' = ' };
}

// Form B: [/* address */] member[sub] OP value; member[sub] OP value; ...
function parseAssignRow(body, indent) {
  const text = body.trim();
  let head = '';
  let rest = text;
  const cm = /^(\/\*.*?\*\/)\s*(.*)$/.exec(text);
  if (cm) {
    head = cm[1];
    rest = cm[2];
  }
  const cells = [];
  for (const raw of rest.split(';')) {
    const p = raw.trim();
    if (p.length === 0) continue;
    const m = /^(.*?)\[\s*([^\]]*)\s*\]\s*(=|\+=|-=|\*=|\/=|%=|&=|\|=|\^=|<<=|>>=)\s*([^;\s]+)\s*$/.exec(p);
    // A chained subscript (e.g. x[a][b] = y) is not a simple "member[sub] = value"
    // row; let the chain form align the accessors instead.
    if (!m || m[1].trim().length === 0 || m[1].indexOf('[') >= 0) return null;
    cells.push({ member: m[1].trim(), sub: m[2], op: m[3], operand: m[4] });
  }
  if (cells.length === 0) return null;
  return { type: 'assign', indent, lhsParts: head ? [head] : [], cells, sep: ';', headerSep: head ? ' ' : '' };
}

// --- alignment ------------------------------------------------------------

function alignRows(rows) {
  const type = rows[0].type;
  if (type === 'call') return alignCallRows(rows);
  if (type === 'chain') return alignChainRows(rows);
  const maxLhs = Math.max.apply(null, rows.map((r) => r.lhsParts.length));
  const lhsW = [];
  for (let i = 0; i < maxLhs; i++) {
    lhsW.push(
      Math.max.apply(
        null,
        rows.filter((r) => r.lhsParts.length > i).map((r) => r.lhsParts[i].length)
      )
    );
  }

  const maxCells = Math.max.apply(null, rows.map((r) => r.cells.length));
  const memberW = [];
  const opW = [];
  for (let pos = 0; pos < maxCells; pos++) {
    const items = rows.filter((r) => r.cells.length > pos).map((r) => r.cells[pos]);
    memberW.push(Math.max.apply(null, items.map((c) => c.member.length)));
    opW.push(Math.max.apply(null, items.map((c) => c.operand.length)));
  }

  const renderCell = (c, pos) => {
    if (type === 'bit') {
      return '(' + c.member.padEnd(memberW[pos]) + '[' + c.sub + ']' + ' << ' + c.operand.padStart(opW[pos]) + ')';
    }
    return c.member.padEnd(memberW[pos]) + '[' + c.sub + ']' + ' ' + c.op + ' ' + c.operand.padStart(opW[pos]);
  };

  const buildCore = (r) => {
    const lhs = r.lhsParts.map((s, i) => s.padEnd(lhsW[i])).join(' = ');
    let expr;
    if (type === 'bit') {
      expr = r.cells.map((c, i) => renderCell(c, i)).join(' ' + r.sep + ' ');
    } else {
      expr = r.cells.map((c, i) => renderCell(c, i)).join(r.sep + ' ');
    }
    return r.indent + lhs + r.headerSep + expr;
  };

  const maxRow = rows.find((r) => r.cells.length === maxCells);
  const refCore = buildCore(maxRow);
  const sepRe = type === 'bit' ? /\+/g : /;/g;
  const sepCols = [];
  let mm;
  while ((mm = sepRe.exec(refCore)) !== null) sepCols.push(mm.index);

  return rows.map((r) => {
    const core = buildCore(r);
    if (type === 'bit' && r.cells.length < maxCells) {
      const target = sepCols[r.cells.length - 1];
      const pad = Math.max(0, target - core.length);
      return core + ' '.repeat(pad) + ';';
    }
    return core + ';';
  });
}

function alignCallRows(rows) {
  const objW = Math.max.apply(null, rows.map((r) => r.obj.length));
  const hasLhs = rows.some((r) => r.lhs !== null);
  const lhsW = hasLhs ? Math.max.apply(null, rows.map((r) => (r.lhs ? r.lhs.length : 0))) : 0;
  const maxArgs = Math.max.apply(null, rows.map((r) => r.cells.length));
  const argW = [];
  for (let pos = 0; pos < maxArgs; pos++) {
    argW.push(
      Math.max.apply(
        null,
        rows.filter((r) => r.cells.length > pos).map((r) => r.cells[pos].member.length)
      )
    );
  }
  const buildCore = (r) => {
    const head = hasLhs ? (r.lhs !== null ? r.lhs.padEnd(lhsW) + ' = ' : '') : '';
    const args = r.cells
      .map((c, i) => {
        if (c.chain) return c.member.padEnd(argW[i]) + c.chain;
        return /^\d/.test(c.member) ? c.member.padStart(argW[i]) : c.member.padEnd(argW[i]);
      })
      .join(', ');
    return r.indent + head + r.obj.padEnd(objW) + r.method + '(' + args + ')';
  };
  return rows.map((r) => buildCore(r) + ';');
}

function alignChainRows(rows) {
  const maxRecv = Math.max.apply(null, rows.map((r) => r.receivers.length));
  const maxW = [];
  for (let i = 0; i < maxRecv; i++) {
    maxW.push(
      Math.max.apply(
        null,
        rows.filter((r) => r.receivers.length > i).map((r) => r.receivers[i].id.length)
      )
    );
  }
  const re = RECEIVER_RE;
  return rows.map((r) => {
    let out = '';
    let last = 0;
    let ordinal = 0;
    let m;
    re.lastIndex = 0;
    while ((m = re.exec(r.text)) !== null) {
      out += r.text.slice(last, m.index) + m[1].padEnd(maxW[ordinal]) + m[2];
      last = re.lastIndex;
      ordinal++;
    }
    out += r.text.slice(last);
    return r.indent + out + ';';
  });
}

// --- document level -------------------------------------------------------

// General "same-shape" token-column aligner.
// Only ever inserts spaces (never changes token content), so it is safe.
// Consecutive lines with an identical symbol skeleton are aligned column-wise:
// every column (word or symbol) is padded to the widest value at that column,
// and operators keep their usual spacing (`.` / `->` / `(` / `[` stay attached,
// `=` / `+` / `,` / `;` / `)` / `]` get a space around them as appropriate).

// Table-style spacing: attached operators (member access, calls, subscript)
// stay glued to their neighbour; separators / closers (; , ) ) get a space.
const NO_SPACE_BEFORE = new Set(['(', '[', '.', '->', '::', ']', '++', '--']);
const NO_SPACE_AFTER = new Set(['(', '[', '.', '->', '::', '++', '--']);

// Identifier immediately before a member / call accessor (`, ->` or `[`), used to
// align the accessor column by padding the receiver name.
const RECEIVER_RE = /([A-Za-z_$][\w$]*)\s*((?:->|\.)(?=\s*[A-Za-z_$])|\[(?=\s*[^\]\s]))/g;

const SYM_LIST = [
  '<<=', '>>=', '...', '->', '===', '!==', '<<', '>>', '<=', '>=', '==', '!=',
  '&&', '||', '++', '--', '+=', '-=', '*=', '/=', '%=', '&=', '|=', '^=',
  '=', '+', '-', '*', '/', '%', '&', '|', '^', '!', '~', '<', '>', '(', ')',
  '[', ']', '{', '}', ';', ',', '.', ':', '?', '#', '@', '$'
];

function tokenize(line) {
  const toks = [];
  let i = 0;
  while (i < line.length) {
    if (/\s/.test(line[i])) {
      i++;
      continue;
    }
    const w = /^[A-Za-z_$][\w$]*/.exec(line.slice(i));
    if (w) {
      toks.push({ type: 'w', text: w[0] });
      i += w[0].length;
      continue;
    }
    const n = /^\d+(?:\.\d+)?/.exec(line.slice(i));
    if (n) {
      toks.push({ type: 'w', text: n[0], num: true });
      i += n[0].length;
      continue;
    }
    if (line[i] === '"' || line[i] === "'") {
      const q = line[i];
      let j = i + 1;
      while (j < line.length && line[j] !== q) j++;
      toks.push({ type: 'w', text: line.slice(i, j + 1) });
      i = j + 1;
      continue;
    }
    let hit = false;
    for (const s of SYM_LIST) {
      if (line.startsWith(s, i)) {
        toks.push({ type: 's', text: s });
        i += s.length;
        hit = true;
        break;
      }
    }
    if (hit) continue;
    toks.push({ type: 'w', text: line[i] });
    i++;
  }
  return toks;
}

function skeletonOf(tokens) {
  return tokens.filter((t) => t.type === 's').map((t) => t.text).join('|');
}

function gapBetween(prev, cur) {
  if (NO_SPACE_BEFORE.has(cur.text)) return 0;
  if (NO_SPACE_AFTER.has(prev.text)) return 0;
  return 1;
}

// Align a sub-run of lines that all share one symbol skeleton.
function alignSameShape(subRun) {
  const parsed = subRun.map((ln) => {
    const m = /^(\s*)(.*)$/.exec(ln);
    return { indent: m[1], tok: tokenize(m[2]) };
  });
  const n = parsed[0].tok.length;
  const colW = [];
  for (let c = 0; c < n; c++) {
    colW.push(Math.max.apply(null, parsed.map((p) => p.tok[c].text.length)));
  }
  return parsed.map((p) => {
    let s = p.indent;
    for (let c = 0; c < n; c++) {
      const tok = p.tok[c];
      if (tok.type === 'w') {
        s += tok.num ? tok.text.padStart(colW[c]) : tok.text.padEnd(colW[c]);
      } else {
        s += tok.text;
      }
      if (c < n - 1) {
        const g = gapBetween(p.tok[c], p.tok[c + 1]);
        s += ' '.repeat(g);
      }
    }
    return s;
  });
}

// Group a run of non-special lines into same-skeleton sub-runs and align them.
function alignGeneralRun(run) {
  if (run.length === 0) return [];
  const out = [];
  let i = 0;
  while (i < run.length) {
    const line = run[i];
    const t = tokenize(line.replace(/^\s+/, ''));
    const sk = skeletonOf(t) + '#' + t.length;
    if (t.length === 0) {
      out.push(line);
      i++;
      continue;
    }
    let j = i;
    const group = [];
    while (j < run.length) {
      const tj = tokenize(run[j].replace(/^\s+/, ''));
      const sjk = skeletonOf(tj) + '#' + tj.length;
      if (sjk === sk && t.length > 0) {
        group.push(run[j]);
        j++;
      } else {
        break;
      }
    }
    if (group.length >= 2) {
      out.push.apply(out, alignSameShape(group));
    } else {
      out.push(run[i]);
    }
    i = j;
  }
  return out;
}

function alignText(text) {
  const eol = text.indexOf('\r\n') >= 0 ? '\r\n' : '\n';
  const lines = text.length === 0 ? [] : text.split(/\r\n|\n|\r/);
  const out = [];
  let i = 0;
  while (i < lines.length) {
    const r = parseRow(lines[i]);
    if (r) {
      const type = r.type;
      const group = [];
      let j = i;
      while (j < lines.length) {
        const rr = parseRow(lines[j]);
        if (rr && rr.type === type) {
          group.push(lines[j]);
          j++;
        } else {
          break;
        }
      }
      out.push.apply(out, alignRows(group.map(parseRow)));
      i = j;
    } else {
      const run = [];
      let j = i;
      while (j < lines.length && parseRow(lines[j]) === null) {
        run.push(lines[j]);
        j++;
      }
      out.push.apply(out, alignGeneralRun(run));
      i = j;
    }
  }
  return out.join(eol);
}

module.exports = { parseRow, alignRows, alignText };
