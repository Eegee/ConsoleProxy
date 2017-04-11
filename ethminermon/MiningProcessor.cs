// <copyright file="MiningProcessor.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.Ethminermon
{
    using System;
    using System.Globalization;
    using Eegee.ConsoleProxy.LineProcessor;
    using Eegee.ConsoleProxy.LineProcessor.Interfaces;

    /// <summary>
    /// A LineProcessor specifically for processing output of 'ethminer'.
    /// </summary>
    public class MiningProcessor : LineProcessor
    {
        private MiningState miningState;
        private CultureInfo cultureInfo;
        private int prevcursortop;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiningProcessor" /> class
        /// </summary>
        /// <param name="processState">an IProcessState to get state information from</param>
        public MiningProcessor(IProcessState processState)
            : base(processState)
        {
            this.miningState = new MiningState();
            this.cultureInfo = CultureInfo.GetCultureInfo("en-US");
            this.prevcursortop = 0;
        }

        /// <summary>
        /// Gets a string with command line usage information.
        /// </summary>
        public override string CommandLineUsage
        {
            get
            {
                return "<path-to-ethminer.exe> <arguments-to-ethminer-no-extra-quotes>";
            }
        }

        /// <summary>
        /// Gets a string with vanity information.
        /// </summary>
        public override string Vanity
        {
            get { return "ethminermonlib MiningProcessor by Erik Jan Meijer"; }
        }

        /// <summary>
        /// Gets a window title for this processor
        /// </summary>
        /// <returns>The window title</returns>
        public override string WindowTitle
        {
            get
            {
                decimal realhashrate = this.miningState.AverageHashrate();
                return string.Format(this.cultureInfo.NumberFormat, "ethminer hashrate: {0:0.00} MH/sec", realhashrate);
            }
        }

        /// <summary>
        /// Processes a received line
        /// </summary>
        /// <param name="lineType">The type of line</param>
        /// <param name="line">The line to process</param>
        /// <param name="foregroundcolor">Optional foreground ConsoleColor for the output line</param>
        /// <param name="backgroundcolor">Optional background ConsoleColor for the output line</param>
        /// <returns>The processed line</returns>
        public override string ProcessLine(LineType lineType, string line, out ConsoleColor? foregroundcolor, out ConsoleColor? backgroundcolor)
        {
            foregroundcolor = null;
            backgroundcolor = null;
            if (lineType == LineType.Error)
            {
                int cursortop = Console.CursorTop;

                bool isMiningOn = StringUtility.Contains(line, "Mining on ", this.cultureInfo, CompareOptions.IgnoreCase);
                if (isMiningOn)
                {
                    this.miningState.SolutionFound = false;
                    decimal hashrate = 0.0M;
                    int mhpos = line.IndexOf("MH/s");
                    if (mhpos > -1)
                    {
                        int colonpos = line.LastIndexOf(':', mhpos);
                        if (colonpos > -1)
                        {
                            string hashStr = line.Substring(colonpos + 1, mhpos - colonpos - 1).TrimStart();
                            decimal.TryParse(hashStr, System.Globalization.NumberStyles.Number, this.cultureInfo.NumberFormat, out hashrate);
                        }
                    }

                    bool wasMiningOn = this.WasMiningOn();
                    if (wasMiningOn)
                    {
                        int top = cursortop - 1;
                        if (top > -1 && top >= this.prevcursortop - 1)
                        {
                            Console.CursorTop = top;
                        }
                    }

                    if (hashrate == 0.0M)
                    {
                        line = null;
                    }
                    else
                    {
                        this.miningState.Hashrate = hashrate;
                    }

                    foregroundcolor = ConsoleColor.DarkGray;
                }
                else if (StringUtility.Contains(line, "FAIL", this.cultureInfo, CompareOptions.IgnoreCase) || StringUtility.Contains(line, ":-(", this.cultureInfo))
                {
                    this.miningState.SolutionFound = false;
                    foregroundcolor = ConsoleColor.Red;
                    if (StringUtility.Contains(line, "Read response failed: End of file", this.cultureInfo, CompareOptions.IgnoreCase))
                    {
                        ProcessState.ReadResponseFailed++;
                        if (ProcessState.ReadResponseFailed >= 3)
                        {
                            ProcessState.HasErrored = true;
                            ProcessState.ReadResponseFailed = 0;
                        }
                    }
                    else
                    {
                        ProcessState.HasErrored = true;
                    }
                }
                else if (StringUtility.Contains(line, "B-)", this.cultureInfo))
                {
                    this.miningState.SolutionFound = false;
                    foregroundcolor = ConsoleColor.Green;
                }
                else if (StringUtility.Contains(line, "Solution found", this.cultureInfo, CompareOptions.IgnoreCase))
                {
                    this.miningState.SolutionFound = true;
                    foregroundcolor = ConsoleColor.DarkGreen;
                }
                else
                {
                    if (this.miningState.SolutionFound)
                    {
                        foregroundcolor = ConsoleColor.DarkGreen;
                    }
                    else
                    {
                        foregroundcolor = ConsoleColor.DarkGray;
                    }
                }
            }
            else if (lineType == LineType.Output)
            {
                if (StringUtility.Contains(line, "ETH: 0xeb9310b185455f863f526dab3d245809f6854b4d", this.cultureInfo, CompareOptions.IgnoreCase))
                {
                    Console.ForegroundColor = this.InitialForegroundColor;
                    Console.WriteLine(line);
                    line = Environment.NewLine +
                           "If you like ethminermon/ConsoleProxy, please consider a donation to: " + Environment.NewLine +
                           "ETH: 0x00D5a55E7c9C92148C451dEF736f8116bf4BD7f5" + Environment.NewLine +
                           "BTC: 18iyZAE2kPkDFr8DZGpVQDxiARmyogS8qX" + Environment.NewLine;
                }
                else
                {
                    foregroundcolor = this.InitialForegroundColor;
                }
            }

            this.prevcursortop = Math.Max(this.prevcursortop, Console.CursorTop);
            return line;
        }

        private bool WasMiningOn()
        {
            string prevLine = ProcessState.LastLine;
            bool wasMiningOn = prevLine != null && StringUtility.Contains(prevLine, "Mining on ", this.cultureInfo, CompareOptions.IgnoreCase);
            return wasMiningOn;
        }
    }
}