using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Unity
{
    public static class WindowsNativeHelper
    {
        const uint GW_OWNER = 4;

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();

            if (activatedHandle == IntPtr.Zero)
                return false;     // No window is currently activated

            var procId = Process.GetCurrentProcess().Id;

            GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            return activeProcId == procId;
        }

        public static IntPtr MainWindowHandle
        {
            get
            {
                if (_mainWindowHandle == IntPtr.Zero)
                    _mainWindowHandle = GetSelfMainWindowHandle();

                return _mainWindowHandle;
            }
        }

        static IntPtr _mainWindowHandle = IntPtr.Zero;

        public static unsafe IntPtr GetSelfMainWindowHandle()
        {
            IntPtr mainWindow = IntPtr.Zero;
            EnumWindows(MainWindowPredicate, (IntPtr)(&mainWindow));
            return mainWindow;
        }

        [MonoPInvokeCallback(typeof(EnumWindowsProc))]
        private static unsafe bool MainWindowPredicate(IntPtr hWnd, IntPtr lParam)
        {
            var mainWindowPtr = (IntPtr*)lParam;
            var processId = Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(hWnd, out var windowPid);

            if (windowPid == processId)
            {
                // Check if it's a top-level visible window (not owned)
                if (GetWindow(hWnd, GW_OWNER) == IntPtr.Zero && IsWindowVisible(hWnd))
                {
                    *mainWindowPtr = hWnd;
                    return false;
                }
            }

            return true;
        }

        public static bool SetWindowTitle(string title)
        {
            var windowHandle = GetSelfMainWindowHandle();

            if (windowHandle == IntPtr.Zero)
                return false;       // No window is currently activated

            return SetWindowText(windowHandle, title);
        }

        public static bool ParentWindowUnderMain(IntPtr window)
        {
            var parent = GetSelfMainWindowHandle();

            if (parent == IntPtr.Zero)
                return false;

            // Adjust styles: remove WS_POPUP, add WS_CHILD
            int style = GetWindowLong(window, GWL_STYLE);
            style &= ~WS_POPUP;
            style |= WS_CHILD;
            SetWindowLong(window, GWL_STYLE, style);

            var result = SetParent(window, parent);

            return result != IntPtr.Zero;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlonga
        const int GWL_STYLE = -16;
        // https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
        const int WS_CHILD = 0x40000000;
        const int WS_POPUP = unchecked((int)0x80000000);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        private static extern bool SetWindowText(IntPtr handle, string title);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
