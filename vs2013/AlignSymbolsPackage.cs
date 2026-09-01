using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace PandaShitsuke.AlignSymbols
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
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
