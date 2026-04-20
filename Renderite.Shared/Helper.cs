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
                IsWine = true;
            }
            catch { }
        }

        public const int EDITOR_PORT = 42512;

        public const string FOLDER_PATH = "Renderer";
        public const string PROCESS_NAME = "Renderite.Renderer";
        public const string QUEUE_NAME_ARGUMENT = "QueueName";
        public const string QUEUE_CAPACITY_ARGUMENT = "QueueCapacity";

        public const string PRIMARY_QUEUE = "Primary";
        public const string BACKGROUND_QUEUE = "Background";

        public static readonly bool IsWine;
        public static readonly string? WineVersion;

        public static string ComposeMemoryViewName(string prefix, int bufferId) => $"{prefix}_{bufferId:X}";

        [DllImport("ntdll.dll", EntryPoint = "wine_get_version", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern string wine_get_version();
    }
}
