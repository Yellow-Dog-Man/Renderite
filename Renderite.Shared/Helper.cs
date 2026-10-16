using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Renderite.Shared
{
    public static class Helper
    {
        static Helper()
        {
            try
            {
                WineVersion = wine_get_version();


                // The returned values do not need to be freed; they're owned by WINE.
                IntPtr sysname_ptr = IntPtr.Zero;
                IntPtr release_ptr = IntPtr.Zero;
                wine_get_host_version(ref sysname_ptr, ref release_ptr);

                if (sysname_ptr != IntPtr.Zero)
                {
                    string sysname = Marshal.PtrToStringAnsi(sysname_ptr);

                    if (sysname == "Linux") IsWineUnderLinux = true;
                    if (sysname == "Darwin") IsWineUnderMacOS = true;
                }
            }
            catch { }

            // Initialize the kill -0 {PID} argv array, used on MacOS.
            Marshal.WriteIntPtr(ARGV, 0, BIN_KILL); // argv[0] = "/bin/kill"
            Marshal.WriteIntPtr(ARGV, IntPtr.Size, DASH_ZERO); // argv[1] = "-0"
            Marshal.WriteIntPtr(ARGV, IntPtr.Size*3, IntPtr.Zero); // argv[3] = NULL
        }

        public const int EDITOR_PORT = 42512;

        public const string FOLDER_PATH = "Renderer";
        public const string PROCESS_NAME = "Renderite.Renderer";
        public const string QUEUE_NAME_ARGUMENT = "QueueName";
        public const string QUEUE_CAPACITY_ARGUMENT = "QueueCapacity";

        public const string PRIMARY_QUEUE = "Primary";
        public const string BACKGROUND_QUEUE = "Background";

        public static readonly bool IsWineUnderLinux;
        public static readonly bool IsWineUnderMacOS;
        public static readonly string? WineVersion;

        public static string ComposeMemoryViewName(string prefix, int bufferId) => $"{prefix}_{bufferId:X}";

        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string wine_get_version();

        [DllImport("ntdll.dll", EntryPoint = "wine_get_host_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void wine_get_host_version(ref IntPtr sysname, ref IntPtr release);
        
        // These are used to unmangle paths.
        [DllImport("kernel32.dll", EntryPoint = "wine_get_unix_file_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr wine_get_unix_file_name([MarshalAs(UnmanagedType.LPWStr)] string path);

        [DllImport("kernel32.dll", EntryPoint = "wine_get_dos_file_name", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr wine_get_dos_file_name([MarshalAs(UnmanagedType.LPStr)] string path);

        // The above wine functions require manual memory management. WINE's example `winepath` code uses these, but I'm not sure if there's a way to do this through Marshal.
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);
        
        public static string? DosToUnix(string path)
        {
            IntPtr ptr = wine_get_unix_file_name(path);

            if (ptr == IntPtr.Zero)
                return null;

            try
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            catch
            {
                return null;
            }
            finally
            {
                HeapFree(GetProcessHeap(), 0, ptr);
            }
        }

        public static string? UnixToDos(string path)
        {
            IntPtr ptr = wine_get_dos_file_name(path);

            if (ptr == IntPtr.Zero)
                return null;

            try
            {
                return Marshal.PtrToStringUni(ptr);
            }
            catch
            {
                return null;
            }
            finally
            {
                HeapFree(GetProcessHeap(), 0, ptr);
            }
        }

        // This is used to spawn a UNIX process.
        [DllImport("ntdll.dll", EntryPoint = "__wine_unix_spawnvp", CallingConvention = CallingConvention.Winapi)]
        private static extern int __wine_unix_spawnvp(IntPtr argv, int wait);
        private static IntPtr BIN_KILL = Marshal.StringToCoTaskMemAnsi("/bin/kill");
        private static IntPtr DASH_ZERO = Marshal.StringToCoTaskMemAnsi("-0");
        private static IntPtr ARGV = Marshal.AllocCoTaskMem(4 * IntPtr.Size);

        // /proc is not POSIX and does not exist on macOS. To query the status of a process, we spawn `kill -0 PID` and
        // determine its return status; if it returns 0, the process exists. If it returns another code, the process is inaccessible.
        public static bool PosixIsRunning(int pid)
        {
            IntPtr pidString = IntPtr.Zero;

            try
            {
                pidString = Marshal.StringToCoTaskMemAnsi(pid.ToString());
                Marshal.WriteIntPtr(ARGV, 2 * IntPtr.Size, pidString); // argv[2] = pidString

                return __wine_unix_spawnvp(ARGV, 1) == 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pidString != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pidString);
            }
        }
    }
}
