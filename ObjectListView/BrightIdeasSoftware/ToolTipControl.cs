namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class ToolTipControl : NativeWindow
    {
        private System.Drawing.Font font;
        private const int GWL_STYLE = -16;
        private bool hasBorder = true;
        private Hashtable settings;
        private StandardIcons standardIcon;
        private string title;
        private const int TTDT_AUTOMATIC = 0;
        private const int TTDT_AUTOPOP = 2;
        private const int TTDT_INITIAL = 3;
        private const int TTDT_RESHOW = 1;
        private const int TTF_CENTERTIP = 2;
        private const int TTF_IDISHWND = 1;
        private const int TTF_PARSELINKS = 0x1000;
        private const int TTF_RTLREADING = 4;
        private const int TTF_SUBCLASS = 0x10;
        private const int TTM_ADDTOOL = 0x432;
        private const int TTM_ADJUSTRECT = 0x41f;
        private const int TTM_DELTOOL = 0x433;
        private const int TTM_GETBUBBLESIZE = 0x41e;
        private const int TTM_GETCURRENTTOOL = 0x43b;
        private const int TTM_GETDELAYTIME = 0x415;
        private const int TTM_GETTIPBKCOLOR = 0x416;
        private const int TTM_GETTIPTEXTCOLOR = 0x417;
        private const int TTM_NEWTOOLRECT = 0x434;
        private const int TTM_POP = 0x41c;
        private const int TTM_SETDELAYTIME = 0x403;
        private const int TTM_SETMAXTIPWIDTH = 0x418;
        private const int TTM_SETTIPBKCOLOR = 0x413;
        private const int TTM_SETTIPTEXTCOLOR = 0x414;
        private const int TTM_SETTITLE = 0x421;
        private const int TTM_SETTOOLINFO = 0x436;
        private const int TTN_FIRST = -520;
        public const int TTN_GETDISPINFO = -530;
        public const int TTN_LINKCLICK = -523;
        public const int TTN_POP = -522;
        public const int TTN_SHOW = -521;
        private const int TTS_BALLOON = 0x40;
        private const int TTS_NOPREFIX = 2;
        private const int TTS_USEVISUALSTYLE = 0x100;
        private const int WM_GETFONT = 0x31;
        private const int WM_SETFONT = 0x30;
        private const int WS_BORDER = 0x800000;
        private const int WS_EX_TOPMOST = 8;

        public event EventHandler<EventArgs> Pop;

        public event EventHandler<ToolTipShowingEventArgs> Showing;

        public void AddTool(IWin32Window window)
        {
            BrightIdeasSoftware.NativeMethods.TOOLINFO lParam = this.MakeToolInfoStruct(window);
            BrightIdeasSoftware.NativeMethods.SendMessageTOOLINFO(base.Handle, 0x432, 0, lParam);
        }

        private void ApplyEventFormatting(ToolTipShowingEventArgs args)
        {
            if ((((args.IsBalloon.HasValue || args.BackColor.HasValue) || (args.ForeColor.HasValue || (args.Title != null))) || (args.StandardIcon.HasValue || args.AutoPopDelay.HasValue)) || (args.Font != null))
            {
                this.PushSettings();
                if (args.IsBalloon.HasValue)
                {
                    this.IsBalloon = args.IsBalloon.Value;
                }
                if (args.BackColor.HasValue)
                {
                    this.BackColor = args.BackColor.Value;
                }
                if (args.ForeColor.HasValue)
                {
                    this.ForeColor = args.ForeColor.Value;
                }
                if (args.StandardIcon.HasValue)
                {
                    this.StandardIcon = args.StandardIcon.Value;
                }
                if (args.AutoPopDelay.HasValue)
                {
                    this.AutoPopDelay = args.AutoPopDelay.Value;
                }
                if (args.Font != null)
                {
                    this.Font = args.Font;
                }
                if (args.Title != null)
                {
                    this.Title = args.Title;
                }
            }
        }

        public void Create(IntPtr parentHandle)
        {
            if (!(base.Handle != IntPtr.Zero))
            {
                CreateParams cp = new CreateParams {
                    ClassName = "tooltips_class32",
                    Style = 2,
                    ExStyle = 8,
                    Parent = parentHandle
                };
                this.CreateHandle(cp);
                this.SetMaxWidth();
            }
        }

        private int GetDelayTime(int which)
        {
            return (int) BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x415, which, 0);
        }

        public virtual bool HandleGetDispInfo(ref Message msg)
        {
            this.SetMaxWidth();
            ToolTipShowingEventArgs e = new ToolTipShowingEventArgs {
                ToolTipControl = this
            };
            this.OnShowing(e);
            if (string.IsNullOrEmpty(e.Text))
            {
                return false;
            }
            this.ApplyEventFormatting(e);
            BrightIdeasSoftware.NativeMethods.NMTTDISPINFO lParam = (BrightIdeasSoftware.NativeMethods.NMTTDISPINFO) msg.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMTTDISPINFO));
            lParam.lpszText = e.Text;
            lParam.hinst = IntPtr.Zero;
            if (e.RightToLeft == RightToLeft.Yes)
            {
                lParam.uFlags |= 4;
            }
            Marshal.StructureToPtr(lParam, msg.LParam, false);
            return true;
        }

        public virtual bool HandleLinkClick(ref Message msg)
        {
            return false;
        }

        protected virtual bool HandleNotify(ref Message msg)
        {
            return false;
        }

        public virtual bool HandlePop(ref Message msg)
        {
            this.PopSettings();
            return true;
        }

        protected virtual bool HandleReflectNotify(ref Message msg)
        {
            BrightIdeasSoftware.NativeMethods.NMHEADER lParam = (BrightIdeasSoftware.NativeMethods.NMHEADER) msg.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMHEADER));
            switch (lParam.nhdr.code)
            {
                case -523:
                    if (!this.HandleLinkClick(ref msg))
                    {
                        break;
                    }
                    return true;

                case -522:
                    if (!this.HandlePop(ref msg))
                    {
                        break;
                    }
                    return true;

                case -521:
                    if (!this.HandleShow(ref msg))
                    {
                        break;
                    }
                    return true;

                case -530:
                    if (this.HandleGetDispInfo(ref msg))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public virtual bool HandleShow(ref Message msg)
        {
            return false;
        }

        private BrightIdeasSoftware.NativeMethods.TOOLINFO MakeToolInfoStruct(IWin32Window window)
        {
            return new BrightIdeasSoftware.NativeMethods.TOOLINFO { hwnd = window.Handle, uFlags = 0x11, uId = window.Handle, lpszText = (IntPtr) (-1) };
        }

        protected virtual void OnPop(EventArgs e)
        {
            if (this.Pop != null)
            {
                this.Pop(this, e);
            }
        }

        protected virtual void OnShowing(ToolTipShowingEventArgs e)
        {
            if (this.Showing != null)
            {
                this.Showing(this, e);
            }
        }

        public void PopSettings()
        {
            if (this.settings != null)
            {
                this.IsBalloon = (bool) this.settings["IsBalloon"];
                this.HasBorder = (bool) this.settings["HasBorder"];
                this.BackColor = (Color) this.settings["BackColor"];
                this.ForeColor = (Color) this.settings["ForeColor"];
                this.Title = (string) this.settings["Title"];
                this.StandardIcon = (StandardIcons) this.settings["StandardIcon"];
                this.AutoPopDelay = (int) this.settings["AutoPopDelay"];
                this.InitialDelay = (int) this.settings["InitialDelay"];
                this.ReshowDelay = (int) this.settings["ReshowDelay"];
                this.Font = (System.Drawing.Font) this.settings["Font"];
                this.settings = null;
            }
        }

        public void PopToolTip(IWin32Window window)
        {
            BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x41c, 0, 0);
        }

        public void PushSettings()
        {
            if (this.settings == null)
            {
                this.settings = new Hashtable();
                this.settings["IsBalloon"] = this.IsBalloon;
                this.settings["HasBorder"] = this.HasBorder;
                this.settings["BackColor"] = this.BackColor;
                this.settings["ForeColor"] = this.ForeColor;
                this.settings["Title"] = this.Title;
                this.settings["StandardIcon"] = this.StandardIcon;
                this.settings["AutoPopDelay"] = this.AutoPopDelay;
                this.settings["InitialDelay"] = this.InitialDelay;
                this.settings["ReshowDelay"] = this.ReshowDelay;
                this.settings["Font"] = this.Font;
            }
        }

        public void RemoveToolTip(IWin32Window window)
        {
            BrightIdeasSoftware.NativeMethods.TOOLINFO lParam = this.MakeToolInfoStruct(window);
            BrightIdeasSoftware.NativeMethods.SendMessageTOOLINFO(base.Handle, 0x433, 0, lParam);
        }

        private void SetDelayTime(int which, int value)
        {
            BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x403, which, value);
        }

        public void SetMaxWidth()
        {
            this.SetMaxWidth(SystemInformation.MaxWindowTrackSize.Width);
        }

        public void SetMaxWidth(int maxWidth)
        {
            BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x418, 0, maxWidth);
        }

        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case 0x4e:
                    if (!this.HandleNotify(ref msg))
                    {
                        return;
                    }
                    break;

                case 0x204e:
                    if (!this.HandleReflectNotify(ref msg))
                    {
                        return;
                    }
                    break;
            }
            base.WndProc(ref msg);
        }

        public int AutoPopDelay
        {
            get
            {
                return this.GetDelayTime(2);
            }
            set
            {
                this.SetDelayTime(2, value);
            }
        }

        public Color BackColor
        {
            get
            {
                int num = (int) BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x416, 0, 0);
                return ColorTranslator.FromWin32(num);
            }
            set
            {
                if (!ObjectListView.IsVista)
                {
                    int wParam = ColorTranslator.ToWin32(value);
                    BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x413, wParam, 0);
                }
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                IntPtr hfont = BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x31, 0, 0);
                if (hfont == IntPtr.Zero)
                {
                    return Control.DefaultFont;
                }
                return System.Drawing.Font.FromHfont(hfont);
            }
            set
            {
                System.Drawing.Font font = value ?? Control.DefaultFont;
                if (font != this.font)
                {
                    this.font = font;
                    IntPtr wParam = this.font.ToHfont();
                    BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x30, wParam, 0);
                }
            }
        }

        public Color ForeColor
        {
            get
            {
                int num = (int) BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x417, 0, 0);
                return ColorTranslator.FromWin32(num);
            }
            set
            {
                if (!ObjectListView.IsVista)
                {
                    int wParam = ColorTranslator.ToWin32(value);
                    BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x414, wParam, 0);
                }
            }
        }

        public bool HasBorder
        {
            get
            {
                return this.hasBorder;
            }
            set
            {
                if (this.hasBorder != value)
                {
                    if (value)
                    {
                        this.WindowStyle |= 0x800000;
                    }
                    else
                    {
                        this.WindowStyle &= -8388609;
                    }
                }
            }
        }

        public int InitialDelay
        {
            get
            {
                return this.GetDelayTime(3);
            }
            set
            {
                this.SetDelayTime(3, value);
            }
        }

        public bool IsBalloon
        {
            get
            {
                return ((this.WindowStyle & 0x40) == 0x40);
            }
            set
            {
                if (this.IsBalloon != value)
                {
                    int windowStyle = this.WindowStyle;
                    if (value)
                    {
                        windowStyle |= 320;
                        if (!ObjectListView.IsVista)
                        {
                            windowStyle &= -8388609;
                        }
                    }
                    else
                    {
                        windowStyle &= -321;
                        if (!ObjectListView.IsVista)
                        {
                            if (this.hasBorder)
                            {
                                windowStyle |= 0x800000;
                            }
                            else
                            {
                                windowStyle &= -8388609;
                            }
                        }
                    }
                    this.WindowStyle = windowStyle;
                }
            }
        }

        public int ReshowDelay
        {
            get
            {
                return this.GetDelayTime(1);
            }
            set
            {
                this.SetDelayTime(1, value);
            }
        }

        public StandardIcons StandardIcon
        {
            get
            {
                return this.standardIcon;
            }
            set
            {
                this.standardIcon = value;
                BrightIdeasSoftware.NativeMethods.SendMessageString(base.Handle, 0x421, (int) this.standardIcon, this.title);
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.title = string.Empty;
                }
                else if (value.Length >= 100)
                {
                    this.title = value.Substring(0, 0x63);
                }
                else
                {
                    this.title = value;
                }
                BrightIdeasSoftware.NativeMethods.SendMessageString(base.Handle, 0x421, (int) this.standardIcon, this.title);
            }
        }

        internal int WindowStyle
        {
            get
            {
                return BrightIdeasSoftware.NativeMethods.GetWindowLong(base.Handle, -16);
            }
            set
            {
                BrightIdeasSoftware.NativeMethods.SetWindowLong(base.Handle, -16, value);
            }
        }

        public enum StandardIcons
        {
            None,
            Info,
            Warning,
            Error,
            InfoLarge,
            WarningLarge,
            ErrorLarge
        }
    }
}

