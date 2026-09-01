using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace PandaShitsuke.AlignSymbols
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    [Guid(PackageGuidString)]
    public sealed class AlignSymbolsPackage : Package
    {
        public const string PackageGuidString = "0b3d2b0a-7a66-4d1f-9c0a-000000000001";

        protected override void Initialize()
        {
            base.Initialize();
            AlignSymbolsCommand.Initialize(this);
        }
    }
}
