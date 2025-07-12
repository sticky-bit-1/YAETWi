using System;

namespace YAETWix
{
    public class Program
    {
        private static void usage()
        {
            Console.WriteLine(String.Format("Usage:\n" +
                "\t.\\YAETWix.exe <\"full_path_to_binary + arguments\">\n" +
                "Example:\n" +
                "\t.\\YAETWix.exe \"c:\\windows\\system32\\cmd.exe /c whoami\"\n" +
                "Keystrokes:\n" +
                "\tr -> resume process"));
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                usage();
                Environment.Exit(0);
            }
            
            Win32.STARTUPINFO si = new Win32.STARTUPINFO();
            Win32.PROCESS_INFORMATION pi = new Win32.PROCESS_INFORMATION();
            
            var start = DateTime.Now;

            bool res = Win32.CreateProcess(null, 
                args[0] + "\0", 
                IntPtr.Zero, 
                IntPtr.Zero, 
                false,
                (uint)Win32.dwCreationFlags.DEBUG_ONLY_THIS_PROCESS,
                IntPtr.Zero, 
                null, 
                ref si, 
                out pi);

            Console.WriteLine(String.Format("[!] Process has been created\n" +
                "[*] Creation time: {0}\n" +
                "[*] PID: {1}\n[*] to resume process -> enter 'r'\n" +
                "[*] to terminate process -> enter CTRL+C", start, pi.dwProcessId));

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                Console.WriteLine(String.Format("[!] Terminating process {0}\n", pi.dwProcessId));
                Win32.TerminateProcess(pi.hProcess, 0);
                Environment.Exit(0);
            };

            while (true)
            {
                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.R:
                    {
                            Console.WriteLine(String.Format("\n[!] Process {0} has been resumed\n", pi.dwProcessId));

                            Win32.DEBUG_EVENT debug_event = new Win32.DEBUG_EVENT();

                            while (Win32.WaitForDebugEvent(ref debug_event, uint.MaxValue))
                            {
                                switch (debug_event.dwDebugEventCode)
                                {
                                    case (int)Win32.dwDebugEventCode.EXIT_PROCESS_DEBUG_EVENT:
                                        {
                                            var end = DateTime.Now;
                                            Console.WriteLine("Catched process trying to finish. Termination time: [{0}]", end);
                                            goto termination;
                                        }
                                        break;
                                    default:
                                        Win32.ContinueDebugEvent((uint)debug_event.dwProcessId, (uint)debug_event.dwThreadId, 0x00010002);
                                        break;
                                }
                            }
                     }
                     break;
                }
            }
        termination:
            Environment.Exit(0);
        }
    }
}
