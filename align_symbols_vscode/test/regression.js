'use strict';

const fs = require('fs');
const path = require('path');
const { alignText } = require('../aligner.js');

const casesDir = path.join(__dirname, 'cases');
const norm = (s) =>
  s
    .replace(/\r/g, '')
    .split('\n')
    .map((l) => l.replace(/\s+$/, ''))
    .join('\n')
    .trim();

let failed = 0;
const sources = fs.readdirSync(casesDir).filter((f) => f.endsWith('_src.txt')).sort();
for (const f of sources) {
  const base = f.replace(/_src\.txt$/, '');
  const expectedFile = path.join(casesDir, base + '_expected.txt');
  if (!fs.existsSync(expectedFile)) {
    console.log(base + ': MISSING expected');
    failed++;
    continue;
  }
  const src = fs.readFileSync(path.join(casesDir, f), 'utf8');
  const expected = fs.readFileSync(expectedFile, 'utf8');
  const got = alignText(src);
  const ok = norm(got) === norm(expected);
  console.log(base + ': ' + (ok ? 'PASS' : 'FAIL'));
  if (!ok) failed++;
}

console.log(failed === 0 ? '\nAll cases passed.' : '\n' + failed + ' case(s) failed.');
process.exit(failed === 0 ? 0 : 1);
