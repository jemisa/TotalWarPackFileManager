namespace PackFileManager.Properties
{
    using Common;
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode, CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    internal class Resources
    {
        private static System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }

        internal static Icon PackFileManager
        {
            get => (Icon) ResourceManager.GetObject("PackFileManager", Culture);
        }
        internal static System.Drawing.Icon Empire
        {
            get => (Icon) ResourceManager.GetObject("Empire", Culture);
        }
        internal static System.Drawing.Icon Napoleon
        {
            get => (Icon) ResourceManager.GetObject("Napoleon", Culture);
        }

        internal static System.Drawing.Icon Shogun
        {
            get => (Icon) ResourceManager.GetObject("Shogun", Culture);
        }

        internal static System.Drawing.Icon Rome2
        {
            get => (Icon) ResourceManager.GetObject("Rome2", Culture);
        }

        internal static System.Drawing.Icon Attila
        {
            get => (Icon) ResourceManager.GetObject("Attila", Culture);
        }

        internal static System.Drawing.Icon Warhammer
        {
            get => (Icon) ResourceManager.GetObject("Warhammer", Culture);
        }

        internal static System.Drawing.Icon Warhammer2
        {
            get => (Icon) ResourceManager.GetObject("Warhammer2", Culture);
        }

        internal static System.Drawing.Icon Britannia
        {
            get => (Icon)ResourceManager.GetObject("Britannia", Culture);
        }

        internal static System.Drawing.Icon ThreeKingdoms
        {
            get => (Icon)ResourceManager.GetObject("ThreeKingdoms", Culture);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("PackFileManager.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static Icon GetGameIcon(Game game) {
            if(game == Game.TWH)
                return Warhammer;
            else if(game == Game.ATW)
                return Attila;
            else if(game == Game.ETW)
                return Empire;
            else if(game == Game.NTW)
                return Napoleon;
            else if(game == Game.R2TW)
                return Rome2;
            else if(game == Game.STW)
                return Shogun;
            else if(game == Game.TWH2)
                return Warhammer2;
            else if(game == Game.TOB)
                return Britannia;
            else if(game == Game.TW3K)
                return ThreeKingdoms;
            else
                return PackFileManager;
        }
    }
}

