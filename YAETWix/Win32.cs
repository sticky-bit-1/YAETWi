using System;
using System.Runtime.InteropServices;

namespace YAETWix
{
    internal static class Win32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct LOAD_DLL_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr lpBaseOfDll;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UNLOAD_DLL_DEBUG_INFO
        {
            public IntPtr lpBaseOfDll;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OUTPUT_DEBUG_STRING_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)] public string lpDebugStringData;
            public ushort fUnicode;
            public ushort nDebugStringLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RIP_INFO
        {
            public uint dwError;
            public uint dwType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EXIT_THREAD_DEBUG_INFO
        {
            public uint dwExitCode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EXIT_PROCESS_DEBUG_INFO
        {
            public uint dwExitCode;
        }
        public struct CREATE_PROCESS_DEBUG_INFO
        {
            public IntPtr hFile;
            public IntPtr hProcess;
            public IntPtr hThread;
            public IntPtr lpBaseOfImage;
            public uint dwDebugInfoFileOffset;
            public uint nDebugInfoSize;
            public IntPtr lpThreadLocalBase;
            public PTHREAD_START_ROUTINE lpStartAddress;
            public IntPtr lpImageName;
            public ushort fUnicode;
        }

        public delegate uint PTHREAD_START_ROUTINE(IntPtr lpThreadParameter);

        [StructLayout(LayoutKind.Sequential)]
        public struct CREATE_THREAD_DEBUG_INFO
        {
            public IntPtr hThread;
            public IntPtr lpThreadLocalBase;
            public PTHREAD_START_ROUTINE lpStartAddress;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EXCEPTION_RECORD
        {
            public uint ExceptionCode;
            public uint ExceptionFlags;
            public IntPtr ExceptionRecord;
            public IntPtr ExceptionAddress;
            public uint NumberParameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.U4)] public uint[] ExceptionInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EXCEPTION_DEBUG_INFO
        {
            public EXCEPTION_RECORD ExceptionRecord;
            public uint dwFirstChance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEBUG_EVENT
        {
            public int dwDebugEventCode;
            public int dwProcessId;
            public int dwThreadId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
            byte[] debugInfo;

            public EXCEPTION_DEBUG_INFO Exception
            {
                get { return GetDebugInfo<EXCEPTION_DEBUG_INFO>(); }
            }

            public CREATE_THREAD_DEBUG_INFO CreateThread
            {
                get { return GetDebugInfo<CREATE_THREAD_DEBUG_INFO>(); }
            }

            public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
            {
                get { return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>(); }
            }

            public EXIT_THREAD_DEBUG_INFO ExitThread
            {
                get { return GetDebugInfo<EXIT_THREAD_DEBUG_INFO>(); }
            }

            public EXIT_PROCESS_DEBUG_INFO ExitProcess
            {
                get { return GetDebugInfo<EXIT_PROCESS_DEBUG_INFO>(); }
            }

            public LOAD_DLL_DEBUG_INFO LoadDll
            {
                get { return GetDebugInfo<LOAD_DLL_DEBUG_INFO>(); }
            }

            public UNLOAD_DLL_DEBUG_INFO UnloadDll
            {
                get { return GetDebugInfo<UNLOAD_DLL_DEBUG_INFO>(); }
            }

            public OUTPUT_DEBUG_STRING_INFO DebugString
            {
                get { return GetDebugInfo<OUTPUT_DEBUG_STRING_INFO>(); }
            }

            public RIP_INFO RipInfo
            {
                get { return GetDebugInfo<RIP_INFO>(); }
            }

            private T GetDebugInfo<T>() where T : struct
            {
                var structSize = Marshal.SizeOf(typeof(T));
                var pointer = Marshal.AllocHGlobal(structSize);
                Marshal.Copy(debugInfo, 0, pointer, structSize);

                var result = Marshal.PtrToStructure(pointer, typeof(T));
                Marshal.FreeHGlobal(pointer);
                return (T)result;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "WaitForDebugEvent")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WaitForDebugEvent(ref DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern bool ContinueDebugEvent(uint dwProcessId, uint dwThreadId, uint dwContinueStatus);

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        public enum dwCreationFlags
        {
            CREATE_SUSPENDED = 0x04,
            DEBUG_ONLY_THIS_PROCESS = 0x02,
            DEBUG_PROCESS = 0x01
        }
        
        public enum dwDebugEventCode
        {
            EXIT_PROCESS_DEBUG_EVENT = 0x05
        }
    }
}
