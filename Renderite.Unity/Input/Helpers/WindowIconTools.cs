using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

// From: https://github.com/YAL-GameMaker/window_set_icon

namespace Renderite.Unity
{
    public class WindowIconTools
    {
        static IntPtr[] _SetIcon_baseIcon = new IntPtr[2];
        static bool[] _SetIcon_hasBaseIcon = new bool[2];
        static IconCache[] _SetIcon_cache = new IconCache[2] { new IconCache(), new IconCache() };

        public static bool SetIcon(Span<byte> bgra, int width, int height, WindowIconKind kind, bool topRowFirst = false)
        {
            var hwnd = WindowsNativeHelper.MainWindowHandle;

            if (hwnd == IntPtr.Zero)
                return false;

            var index = (int)kind;

            IntPtr icon;

            if (!bgra.IsEmpty)
            {
                icon = _SetIcon_cache[index].Update(bgra, width, height, topRowFirst);

                if (icon == IntPtr.Zero)
                    return false;

                if (!_SetIcon_hasBaseIcon[index])
                {
                    _SetIcon_hasBaseIcon[index] = true;
                    _SetIcon_baseIcon[index] = SendMessage(hwnd, 0x7F/*WM_GETICON*/, index, IntPtr.Zero);
                }
            }
            else
            {
                if (!_SetIcon_hasBaseIcon[index])
                    return true;

                icon = _SetIcon_baseIcon[index];
            }

            SendMessage(hwnd, 0x80/*WM_SETICON*/, index, icon);

            return true;
        }

        static IconCache _SetOverlayIcon_cache = new IconCache();

        public static bool SetOverlayIcon(Span<byte> bgra, int width, int height, string description = "")
        {
            var hwnd = WindowsNativeHelper.MainWindowHandle;

            if (hwnd == IntPtr.Zero)
                return false;

            if (!bgra.IsEmpty)
            {
                var icon = _SetOverlayIcon_cache.Update(bgra, width, height);

                if (icon == IntPtr.Zero)
                    return false;

                taskbarList.SetOverlayIcon(hwnd, icon, description);
            }
            else
                taskbarList.SetOverlayIcon(hwnd, IntPtr.Zero, description);

            return true;
        }

        #region progress
        public static bool SetProgress(TaskbarProgressBarState state, ulong completed, ulong total)
        {
            var hwnd = WindowsNativeHelper.MainWindowHandle;

            if (hwnd == IntPtr.Zero)
                return false;

            var tbl = taskbarList;
            tbl.SetProgressState(hwnd, state);
            tbl.SetProgressValue(hwnd, completed, total);

            return true;
        }

        public static bool SetProgressState(TaskbarProgressBarState state)
        {
            var hwnd = WindowsNativeHelper.MainWindowHandle;

            if (hwnd == IntPtr.Zero)
                return false;

            taskbarList.SetProgressState(hwnd, state);

            return true;
        }

        public static bool SetProgressValue(ulong completed, ulong total)
        {
            var hwnd = WindowsNativeHelper.MainWindowHandle;

            if (hwnd == IntPtr.Zero)
                return false;

            taskbarList.SetProgressValue(hwnd, completed, total);

            return true;
        }
        #endregion

        #region Icon externs

        class IconCache
        {
            IntPtr bitmap = IntPtr.Zero;
            IntPtr icon = IntPtr.Zero;

