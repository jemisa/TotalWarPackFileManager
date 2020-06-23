namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class NativeMethods
    {
        private const int HDF_BITMAP = 0x2000;
        private const int HDF_BITMAP_ON_RIGHT = 0x1000;
        private const int HDF_CENTER = 2;
        private const int HDF_IMAGE = 0x800;
        private const int HDF_JUSTIFYMASK = 3;
        private const int HDF_LEFT = 0;
        private const int HDF_RIGHT = 1;
        private const int HDF_RTLREADING = 4;
        private const int HDF_SORTDOWN = 0x200;
        private const int HDF_SORTUP = 0x400;
        private const int HDF_STRING = 0x4000;
        private const int HDI_BITMAP = 0x10;
        private const int HDI_FORMAT = 4;
        private const int HDI_IMAGE = 0x20;
        private const int HDI_TEXT = 2;
        private const int HDI_WIDTH = 1;
        private const int HDM_FIRST = 0x1200;
        private const int HDM_GETITEM = 0x120b;
        private const int HDM_GETITEMRECT = 0x1207;
        private const int HDM_HITTEST = 0x1206;
        private const int HDM_SETITEM = 0x120c;
        private const int ILD_BLEND25 = 2;
        private const int ILD_BLEND50 = 4;
        private const int ILD_IMAGE = 0x20;
        private const int ILD_MASK = 0x10;
        private const int ILD_NORMAL = 0;
        private const int ILD_TRANSPARENT = 1;
        private const int L_MAX_URL_LENGTH = 0x824;
        private const int LVCF_FMT = 1;
        private const int LVCF_IMAGE = 0x10;
        private const int LVCF_ORDER = 0x20;
        private const int LVCF_SUBITEM = 8;
        private const int LVCF_TEXT = 4;
        private const int LVCF_WIDTH = 2;
        private const int LVCFMT_BITMAP_ON_RIGHT = 0x1000;
        private const int LVCFMT_CENTER = 2;
        private const int LVCFMT_COL_HAS_IMAGES = 0x8000;
        private const int LVCFMT_IMAGE = 0x800;
        private const int LVCFMT_JUSTIFYMASK = 3;
        private const int LVCFMT_LEFT = 0;
        private const int LVCFMT_RIGHT = 1;
        private const int LVIF_IMAGE = 2;
        private const int LVIF_INDENT = 0x10;
        private const int LVIF_NORECOMPUTE = 0x800;
        private const int LVIF_PARAM = 4;
        private const int LVIF_STATE = 8;
        private const int LVIF_TEXT = 1;
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETCOLUMN = 0x105f;
        private const int LVM_GETCOUNTPERPAGE = 0x1028;
        private const int LVM_GETGROUPINFO = 0x1095;
        private const int LVM_GETGROUPSTATE = 0x105c;
        private const int LVM_GETHEADER = 0x101f;
        private const int LVM_GETTOOLTIPS = 0x104e;
        private const int LVM_INSERTGROUP = 0x1091;
        private const int LVM_REMOVEALLGROUPS = 0x10a0;
        private const int LVM_SCROLL = 0x1014;
        private const int LVM_SETCOLUMN = 0x1060;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private const int LVM_SETGROUPINFO = 0x1093;
        private const int LVM_SETGROUPMETRICS = 0x109b;
        private const int LVM_SETIMAGELIST = 0x1003;
        private const int LVM_SETITEM = 0x104c;
        private const int LVM_SETITEMSTATE = 0x102b;
        private const int LVM_SETSELECTEDCOLUMN = 0x108c;
        private const int LVM_SETTOOLTIPS = 0x104a;
        private const int LVS_EX_SUBITEMIMAGES = 2;
        private const int MAX_LINKID_TEXT = 0x30;
        private const int SB_BOTH = 3;
        private const int SB_CTL = 2;
        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;
        private const int SIF_ALL = 0x17;
        private const int SIF_DISABLENOSCROLL = 8;
        private const int SIF_PAGE = 2;
        private const int SIF_POS = 4;
        private const int SIF_RANGE = 1;
        private const int SIF_TRACKPOS = 0x10;
        public const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOMOVE = 2;
        private const int SWP_NOREDRAW = 8;
        private const int SWP_NOSIZE = 1;
        private const int SWP_NOZORDER = 4;
        private const int SWP_sizeOnly = 30;
        private const int SWP_updateFrame = 0x37;
        private const int SWP_zOrderOnly = 0x1b;

        public static bool ChangeSize(IWin32Window toBeMoved, int width, int height)
        {
            return SetWindowPos(toBeMoved.Handle, IntPtr.Zero, 0, 0, width, height, 30);
        }

        public static bool ChangeZOrder(IWin32Window toBeMoved, IWin32Window reference)
        {
            return SetWindowPos(toBeMoved.Handle, reference.Handle, 0, 0, 0, 0, 0x1b);
        }

        public static int ClearGroups(VirtualObjectListView virtualObjectListView)
        {
            return (int) SendMessage(virtualObjectListView.Handle, 0x10a0, 0, 0);
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr objectHandle);
        public static void DeselectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 0);
        }

        public static bool DrawImageList(Graphics g, ImageList il, int index, int x, int y, bool isSelected)
        {
            int fStyle = 1;
            if (isSelected)
            {
                fStyle |= 2;
            }
            bool flag = ImageList_Draw(il.Handle, index, g.GetHdc(), x, y, fStyle);
            g.ReleaseHdc();
            return flag;
        }

        public static void ForceSubItemImagesExStyle(ListView list)
        {
            SendMessage(list.Handle, 0x1036, 2, 2);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(IntPtr hWnd, ref Rectangle r);
        public static Point GetColumnSides(ObjectListView lv, int columnIndex)
        {
            Point point = new Point(-1, -1);
            IntPtr headerControl = GetHeaderControl(lv);
            if (headerControl == IntPtr.Zero)
            {
                return new Point(-1, -1);
            }
            RECT r = new RECT();
            SendMessageRECT(headerControl, 0x1207, columnIndex, ref r);
            return new Point(r.left, r.right);
        }

        public static int GetColumnUnderPoint(IntPtr handle, Point pt)
        {
            return HeaderControlHitTest(handle, pt, 6);
        }

        public static int GetCountPerPage(ListView list)
        {
            return (int) SendMessage(list.Handle, 0x1028, 0, 0);
        }

        public static int GetDividerUnderPoint(IntPtr handle, Point pt)
        {
            return HeaderControlHitTest(handle, pt, 4);
        }

        public static int GetGroupInfo(ObjectListView olv, int groupId, ref LVGROUP2 group)
        {
            return (int) SendMessage(olv.Handle, 0x1095, groupId, ref group);
        }

        public static GroupState GetGroupState(ObjectListView olv, int groupId, GroupState mask)
        {
            return (GroupState) ((int) SendMessage(olv.Handle, 0x105c, groupId, (int) mask));
        }

        public static IntPtr GetHeaderControl(ListView list)
        {
            return SendMessage(list.Handle, 0x101f, 0, 0);
        }

        public static Point GetScrolledColumnSides(ListView lv, int columnIndex)
        {
            IntPtr headerControl = GetHeaderControl(lv);
            if (headerControl == IntPtr.Zero)
            {
                return new Point(-1, -1);
            }
            RECT r = new RECT();
            IntPtr ptr2 = SendMessageRECT(headerControl, 0x1207, columnIndex, ref r);
            int scrollPosition = GetScrollPosition(lv, true);
            return new Point(r.left - scrollPosition, r.right - scrollPosition);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, SCROLLINFO si);
        public static int GetScrollPosition(ListView lv, bool horizontalBar)
        {
            int fnBar = horizontalBar ? 0 : 1;
            SCROLLINFO si = new SCROLLINFO {
                fMask = 4
            };
            if (GetScrollInfo(lv.Handle, fnBar, si))
            {
                return si.nPos;
            }
            return -1;
        }

        public static IntPtr GetTooltipControl(ListView lv)
        {
            return SendMessage(lv.Handle, 0x104e, 0, 0);
        }

        public static Rectangle GetUpdateRect(Control cntl)
        {
            Rectangle r = new Rectangle();
            GetUpdateRectInternal(cntl.Handle, ref r, false);
            return r;
        }

        [DllImport("user32.dll", EntryPoint="GetUpdateRect", CharSet=CharSet.Auto)]
        private static extern int GetUpdateRectInternal(IntPtr hWnd, ref Rectangle r, bool eraseBackground);
        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return (int) GetWindowLong32(hWnd, nIndex);
            }
            return (int) ((long) GetWindowLongPtr64(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint="GetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint="GetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        public static bool HasBuiltinSortIndicators()
        {
            return (OSFeature.Feature.GetVersionPresent(OSFeature.Themes) != null);
        }

        private static int HeaderControlHitTest(IntPtr handle, Point pt, int flag)
        {
            HDHITTESTINFO lParam = new HDHITTESTINFO {
                pt_x = pt.X,
                pt_y = pt.Y
            };
            IntPtr ptr = SendMessageHDHITTESTINFO(handle, 0x1206, IntPtr.Zero, lParam);
            if ((lParam.flags & flag) != 0)
            {
                return lParam.iItem;
            }
            return -1;
        }

        [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
        private static extern bool ImageList_Draw(IntPtr himl, int i, IntPtr hdcDst, int x, int y, int fStyle);
        public static int InsertGroup(ObjectListView olv, LVGROUP2 group)
        {
            return (int) SendMessage(olv.Handle, 0x1091, -1, ref group);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool InvalidateRect(IntPtr hWnd, int ignored, bool erase);
        public static bool MakeTopMost(IWin32Window toBeMoved)
        {
            IntPtr hWndInsertAfter = (IntPtr) (-1);
            return SetWindowPos(toBeMoved.Handle, hWndInsertAfter, 0, 0, 0, 0, 0x1b);
        }

        public static bool Scroll(ListView list, int dx, int dy)
        {
            return (SendMessage(list.Handle, 0x1014, dx, dy) != IntPtr.Zero);
        }

        public static void SelectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 2);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP2 lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUPMETRICS lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVHITTESTINFO ht);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageHDHITTESTINFO(IntPtr hWnd, int Msg, IntPtr wParam, [In, Out] HDHITTESTINFO lParam);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        private static extern IntPtr SendMessageHDItem(IntPtr hWnd, int msg, int wParam, ref HDITEM hdi);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageIUnknown(IntPtr hWnd, int msg, [MarshalAs(UnmanagedType.IUnknown)] object wParam, int lParam);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageLVBKIMAGE(IntPtr hWnd, int Msg, int wParam, ref LVBKIMAGE lParam);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageRECT(IntPtr hWnd, int msg, int wParam, ref RECT r);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageString(IntPtr hWnd, int Msg, int wParam, string lParam);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageTOOLINFO(IntPtr hWnd, int Msg, int wParam, TOOLINFO lParam);
        public static bool SetBackgroundImage(ListView lv, Image image)
        {
            LVBKIMAGE lParam = new LVBKIMAGE();
            Bitmap bitmap = image as Bitmap;
            if (bitmap == null)
            {
                lParam.ulFlags = 0;
            }
            else
            {
                lParam.hBmp = bitmap.GetHbitmap();
                lParam.ulFlags = 0x10000000;
            }
            Application.OleRequired();
            return (SendMessageLVBKIMAGE(lv.Handle, 0x108a, 0, ref lParam) != IntPtr.Zero);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetBkColor(IntPtr hDC, int clr);
        public static void SetColumnImage(ListView list, int columnIndex, SortOrder order, int imageIndex)
        {
            IntPtr headerControl = GetHeaderControl(list);
            if (headerControl.ToInt32() != 0)
            {
                HDITEM hdi = new HDITEM {
                    mask = 4
                };
                IntPtr ptr2 = SendMessageHDItem(headerControl, 0x120b, columnIndex, ref hdi);
                hdi.fmt &= -7681;
                if (HasBuiltinSortIndicators())
                {
                    if (order == SortOrder.Ascending)
                    {
                        hdi.fmt |= 0x400;
                    }
                    if (order == SortOrder.Descending)
                    {
                        hdi.fmt |= 0x200;
                    }
                }
                else
                {
                    hdi.mask |= 0x20;
                    hdi.fmt |= 0x1800;
                    hdi.iImage = imageIndex;
                }
                ptr2 = SendMessageHDItem(headerControl, 0x120c, columnIndex, ref hdi);
            }
        }

        public static void SetExtendedStyle(ListView list, int style, int styleMask)
        {
            SendMessage(list.Handle, 0x1036, style, styleMask);
        }

        public static int SetGroupImageList(ObjectListView olv, ImageList il)
        {
            IntPtr zero = IntPtr.Zero;
            if (il != null)
            {
                zero = il.Handle;
            }
            return (int) SendMessage(olv.Handle, 0x1003, 3, zero);
        }

        public static int SetGroupInfo(ObjectListView olv, int groupId, LVGROUP2 group)
        {
            return (int) SendMessage(olv.Handle, 0x1093, groupId, ref group);
        }

        public static int SetGroupMetrics(ObjectListView olv, int groupId, LVGROUPMETRICS metrics)
        {
            return (int) SendMessage(olv.Handle, 0x109b, groupId, ref metrics);
        }

        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            LVITEM lvi = new LVITEM {
                stateMask = mask,
                state = value
            };
            SendMessageLVItem(list.Handle, 0x102b, itemIndex, ref lvi);
        }

        public static void SetSelectedColumn(ListView objectListView, ColumnHeader value)
        {
            SendMessage(objectListView.Handle, 0x108c, (value == null) ? -1 : value.Index, 0);
        }

        public static void SetSubItemImage(ListView list, int itemIndex, int subItemIndex, int imageIndex)
        {
            LVITEM lvi = new LVITEM {
                mask = 2,
                iItem = itemIndex,
                iSubItem = subItemIndex,
                iImage = imageIndex
            };
            SendMessageLVItem(list.Handle, 0x104c, 0, ref lvi);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetTextColor(IntPtr hDC, int crColor);
        public static IntPtr SetTooltipControl(ListView lv, ToolTipControl tooltip)
        {
            return SendMessage(lv.Handle, 0x104a, 0, tooltip.Handle);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return (int) SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return (int) ((long) SetWindowLongPtr64(hWnd, nIndex, dwNewLong));
        }

        [DllImport("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SetWindowTheme(IntPtr hWnd, string subApp, string subIdList);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void ShowWithoutActivate(IWin32Window win)
        {
            ShowWindow(win.Handle, 8);
        }

        [DllImport("user32.dll", EntryPoint="ValidateRect", CharSet=CharSet.Auto)]
        private static extern IntPtr ValidatedRectInternal(IntPtr hWnd, ref Rectangle r);
        public static void ValidateRect(Control cntl, Rectangle r)
        {
            ValidatedRectInternal(cntl.Handle, ref r);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HDHITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public int mask;
            public int cxy;
            public IntPtr pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            public int type;
            public IntPtr pvFilter;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HDLAYOUT
        {
            public IntPtr prc;
            public IntPtr pwpos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct LITEM
        {
            public uint mask;
            public int iLink;
            public uint state;
            public uint stateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x30)]
            public string szID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x824)]
            public string szUrl;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVBKIMAGE
        {
            public int ulFlags;
            public IntPtr hBmp;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszImage;
            public int cchImageMax;
            public int xOffset;
            public int yOffset;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVCOLUMN
        {
            public int mask;
            public int fmt;
            public int cx;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iSubItem;
            public int iImage;
            public int iOrder;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVFINDINFO
        {
            public int flags;
            public string psz;
            public IntPtr lParam;
            public int ptX;
            public int ptY;
            public int vkDirection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUP
        {
            public uint cbSize;
            public uint mask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszHeader;
            public int cchHeader;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszFooter;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUP2
        {
            public uint cbSize;
            public uint mask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszHeader;
            public uint cchHeader;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszFooter;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszSubtitle;
            public uint cchSubtitle;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszTask;
            public uint cchTask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszDescriptionTop;
            public uint cchDescriptionTop;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszDescriptionBottom;
            public uint cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public int cItems;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszSubsetTitle;
            public uint cchSubsetTitle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUPMETRICS
        {
            public uint cbSize;
            public uint mask;
            public uint Left;
            public uint Top;
            public uint Right;
            public uint Bottom;
            public int crLeft;
            public int crTop;
            public int crRight;
            public int crBottom;
            public int crHeader;
            public int crFooter;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVHITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
            public int iSubItem;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVITEMINDEX
        {
            public int iItem;
            public int iGroup;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMCUSTOMDRAW
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR nmcd;
            public int dwDrawStage;
            public IntPtr hdc;
            public BrightIdeasSoftware.NativeMethods.RECT rc;
            public IntPtr dwItemSpec;
            public int uItemState;
            public IntPtr lItemlParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHEADER
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR nhdr;
            public int iItem;
            public int iButton;
            public IntPtr pHDITEM;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLISTVIEW
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR hdr;
            public int iItem;
            public int iSubItem;
            public int uNewState;
            public int uOldState;
            public int uChanged;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVCUSTOMDRAW
        {
            public BrightIdeasSoftware.NativeMethods.NMCUSTOMDRAW nmcd;
            public int clrText;
            public int clrTextBk;
            public int iSubItem;
            public int dwItemType;
            public int clrFace;
            public int iIconEffect;
            public int iIconPhase;
            public int iPartId;
            public int iStateId;
            public BrightIdeasSoftware.NativeMethods.RECT rcText;
            public uint uAlign;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVFINDITEM
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR hdr;
            public int iStart;
            public BrightIdeasSoftware.NativeMethods.LVFINDINFO lvfi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVLINK
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR hdr;
            public BrightIdeasSoftware.NativeMethods.LITEM link;
            public int iItem;
            public int iSubItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMLVSCROLL
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR hdr;
            public int dx;
            public int dy;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public struct NMTTDISPINFO
        {
            public BrightIdeasSoftware.NativeMethods.NMHDR hdr;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szText;
            public IntPtr hinst;
            public int uFlags;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SCROLLINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(BrightIdeasSoftware.NativeMethods.SCROLLINFO));
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class TOOLINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(BrightIdeasSoftware.NativeMethods.TOOLINFO));
            public int uFlags;
            public IntPtr hwnd;
            public IntPtr uId;
            public BrightIdeasSoftware.NativeMethods.RECT rect;
            public IntPtr hinst = IntPtr.Zero;
            public IntPtr lpszText;
            public IntPtr lParam = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }
    }
}

