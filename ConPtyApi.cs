using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PtyLauncher {
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD { public short X; public short Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFOEX {
        public STARTUPINFO StartupInfo;
        public IntPtr lpAttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO {
        public int cb; public string lpReserved; public string lpDesktop; public string lpTitle;
        public int dwX; public int dwY; public int dwXSize; public int dwYSize;
        public int dwXCountChars; public int dwYCountChars; public int dwFillAttribute;
        public int dwFlags; public short wShowWindow; public short cbReserved2;
        public IntPtr lpReserved2; public IntPtr hStdInput; public IntPtr hStdOutput; public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION {
        public IntPtr hProcess; public IntPtr hThread; public int dwProcessId; public int dwThreadId;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int CreatePseudoConsole(COORD size, IntPtr hInput, IntPtr hOutput, uint dwFlags, out IntPtr phPC);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, uint dwFlags, ref IntPtr lpSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, IntPtr lpPipeAttributes, uint nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

    public static void Run(Stream networkStream, short width, short height) {
        IntPtr hInR, hInW, hOutR, hOutW;
        CreatePipe(out hInR, out hInW, IntPtr.Zero, 0);
        CreatePipe(out hOutR, out hOutW, IntPtr.Zero, 0);

        IntPtr hPC;
        CreatePseudoConsole(new COORD { X = width, Y = height }, hInR, hOutW, 0, out hPC);

        Task.Run(() => {
            byte[] buf = new byte[1024];
            int read;
            try {
                while ((read = networkStream.Read(buf, 0, buf.Length)) > 0) {
                    uint written;
                    WriteFile(hInW, buf, (uint)read, out written, IntPtr.Zero);
                }
            } catch {}
        });

        Task.Run(() => {
            byte[] buf = new byte[1024];
            uint read;
            try {
                while (ReadFile(hOutR, buf, (uint)buf.Length, out read, IntPtr.Zero) && read > 0) {
                    networkStream.Write(buf, 0, (int)read);
                    networkStream.Flush();
                }
            } catch {}
        });

        var si = new STARTUPINFOEX();
        si.StartupInfo.cb = Marshal.SizeOf(si);
        IntPtr lpSize = IntPtr.Zero;
        InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
        si.lpAttributeList = Marshal.AllocHGlobal(lpSize);
        InitializeProcThreadAttributeList(si.lpAttributeList, 1, 0, ref lpSize);
        UpdateProcThreadAttribute(si.lpAttributeList, 0, (IntPtr)0x00020016, hPC, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

        PROCESS_INFORMATION pi;
        CreateProcess(null, "cmd.exe", IntPtr.Zero, IntPtr.Zero, true, 0x00080000, IntPtr.Zero, null, ref si, out pi);
    }
}
