// <copyright file="Program.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Timers;
    using Eegee.ConsoleProxy.LineProcessor;
    using Eegee.ConsoleProxy.LineProcessor.Interfaces;

    /// <summary>
    /// A CtrlType for the ConsoleEventDelegate
    /// </summary>
    internal enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    /// <summary>
    /// The main program
    /// </summary>
    internal sealed class Program
    {
        private const string SettingLineProcessor = "LineProcessorAssemblyFile";
        private const int ErrorInvalidCommandLine = 0x667;
        private static ConsoleEventDelegate handler;
        private static ProcessState processState;
        private static ILineProcessor processor;
        private static Process process;
        private static Timer timer;
        private static ConsoleWriter consoleWriter = new ConsoleWriter();
        private static string defaultTitle = Console.Title;

        private delegate bool ConsoleEventDelegate(CtrlType eventType);

        /// <summary>
        /// The main process entry
        /// </summary>
        /// <param name="args">The command line arguments</param>
        public static void Main(string[] args)
        {
            ////AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.CancelKeyPress += Console_CancelKeyPress;
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            processState = new ProcessState();
            processor = CreateProcessor(processState);
            if (processor == null)
            {
                consoleWriter.WriteMessage("Could not create the LineProcessor. Please check your setting \"" + SettingLineProcessor + "\".");
                Environment.Exit(3);
            }

            if (args.Length == 0)
            {
                ShowCLIUsage();
            }

            string command = args[0];
            if (command.Equals("--help", StringComparison.InvariantCultureIgnoreCase) ||
                command.Equals("/?", StringComparison.InvariantCultureIgnoreCase) ||
                command.Equals("-?", StringComparison.InvariantCultureIgnoreCase))
            {
                ShowCLIUsage();
            }

            string arguments = GetProgramArguments(args);

            timer = new Timer(1000);
            timer.Elapsed += TimerElapsed;

            do
            {
                if (processState.TooManyRestarts)
                {
                    consoleWriter.WriteMessage("Too many restarts in a short time. Aborting.", ConsoleColor.Red);
                    break;
                }
                else
                {
                    Console.ResetColor();

                    processState.HasErrored = false;
                    processState.IsClosing = false;
                    processState.RegisterRestart();
                    process = SetupProcess(command, arguments);

                    consoleWriter.WriteMessage(string.Format("Starting process {0}...", command), ConsoleColor.White);

                    bool newProcess = false;
                    try
                    {
                        newProcess = process.Start();
                    }
                    catch (Win32Exception)
                    {
                        consoleWriter.WriteMessage(string.Format("{0} not found", command), ConsoleColor.Red);
                        Console.WriteLine();
                        ShowCLIUsage();
                    }

                    if (newProcess)
                    {
                        timer.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                    else
                    {
                        consoleWriter.WriteMessage("Process already running.", ConsoleColor.White);
                        processState.MustRestart = true;
                        CloseProcess();
                    }
                }
            }
            while (processState.MustRestart);

            Console.ResetColor();
        }

        private static ILineProcessor CreateProcessor(IProcessState processState)
        {
            ILineProcessor result = null;
            string processorAssemblyFilePath = Properties.Settings.Default.LineProcessorAssemblyFile;
            if (string.IsNullOrEmpty(Path.GetExtension(processorAssemblyFilePath)))
            {
                processorAssemblyFilePath = Path.ChangeExtension(processorAssemblyFilePath, "dll");
            }

            if (!File.Exists(processorAssemblyFilePath))
            {
                processorAssemblyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, processorAssemblyFilePath);
            }

            if (File.Exists(processorAssemblyFilePath))
            {
                Assembly processorAssembly = Assembly.LoadFrom(processorAssemblyFilePath);

                string processorTypeName = Properties.Settings.Default.LineProcessorType;
                foreach (Type type in processorAssembly.GetExportedTypes())
                {
                    if (type.FullName == processorTypeName)
                    {
                        result = (ILineProcessor)Activator.CreateInstance(type, processState);
                        break;
                    }
                }
            }
            else
            {
                consoleWriter.WriteMessage("Setting \"" + SettingLineProcessor + "\" is missing. Please add this setting to app.config. Specify the file path for the assembly containing your LineProcessor class.");
                Environment.Exit(2);
            }

            return result;
        }

        /// <summary>
        /// Shows command line interface usage information in the console and exits
        /// </summary>
        private static void ShowCLIUsage()
        {
            string exename = System.AppDomain.CurrentDomain.FriendlyName;
            object[] assemblyTitleAttributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string title;
            if (assemblyTitleAttributes != null && assemblyTitleAttributes.Length > 0)
            {
                AssemblyTitleAttribute assemblyTitleAttribute = assemblyTitleAttributes[0] as AssemblyTitleAttribute;
                title = assemblyTitleAttributes != null ? assemblyTitleAttribute.Title : exename;
            }

            Console.WriteLine("ConsoleProxy (c) by Erik Jan Meijer");
            if (processor != null)
            {
                Console.WriteLine(processor.Vanity);
            }

            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.Write(string.Format("{0} ", exename));
            if (processor != null)
            {
                Console.WriteLine(processor.CommandLineUsage);
            }

            Environment.Exit(ErrorInvalidCommandLine);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        ////[DllImport("kernel32.dll", SetLastError = true)]
        ////private static extern bool GenerateConsoleCtrlEvent(CtrlType dwCtrlEvent, uint dwProcessGroupId);

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            consoleWriter.WriteMessage("Exception occurred.", ConsoleColor.White);
            Console.WriteLine(e.ExceptionObject.ToString());
            processState.MustRestart = false;
            CloseProcess();
            Environment.Exit(1);
        }

        private static string GetProgramArguments(string[] args)
        {
            string arguments;
            if (args.Length > 1)
            {
                string program = Environment.GetCommandLineArgs()[0];
                string rawArgs = Environment.CommandLine;
                int index = rawArgs.IndexOf(program);
                if (index < 0)
                {
                    index = 0;
                }

                rawArgs = rawArgs.Substring(index + program.Length).TrimStart(new char[] { '"', ' ' });

                string command = args[0];
                arguments = rawArgs.Substring(index + command.Length).TrimStart(new char[] { '"', ' ' });
            }
            else
            {
                arguments = null;
            }

            return arguments;
        }

        private static Process SetupProcess(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            if (!string.IsNullOrEmpty(arguments) && !string.IsNullOrEmpty(arguments.Trim()))
            {
                startInfo.Arguments = arguments;
            }

            startInfo.FileName = command;

            Process process = new Process() { StartInfo = startInfo };
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += OutputDataReceived;
            process.ErrorDataReceived += ErrorDataReceived;
            process.Exited += Exited;
            return process;
        }

        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            DataReceived(LineType.Output, e.Data);
        }

        private static void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            DataReceived(LineType.Error, e.Data);
        }

        private static void DataReceived(LineType lineType, string line)
        {
            if (line == null)
            {
                return;
            }

            ConsoleColor? foregroundColor;
            ConsoleColor? backgroundColor;
            line = processor.ProcessLine(lineType, line, out foregroundColor, out backgroundColor);

            WriteReceivedLine(line, foregroundColor, backgroundColor);
        }

        private static void WriteReceivedLine(string line, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
        {
            if (!processState.IsClosing)
            {
                consoleWriter.WriteMessage(line, foregroundColor, backgroundColor);
                if (!string.IsNullOrEmpty(line) && !string.IsNullOrEmpty(line.Trim()))
                {
                    processState.LastOutput = DateTime.Now;
                    processState.LastLine = line;
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                consoleWriter.WriteMessage("Received ^C.", ConsoleColor.White);
                CloseProcess();
            }
        }

        private static bool ConsoleEventCallback(CtrlType eventType)
        {
            if (eventType > CtrlType.CTRL_C_EVENT)
            {
                consoleWriter.WriteMessage("Received Close Event.", ConsoleColor.White);
                CloseProcess();
            }

            return false;
        }

        private static void Exited(object sender, EventArgs e)
        {
            consoleWriter.WriteMessage("Process closed.", ConsoleColor.White);
            processState.IsClosing = false;
            timer.Stop();
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!processState.IsClosing)
            {
                DateTime now = DateTime.Now;
                if (processState.LastOutput.HasValue && processState.LastOutput < now.AddMinutes(-1))
                {
                    consoleWriter.WriteMessage("Last output was more than one minute ago.", ConsoleColor.Yellow);
                    processState.MustRestart = true;
                    CloseProcess();
                }

                ////if (restartTime < now.AddSeconds(-20))
                ////{
                ////    consoleWriter.WriteMessage("Testje.", ConsoleColor.Yellow);
                ////    mustRestart = true;
                ////    CloseProcess();
                ////}

                if (processState.HasErrored && process != null && !process.HasExited)
                {
                    consoleWriter.WriteMessage("Received error.", ConsoleColor.Magenta);
                    processState.MustRestart = true;
                    CloseProcess();
                }
            }

            Console.Title = processor.WindowTitle;
        }

        private static void CloseProcess()
        {
            if (process != null)
            {
                ////GenerateConsoleCtrlEvent(CtrlType.CTRL_C_EVENT, 0);
                processState.IsClosing = true;
                if (!process.HasExited)
                {
                    consoleWriter.WriteMessage("Closing process...", ConsoleColor.White);
                    if (!process.CloseMainWindow())
                    {
                        process.Kill();
                    }
                    else
                    {
                        if (!process.WaitForExit(20000))
                        {
                            consoleWriter.WriteMessage("CloseMainWindow failed, terminating...", ConsoleColor.Magenta);
                            process.Kill();
                        }
                    }

                    process.Close();
                    process = null;
                }
                else
                {
                    consoleWriter.WriteMessage("Process already closed.", ConsoleColor.White);
                }

                timer.Stop();
                processState.IsClosing = false;
                Console.ResetColor();
            }
        }
    }
}