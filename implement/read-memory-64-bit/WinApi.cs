using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace read_memory_64_bit
{
    public static partial class WinApi
    {
        internal const int SW_HIDE = 0;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int SW_SHOW = 5;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_RESTORE = 9;
        internal const int SW_SHOWDEFAULT = 10;
        internal const int SW_FORCEMINIMIZE = 11;
        internal const int SW_MAXIMIZE = 3;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial IntPtr OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        // Can't get this to work with LibraryImport. Compiles fine but causes infinite memory reads at runtime
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref UIntPtr lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(IntPtr hHandle);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetProcessDPIAware();

        [LibraryImport("user32.dll")]
        public static partial IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial IntPtr GetDesktopWindow();

        [LibraryImport("user32.dll")]
        public static partial IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /*
        https://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title/20276701#20276701
        https://stackoverflow.com/questions/295996/is-the-order-in-which-handles-are-returned-by-enumwindows-meaningful/296014#296014
        */
        public static System.Collections.Generic.IReadOnlyList<IntPtr> ListWindowHandlesInZOrder()
        {
            IntPtr found = IntPtr.Zero;
            System.Collections.Generic.List<IntPtr> windowHandles = [];

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                windowHandles.Add(wnd);

                // return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windowHandles;
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void ShowWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOW);
            SetForegroundWindow(hWnd);
        }

        public static IntPtr HideWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_HIDE);
        }

        public static IntPtr MinimizeWindow(IntPtr hWnd)
        {
            return ShowWindow(hWnd, SW_MINIMIZE);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point(int x, int y)
        {
            public int x = x;
            public int y = y;
        }

        //  http://www.pinvoke.net/default.aspx/kernel32.virtualqueryex
        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public AllocationProtect AllocationProtect;
            public IntPtr RegionSize;
            public MemoryInformationState State;
            public AllocationProtect Protect;
            public MemoryInformationType Type;
        }

        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        public enum MemoryInformationState : int
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000,
        }

        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        public enum MemoryInformationType : int
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000,
        }

        //  https://docs.microsoft.com/en-au/windows/win32/memory/memory-protection-constants
        [Flags]
        public enum MemoryInformationProtection : int
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,

            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
        }
    }
}
