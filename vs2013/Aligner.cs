using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PandaShitsuke.AlignSymbols
{
    /// <summary>
    /// Port of the align-symbols (by column) engine. Aligns blocks of similar
    /// C/C++ lines (bitfield shift-sums, member=value tables, method calls,
    /// nested member-access chains, and general same-shape runs) by pushing
    /// symbols onto shared columns. Only inserts whitespace.
    /// </summary>
    public static class Aligner
    {
        private static readonly HashSet<string> NoSpaceBefore =
            new HashSet<string> { "(", "[", ".", "->", "::", "]", "++", "--" };
        private static readonly HashSet<string> NoSpaceAfter =
            new HashSet<string> { "(", "[", ".", "->", "::", "++", "--" };

        private static readonly string[] SymList =
        {
            "<<=", ">>=", "...", "->", "===", "!==", "<<", ">>", "<=", ">=", "==", "!=",
            "&&", "||", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
            "=", "+", "-", "*", "/", "%", "&", "|", "^", "!", "~", "<", ">", "(", ")",
            "[", "]", "{", "}", ";", ",", ".", ":", "?", "#", "@", "$"
        };

        private static readonly Regex RowRe = new Regex(@"^(\s*)(.*?);\s*$", RegexOptions.Singleline);
        private static readonly Regex ReceiverRe = new Regex(
            @"([A-Za-z_$][\w$]*)\s*((?:->|\.)(?=\s*[A-Za-z_$])|\[(?=\s*[^\]\s]))", RegexOptions.Compiled);

        public static string AlignText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string eol = text.Contains("\r\n") ? "\r\n" : "\n";
            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var sb = new StringBuilder();
            int i = 0;
            while (i < lines.Length)
            {
                var r = ParseRow(lines[i]);
                if (r != null)
                {
                    var group = new List<Row>();
                    int j = i;
                    while (j < lines.Length)
                    {
                        var rr = ParseRow(lines[j]);
                        if (rr != null && rr.Type == r.Type) { group.Add(rr); j++; }
                        else break;
                    }
                    sb.Append(string.Join(eol, AlignRows(group)));
                    i = j;
                }
                else
                {
                    var run = new List<string>();
                    int j = i;
                    while (j < lines.Length && ParseRow(lines[j]) == null) { run.Add(lines[j]); j++; }
                    sb.Append(string.Join(eol, AlignGeneralRun(run)));
                    i = j;
                }
            }
            return sb.ToString();
        }

        // ----- parsing ----------------------------------------------------

        private class Row
        {
            public string Type, Indent, Text, Sep, HeaderSep, Obj, Method, Lhs, Prefix;
            public List<string> LhsParts;
            public List<Cell> Cells;
            public List<Receiver> Receivers;
        }
        private class Cell { public string Member, Sub, Chain, Op, Operand; }
        private class Receiver { public string Id, Op; }

        private static Row ParseRow(string line)
        {
            var m = RowRe.Match(line);
            if (!m.Success || m.Groups[2].Value.Trim().Length == 0) return null;
            string indent = m.Groups[1].Value;
            string body = m.Groups[2].Value;
            var bit = ParseBitRow(body, indent);
            if (bit != null) return bit;
            var asg = ParseAssignRow(body, indent);
            if (asg != null) return asg;
            var call = ParseCallRow(body, indent);
            if (call != null) return call;
            var chain = ParseChainRow(body, indent);
            if (chain != null) return chain;
            return null;
        }

        private static Row ParseBitRow(string body, string indent)
        {
            string text = body.Trim();
            int paren = text.IndexOf('(');
            if (paren < 0) return null;
            string expr = text.Substring(paren).Trim();
            string pre = text.Substring(0, paren).Trim();
            int lastEq = pre.LastIndexOf('=');
            if (lastEq < 0) return null;
            string lhs = pre.Substring(0, lastEq).Trim();
            var lhsParts = lhs.Split(new[] { " = " }, StringSplitOptions.None)
                .Select(s => s.Trim()).ToList();
            if (lhsParts.Count == 0 || lhsParts.Any(s => s.Length == 0)) return null;

            var cells = new List<Cell>();
            foreach (Match mm in Regex.Matches(expr, @"\(([^()]*?)\)"))
            {
                var tm = Regex.Match(mm.Groups[1].Value,
                    @"^\s*(.*?)\s*\[\s*([^\]]*)\s*\]\s*<<\s*([^\s()]+)\s*$");
                if (!tm.Success || tm.Groups[1].Value.Trim().Length == 0) return null;
                cells.Add(new Cell
                {
                    Member = tm.Groups[1].Value.Trim(),
                    Sub = tm.Groups[2].Value,
                    Operand = tm.Groups[3].Value,
                    Op = "<<"
                });
            }
            if (cells.Count == 0) return null;

            var gaps = Regex.Split(expr, @"\([^()]*?\)");
            if (gaps.Length != cells.Count + 1) return null;
            if (gaps[0].Trim() != "" || gaps[cells.Count].Trim() != "") return null;
            string sep = cells.Count > 1 ? gaps[1].Trim() : "+";
            if (sep.Length == 0) return null;
            for (int k = 1; k < cells.Count; k++) if (gaps[k].Trim() != sep) return null;

            return new Row { Type = "bit", Indent = indent, LhsParts = lhsParts, Cells = cells, Sep = sep, HeaderSep = " = " };
        }

        private static Row ParseAssignRow(string body, string indent)
        {
            string text = body.Trim();
            string head = "", rest = text;
            var cm = Regex.Match(text, @"^(/\*.*?\*/)\s*(.*)$");
            if (cm.Success) { head = cm.Groups[1].Value; rest = cm.Groups[2].Value; }
            var cells = new List<Cell>();
            foreach (var raw in rest.Split(';'))
            {
                string p = raw.Trim();
                if (p.Length == 0) continue;
                var m = Regex.Match(p,
                    @"^(.*?)\[\s*([^\]]*)\s*\]\s*(=|\+=|-=|\*=|\/=|%=|&=|\|=|\^=|<<=|>>=)\s*([^;\s]+)\s*$");
                if (!m.Success || m.Groups[1].Value.Trim().Length == 0 || m.Groups[1].Value.Contains("[")) return null;
                cells.Add(new Cell
                {
                    Member = m.Groups[1].Value.Trim(),
                    Sub = m.Groups[2].Value,
                    Op = m.Groups[3].Value,
                    Operand = m.Groups[4].Value
                });
            }
            if (cells.Count == 0) return null;
            return new Row
            {
                Type = "assign", Indent = indent,
                LhsParts = head.Length > 0 ? new List<string> { head } : new List<string>(),
                Cells = cells, Sep = ";", HeaderSep = head.Length > 0 ? " " : ""
            };
        }

        private static Row ParseCallRow(string body, string indent)
        {
            string text = body.Trim();
            int open = text.IndexOf('(');
            if (open < 0) return null;
            int close = text.LastIndexOf(')');
            if (close < open) return null;
            if (text.Substring(close + 1).Trim() != "") return null;
            string callee = text.Substring(0, open).Trim();
            if (callee.Length == 0) return null;

            string lhs = null;
            int eq = callee.LastIndexOf('=');
            if (eq >= 0)
            {
                lhs = callee.Substring(0, eq).Trim();
                callee = callee.Substring(eq + 1).Trim();
                if (lhs.Length == 0 || callee.Length == 0) return null;
            }
            int dot = callee.LastIndexOf('.');
            int arrow = callee.LastIndexOf("->", StringComparison.Ordinal);
            string obj, method;
            if (arrow > dot) { obj = callee.Substring(0, arrow); method = callee.Substring(arrow); }
            else if (dot >= 0) { obj = callee.Substring(0, dot); method = callee.Substring(dot); }
            else { obj = callee; method = ""; }
            if (obj.Length == 0) return null;

            string inside = text.Substring(open + 1, close - open - 1);
            var args = inside.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
            if (args.Count == 0 || args.Any(a => a.IndexOfAny(new[] { '(', ')' }) >= 0)) return null;
            var cells = args.Select(a => {
                int i = a.IndexOf('[');
                return i >= 0
                    ? new Cell { Member = a.Substring(0, i).Trim(), Chain = a.Substring(i) }
                    : new Cell { Member = a, Chain = "" };
            }).ToList();
            return new Row { Type = "call", Indent = indent, Lhs = lhs, Obj = obj, Method = method, Cells = cells, Sep = "," };
        }

        private static Row ParseChainRow(string body, string indent)
        {
            string text = body.Trim();
            if (text.IndexOf('(') < 0) return null;
            var receivers = new List<Receiver>();
            foreach (Match m in ReceiverRe.Matches(text))
                receivers.Add(new Receiver { Id = m.Groups[1].Value, Op = m.Groups[2].Value });
            if (receivers.Count < 2) return null;
            return new Row { Type = "chain", Indent = indent, Text = text, Receivers = receivers };
        }

        // ----- alignment --------------------------------------------------

        private static List<string> AlignRows(List<Row> rows)
        {
            string type = rows[0].Type;
            if (type == "call") return AlignCallRows(rows);
            if (type == "chain") return AlignChainRows(rows);

            int maxLhs = rows.Max(r => r.LhsParts.Count);
            var lhsW = new int[maxLhs];
            for (int i = 0; i < maxLhs; i++)
                lhsW[i] = rows.Where(r => r.LhsParts.Count > i).Max(r => r.LhsParts[i].Length);

            int maxCells = rows.Max(r => r.Cells.Count);
            var memberW = new int[maxCells];
            var opW = new int[maxCells];
            for (int pos = 0; pos < maxCells; pos++)
            {
                memberW[pos] = rows.Where(r => r.Cells.Count > pos).Max(r => r.Cells[pos].Member.Length);
                opW[pos] = rows.Where(r => r.Cells.Count > pos)
                    .Max(r => (r.Cells[pos].Operand ?? "").Length);
            }

            Func<Row, string> buildCore = (r) =>
            {
                var lhs = string.Join(" = ",
                    Enumerable.Range(0, r.LhsParts.Count).Select(i => r.LhsParts[i].PadRight(lhsW[i])));
                string expr;
                var parts = new List<string>();
                for (int c = 0; c < r.Cells.Count; c++)
                {
                    var cell = r.Cells[c];
                    if (type == "bit")
                        parts.Add("(" + cell.Member.PadRight(memberW[c]) + "[" + cell.Sub + "] << " +
                                  cell.Operand.PadLeft(opW[c]) + ")");
                    else
                        parts.Add(cell.Member.PadRight(memberW[c]) + "[" + cell.Sub + "] " + cell.Op + " " +
                                  cell.Operand.PadLeft(opW[c]));
                }
                expr = type == "bit"
                    ? string.Join(" " + r.Sep + " ", parts)
                    : string.Join(r.Sep + " ", parts);
                return r.Indent + lhs + r.HeaderSep + expr;
            };

            var maxRow = rows.First(r => r.Cells.Count == maxCells);
            string refCore = buildCore(maxRow);
            var sepRe = new Regex(Regex.Escape(type == "bit" ? "+" : ";"));
            var sepCols = new List<int>();
            foreach (Match mm in sepRe.Matches(refCore)) sepCols.Add(mm.Index);

            var result = new List<string>();
            foreach (var r in rows)
            {
                string core = buildCore(r);
                if (type == "bit" && r.Cells.Count < maxCells)
                {
                    int target = sepCols[r.Cells.Count - 1];
                    int pad = Math.Max(0, target - core.Length);
                    result.Add(core + new string(' ', pad) + ";");
                }
                else result.Add(core + ";");
            }
            return result;
        }

        private static List<string> AlignCallRows(List<Row> rows)
        {
            int objW = rows.Max(r => r.Obj.Length);
            bool hasLhs = rows.Any(r => r.Lhs != null);
            int lhsW = hasLhs ? rows.Max(r => (r.Lhs ?? "").Length) : 0;
            int maxArgs = rows.Max(r => r.Cells.Count);
            var argW = new int[maxArgs];
            for (int pos = 0; pos < maxArgs; pos++)
                argW[pos] = rows.Where(r => r.Cells.Count > pos).Max(r => r.Cells[pos].Member.Length);

            var result = new List<string>();
            foreach (var r in rows)
            {
                string head = hasLhs ? (r.Lhs != null ? r.Lhs.PadRight(lhsW) + " = " : "") : "";
                var parts = new List<string>();
                for (int i = 0; i < r.Cells.Count; i++)
                {
                    var c = r.Cells[i];
                    if (c.Chain.Length > 0) parts.Add(c.Member.PadRight(argW[i]) + c.Chain);
                    else parts.Add(IsNumeric(c.Member) ? c.Member.PadLeft(argW[i]) : c.Member.PadRight(argW[i]));
                }
                result.Add(r.Indent + head + r.Obj.PadRight(objW) + r.Method + "(" + string.Join(", ", parts) + ")" + ";");
            }
            return result;
        }

        private static List<string> AlignChainRows(List<Row> rows)
        {
            int maxRecv = rows.Max(r => r.Receivers.Count);
            var maxW = new int[maxRecv];
            for (int i = 0; i < maxRecv; i++)
                maxW[i] = rows.Where(r => r.Receivers.Count > i).Max(r => r.Receivers[i].Id.Length);

            var result = new List<string>();
            foreach (var r in rows)
            {
                var sb = new StringBuilder();
                int last = 0, ordinal = 0;
                foreach (Match m in ReceiverRe.Matches(r.Text))
                {
                    sb.Append(r.Text.Substring(last, m.Index - last));
                    sb.Append(m.Groups[1].Value.PadRight(maxW[ordinal]));
                    sb.Append(m.Groups[2].Value);
                    last = m.Index + m.Length;
                    ordinal++;
                }
                sb.Append(r.Text.Substring(last));
                result.Add(r.Indent + sb + ";");
            }
            return result;
        }

        // ----- general same-shape fallback --------------------------------

        private static List<string> AlignGeneralRun(List<string> run)
        {
            if (run.Count == 0) return run;
            var result = new List<string>();
            int i = 0;
            while (i < run.Count)
            {
                string line = run[i];
                var tokens = Tokenize(LineNoIndent(line));
                string sk = Skeleton(tokens) + "#" + tokens.Count;
                if (tokens.Count == 0) { result.Add(line); i++; continue; }
                var group = new List<string>();
                int j = i;
                while (j < run.Count)
                {
                    var tj = Tokenize(LineNoIndent(run[j]));
                    if (Skeleton(tj) + "#" + tj.Count == sk) { group.Add(run[j]); j++; }
                    else break;
                }
                if (group.Count >= 2) result.AddRange(AlignSameShape(group));
                else result.Add(run[i]);
                i = j;
            }
            return result;
        }

        private static List<string> AlignSameShape(List<string> subRun)
        {
            var parsed = subRun.Select(ln =>
            {
                var m = Regex.Match(ln, @"^(\s*)(.*)$");
                return new { Indent = m.Groups[1].Value, Tok = Tokenize(m.Groups[2].Value) };
            }).ToList();
            int n = parsed[0].Tok.Count;
            var colW = new int[n];
            for (int c = 0; c < n; c++)
                colW[c] = parsed.Max(p => p.Tok[c].Text.Length);

            return parsed.Select(p =>
            {
                var sb = new StringBuilder(p.Indent);
                for (int c = 0; c < n; c++)
                {
                    var tok = p.Tok[c];
                    if (tok.IsSym) sb.Append(tok.Text);
                    else sb.Append(tok.IsNum ? tok.Text.PadLeft(colW[c]) : tok.Text.PadRight(colW[c]));
                    if (c < n - 1) sb.Append(new string(' ', Gap(p.Tok[c], p.Tok[c + 1])));
                }
                return sb.ToString();
            }).ToList();
        }

        private static int Gap(Token prev, Token cur)
        {
            if (NoSpaceBefore.Contains(cur.Text)) return 0;
            if (NoSpaceAfter.Contains(prev.Text)) return 0;
            return 1;
        }

        private class Token { public string Text; public bool IsSym, IsNum; }

        private static string LineNoIndent(string line) { return Regex.Replace(line, @"^\s+", ""); }

        private static string Skeleton(List<Token> tokens)
        {
            return string.Join("|", tokens.Where(t => t.IsSym).Select(t => t.Text));
        }

        private static List<Token> Tokenize(string line)
        {
            var toks = new List<Token>();
            int i = 0;
            while (i < line.Length)
            {
                char c = line[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                var wm = Regex.Match(line.Substring(i), @"^[A-Za-z_$][\w$]*");
                if (wm.Success) { toks.Add(new Token { Text = wm.Value }); i += wm.Value.Length; continue; }
                var nm = Regex.Match(line.Substring(i), @"^\d+(?:\.\d+)?");
                if (nm.Success) { toks.Add(new Token { Text = nm.Value, IsNum = true }); i += nm.Value.Length; continue; }
                if (c == '"' || c == '\'')
                {
                    int j = i + 1;
                    while (j < line.Length && line[j] != c) j++;
                    toks.Add(new Token { Text = line.Substring(i, j - i + 1) });
                    i = j + 1;
                    continue;
                }
                bool hit = false;
                foreach (var s in SymList)
                    if (line.Substring(i).StartsWith(s)) { toks.Add(new Token { Text = s, IsSym = true }); i += s.Length; hit = true; break; }
                if (hit) continue;
                toks.Add(new Token { Text = c.ToString() });
                i++;
            }
            return toks;
        }

        private static bool IsNumeric(string s)
        {
            return s.Length > 0 && char.IsDigit(s[0]);
        }
    }
}
