using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace PandaShitsuke.AlignSymbols
{
    internal static class AlignSymbolsCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("2f00e0ec-58a3-4ac0-8d7f-000000000001");

        private static Package _package;
        private static OleMenuCommand _menuItem;

        public static void Initialize(Package package)
        {
            _package = package;
            IServiceProvider provider = (IServiceProvider)package;
            var service = provider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (service != null)
            {
                _menuItem = new OleMenuCommand(Execute, new CommandID(CommandSet, CommandId));
                _menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
                service.AddCommand(_menuItem);
            }
        }

        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            IServiceProvider provider = (IServiceProvider)_package;
            var dte = provider.GetService(typeof(DTE)) as DTE;
            _menuItem.Enabled = (dte != null && dte.ActiveDocument != null);
        }

        private static void Execute(object sender, EventArgs e)
        {
            IServiceProvider provider = (IServiceProvider)_package;
            var dte = provider.GetService(typeof(DTE)) as DTE;
            if (dte == null) return;
            Document doc = dte.ActiveDocument;
            if (doc == null) return;
            TextSelection selection = doc.Selection as TextSelection;
            if (selection == null) return;

            string text = selection.Text;
            if (string.IsNullOrEmpty(text)) return;

            string aligned = Aligner.AlignText(text);
            if (aligned != text)
                selection.Text = aligned;
        }
    }
}
