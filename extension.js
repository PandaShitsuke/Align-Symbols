'use strict';

const vscode = require('vscode');
const { alignText } = require('./aligner');

function activate(context) {
  const cmd = vscode.commands.registerCommand('alignSymbols.align', () => {
    const editor = vscode.window.activeTextEditor;
    if (!editor) {
      vscode.window.showInformationMessage('Symbol Align: no active editor.');
      return;
    }

    const doc = editor.document;
    const selection = editor.selection;
    const hasSelection = !selection.isEmpty;
    const range = hasSelection
      ? selection
      : new vscode.Range(doc.positionAt(0), doc.positionAt(doc.getText().length));

    const before = doc.getText(range);
    const after = alignText(before);

    if (after === before) {
      vscode.window.setStatusBarMessage('Symbol Align: nothing to align', 3000);
      return;
    }

    editor
      .edit((editBuilder) => editBuilder.replace(range, after))
      .then(
        () => vscode.window.setStatusBarMessage('Symbol Align: done', 3000),
        (err) =>
          vscode.window.showErrorMessage('Symbol Align failed: ' + (err && err.message))
      );
  });

  context.subscriptions.push(cmd);
}

function deactivate() {}

module.exports = { activate, deactivate };
