// <copyright file="DefaultProcessor.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Defaultprocessorlib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Eegee.ConsoleProxy.LineProcessor;
    using Eegee.ConsoleProxy.LineProcessor.Interfaces;

    /// <summary>
    /// An example of a class derived from LineProcessor
    /// </summary>
    public class DefaultProcessor : LineProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProcessor" /> class
        /// </summary>
        /// <param name="processState">an IProcessState to get state information from</param>
        public DefaultProcessor(IProcessState processState)
            : base(processState)
        {
        }

        /// <summary>
        /// Gets a string with command line usage information.
        /// </summary>
        public override string CommandLineUsage
        {
            get { return "<path-to-proxified.exe> <proxified-arguments>"; }
        }

        /// <summary>
        /// Gets a string with vanity information.
        /// </summary>
        public override string Vanity
        {
            get { return "DefaultProcessor by Erik Jan Meijer"; }
        }

        /// <summary>
        /// Gets a window title for this processor
        /// </summary>
        /// <returns>The window title</returns>
        public override string WindowTitle
        {
            get { return Console.Title; }
        }

        /// <summary>
        /// Processes a received line
        /// </summary>
        /// <param name="lineType">The type of line</param>
        /// <param name="line">The line to process</param>
        /// <param name="foregroundColor">Optional foreground ConsoleColor for the output line</param>
        /// <param name="backgroundColor">Optional background ConsoleColor for the output line</param>
        /// <returns>The processed line</returns>
        public override string ProcessLine(LineType lineType, string line, out ConsoleColor? foregroundColor, out ConsoleColor? backgroundColor)
        {
            foregroundColor = null;
            backgroundColor = null;
            return line;
        }
    }
}
