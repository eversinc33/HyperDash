using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace HyperDash
{
    class Program
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_SPACE = 0x20;
        private const int VK_C = 0x43;
        private const int WH_KEYBOARD_LL = 13;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool isDashing = false;
        private static Process gameProcess;

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        static void DoDash()
        {
            while (isDashing)
            {
                PostMessage(gameProcess.MainWindowHandle, WM_KEYDOWN, VK_SPACE, 0);
                Thread.Sleep(16);
                PostMessage(gameProcess.MainWindowHandle, WM_KEYUP, VK_SPACE, 0);
                Thread.Sleep(250);
            }
        }

        [STAThread]
        static void Main()
        {
            AllocConsole();
            
            _hookID = SetHook(_proc); // Keyboard Hook

            Console.WriteLine("" +
"__  __ _  _ _____ _____ _____ _____  ___    __ __  __ \n" +
"||==|| \\\\// ||_// ||==  ||_// ||  ) ||=||  ((  ||==|| \n" +
"||  ||  //  ||    ||___ || \\\\ ||_// || || \\_)) ||  || \n"
            );
            Console.WriteLine("[i] Hold 'C' to start chain-dashing. Chain-Dash has to be set to 'space'");
            Console.WriteLine("    Waiting for Hyper Light Drifter to start.");
            Console.WriteLine("    Hold Ctrl+C to quit.\n");

            while (true) // <3
            {
                Process[] processes = Process.GetProcessesByName("HyperLightDrifter");
                Thread.Sleep(1000);

                if (processes.Length > 0)
                {
                    Console.WriteLine("[+] Found Hyper Light Drifter process. Chain-Dash shortcut installed.\n");
                    gameProcess = processes[0];
                    Application.Run();
                }
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            // on c press, start chain dashing, stop when c up
            if (vkCode == VK_C)
            {
                if (!isDashing && nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    Thread dashThread = new Thread(new ThreadStart(DoDash)); // Thread to send spacebar-presses
                    dashThread.Start();
                    isDashing = true;
                }

                else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
                {
                    isDashing = false;
                }
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
