﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

using YAETWi.Helper;
using YAETWi.Core;

namespace YAETWi
{
    class Program
    {
        private static Dictionary<string, string> parameters = new Dictionary<string, string>();
        public static HashSet<int> pids = new HashSet<int>();
        private static int extConn = 0;
        public static int events = 0;
        public static bool kernel = false;
        public static string kProvider = "";

        static void Main(string[] args)
        {
            if (!Utils.isPrivileged())
            {
                Console.WriteLine("For this program to work properly, you must run it as an administrator.");
                Environment.Exit(1);
            }

            parameters = Helper.ArgParser.parse(args);
            if (parameters.ContainsKey(ArgParser.Parameters.kernel.ToString()))
                kernel = true;

            TraceEventSession tcpipKernelSession = null;
            if (parameters.ContainsKey(ArgParser.Parameters.externalIP.ToString()))
            {
                tcpipKernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
                tcpipKernelSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                Logger.printInfo("Starting TCPIP Session");

                tcpipKernelSession.Source.Kernel.TcpIpAccept += ((TcpIpConnectTraceData data) =>
                {
                    Logger.logKernel(data);
                    Logger.printEvent(String.Format("conn: {0} -> :{1}\tproc: {2} -> {3}\n", data.daddr, data.sport, data.ProcessID, data.ProcessName));

                    if (data.daddr.ToString() == parameters[ArgParser.Parameters.externalIP.ToString()])
                    {
                        pids.Add(data.ProcessID);
                        extConn++;
                    }
                });

                Task.Run(() => tcpipKernelSession.Source.Process());

            } else if (parameters.ContainsKey(ArgParser.Parameters.pid.ToString()))
            {
                ArgParser.readPids(parameters[ArgParser.Parameters.pid.ToString()], pids);
            }
            else
            {
                Helper.Help.print();
                Environment.Exit(0);
            }

            TraceEventSession kernelSession = null;
            TraceEventSession allProvidersSession = null;
            if (kernel)
            {
                ETW.traceKernel(kernelSession);
            }
            else
            {
                ETW.traceAllProviders(allProvidersSession);
            }

            Helper.Help.keystrokes();

            var start = DateTime.Now;

            while (true)
            {
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    var end = DateTime.Now;
                    Console.WriteLine(String.Format("\n[*] Session started: {0}\n[*] Session ended: {1}\n[*] Overall time: {2}\n[*] Overall external connections: {3}\n[*] # of events: {4}\n",
                        start,
                        end,
                        end - start,
                        extConn,
                        events));
                    try
                    {
                        tcpipKernelSession?.Dispose();
                        allProvidersSession?.Dispose();
                        kernelSession?.Dispose();
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                    Environment.Exit(0);
                };

                var cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.R:
                        {
                            Console.Write("Enter provider name:");
                            string p = Console.ReadLine();
                            if (kernel)
                            {
                                kProvider = p;
                                Console.WriteLine(String.Format("Started listener for {0} provider -> no historical data for kernel events, only live logging.\n", kProvider));
                            }
                            else
                            {
                                Logger.dumpETWProvider(p);
                            }

                            Helper.Help.keystrokes();
                        }
                        break;
                    case ConsoleKey.W:
                        {
                            Console.Write("Enter provider name:");
                            string p = Console.ReadLine();
                            Console.Write("Enter directory to save file in (if empty, the file will be kept in the current directory):");
                            string d = Console.ReadLine();
                            Logger.writeETWProvider(p, d);

                            Helper.Help.keystrokes();
                        }
                        break;
                    case ConsoleKey.D:
                        {
                            Logger.printPids();
                            if (kernel)
                            {
                                Logger.dumpKernelEvents();
                                kProvider = "";
                            }
                            else
                                Logger.dumpETWProviders();

                            Helper.Help.keystrokes();
                        }
                        break;
                    case ConsoleKey.C:
                        {
                            Program.pids = new HashSet<int>();
                            ETW.refreshCollection();
                            Logger.printInfo("Purged collections");

                            Helper.Help.keystrokes();
                        }
                        break;
                    case ConsoleKey.P:
                        {
                            Console.Write("Enter comma-separated list of pids to monitor:");
                            string input = Console.ReadLine();
                            ETW.refreshCollection();
                            ArgParser.readPids(input, pids);

                            Helper.Help.keystrokes();
                        }
                        break;
                    case ConsoleKey.H:
                        {
                            Helper.Help.keystrokes();
                        }
                        break;
                }
            }
        }
    }
}