            public unsafe IntPtr Update(Span<byte> bgra, int width, int height, bool topRowFirst = false)
            {
                var bitmap = CreateBitmap(width, height, 1, 32, IntPtr.Zero);
                var dc = GetDC(IntPtr.Zero);

                BITMAPINFOHEADER bmi = new BITMAPINFOHEADER();

                bmi.Init();

                bmi.biWidth = width;
                bmi.biHeight = topRowFirst ? -height : height;
                bmi.biPlanes = 1;
                bmi.biBitCount = 32;
                bmi.biCompression = BitmapCompressionMode.BI_RGB;

                fixed(void* bgraPtr = bgra)
                    SetDIBits(dc, bitmap, 0, (uint)height, new IntPtr(bgraPtr), ref bmi, (int)DibColors.DIB_RGB_COLORS);

                ReleaseDC(IntPtr.Zero, dc);

                var inf = new ICONINFO();

                inf.IsIcon = true;
                inf.ColorBitmap = bitmap;
                inf.MaskBitmap = bitmap;

                var icon = CreateIconIndirect(ref inf);

                if (icon == IntPtr.Zero)
                {
                    DeleteObject(bitmap);
                    return IntPtr.Zero;
                }

                if (this.bitmap != IntPtr.Zero)
                    DeleteObject(this.bitmap);

                this.bitmap = bitmap;

                if (this.icon != IntPtr.Zero)
                    DestroyIcon(this.icon);

                this.icon = icon;

                return icon;
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("gdi32.dll")]
        static extern int SetDIBits(IntPtr hDC, IntPtr hBitmap, uint start, uint clines, IntPtr lpvBits, ref BITMAPINFOHEADER lpbmi, uint colorUse);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public BitmapCompressionMode biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }
        enum BitmapCompressionMode : uint
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        [DllImport("user32.dll")]
        static extern IntPtr CreateIconIndirect([In] ref ICONINFO piconinfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool IsIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr MaskBitmap;
            public IntPtr ColorBitmap;
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        #endregion

        #region Taskbarlist impl.
        private static object _initLock = new object();

        private static ITaskbarList4 _taskbarList;
        private static bool _taskbarListReady = false;
        private static ITaskbarList4 taskbarList
        {
            get
            {
                if (!_taskbarListReady)
                {
                    lock (_initLock)
                    {
                        if (!_taskbarListReady)
                        {
                            try
                            {
                                _taskbarList = (ITaskbarList4)new CTaskbarList();
                                _taskbarList.HrInit();
                            }
                            catch (Exception)
                            {
                                Debug.LogError("ITaskbarList4 init failed!"
                                    + " Go to Build Settings > Player Settings > Standalone > Other Settings,"
                                    + " and set Api Compatibility Level to 4.x");
                            }
                            _taskbarListReady = true;
                        }
                    }
                }
                return _taskbarList;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [ComImport, Guid("c43dc798-95d1-4bea-9030-bb99e2983a1a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList4
        {
            [PreserveSig] void HrInit();
            [PreserveSig] void AddTab(IntPtr hwnd);
            [PreserveSig] void DeleteTab(IntPtr hwnd);
            [PreserveSig] void ActivateTab(IntPtr hwnd);
            [PreserveSig] void SetActiveAlt(IntPtr hwnd);
            [PreserveSig] void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
            [PreserveSig] void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            [PreserveSig] void SetProgressState(IntPtr hwnd, TaskbarProgressBarState tbpFlags);
            [PreserveSig] void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
            [PreserveSig] void UnregisterTab(IntPtr hwndTab);
            [PreserveSig] void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
            [PreserveSig] void SetTabActive(IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved);
            [PreserveSig] int ThumbBarAddButtons(IntPtr hwnd, uint cButtons, IntPtr pButtons);
            [PreserveSig] int ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, IntPtr pButtons);
            [PreserveSig] void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
            [PreserveSig] void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
            [PreserveSig] void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
            [PreserveSig] void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);
        }
        [ComImport, Guid("56fdf344-fd6d-11d0-958a-006097c9a090"), ClassInterface(ClassInterfaceType.None)]
        private class CTaskbarList { }
        #endregion
    }

    public enum WindowIconKind
    {
        Small = 0,
        Big = 1
    }

    public enum TaskbarProgressBarState
    {
        NoProgress = 0,
        Indeterminate = 1,
        Normal = 2,
        Error = 4,
        Paused = 8
    }

    public enum DibColors
    {
        DIB_RGB_COLORS = 0x00,
        DIB_PAL_COLORS = 0x01,
        DIB_PAL_INDICES = 0x02,
    }
}
