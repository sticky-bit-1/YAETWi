using System;

namespace YAETWi.Helper
{
    public static class Help
    {
        private static string version = "\nv2.7.2\n";
        public static void usage()
        {
            Console.WriteLine("Version:" + version + "\n" +
            "Usage:\n\t YAETWi.exe\n" +
            "\t\t/externalIP=<IP the connection from which to be scrutinized> | /pid=<comma-separated list of pids to be traced>\n" +
            "\t\t[/kernel]\n\n" +
            "Use YAETWix.exe alongside a program to be tested -> it will provide you with a PID you can then provide as a parameter to YAETWi.exe\n\n" +
            "For kernel events: due to the overwhelming number of events, only live logging is provided, not historical data. " + 
            "Step 1: execute a tested application for the first time to list the kernel events been triggered. " + 
            "Step 2: set up a listener for the particular kernel event via 'r' parameter, then run the application again to trace the events.\n"
            );
        }
        public static void print()
        {
            usage();
            keystrokes();
        }
        public static void keystrokes()
        {
            Console.WriteLine(
                "Keystrokes:\n" +
                "\t 'd' -> (dump) all traced providers\n" +
                "\t 'r' -> (read) provider name to print detailed output for\n" +
                "\t 'w' -> (write) output of particular provider down to a file\n" +
                "\t 'c' -> (clear) all events\n" +
                "\t 'p' -> manually provide comma-separated pids to be traced -> all collections and pids will be purged\n" +
                "\t 'h' -> show (help) menu");
        }
    }
}
