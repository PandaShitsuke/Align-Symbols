// Guids.cs
// MUST match guids.h
using System;

namespace PandaShitsuke.AlignSymbols
{
    static class GuidList
    {
        public const string guidAlignSymbolsPkgString = "83516aa2-019f-4f3d-9b9d-2e03c495eb52";
        public const string guidAlignSymbolsCmdSetString = "e5effb40-fc30-4965-870d-52f553bb9797";

        public static readonly Guid guidAlignSymbolsCmdSet = new Guid(guidAlignSymbolsCmdSetString);
    };
}