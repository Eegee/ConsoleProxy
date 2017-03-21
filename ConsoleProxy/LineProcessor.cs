// <copyright file="LineProcessor.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy.LineProcessor
{
    using System;
    using System.Globalization;
    using Eegee.ConsoleProxy.LineProcessor.Interfaces;

    /// <summary>
    /// The type of console line
    /// </summary>
    public enum LineType
    {
        Undefined,
        Input,
        Output,
        Error
    }

    /// <summary>
    /// A processing class which processes a text line
    /// </summary>
    public abstract class LineProcessor : ILineProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineProcessor" /> class
        /// </summary>
        /// <param name="processState">an IProcessState to get state information from</param>
        public LineProcessor(IProcessState processState)
        {
            this.ProcessState = processState;
            this.InitialForegroundColor = Console.ForegroundColor;
            this.InitialBackgroundColor = Console.BackgroundColor;
        }

        /// <summary>
        /// Gets a string with command line usage information.
        /// </summary>
        public abstract string CommandLineUsage { get; }

        /// <summary>
        /// Gets a string with vanity information.
        /// </summary>
        public abstract string Vanity { get; }

        /// <summary>
        /// Gets a window title for this processor
        /// </summary>
        /// <returns>The window title</returns>
        public abstract string WindowTitle { get; }

        /// <summary>
        /// Gets the ProcessState to get state information from
        /// </summary>
        protected IProcessState ProcessState { get; private set; }

        /// <summary>
        /// Gets the foreground ConsoleColor this processor will start with
        /// </summary>
        protected ConsoleColor InitialForegroundColor { get; private set; }

        /// <summary>
        /// Gets the background ConsoleColor this processor will start with
        /// </summary>
        protected ConsoleColor InitialBackgroundColor { get; private set; }

        /// <summary>
        /// Processes a received line
        /// </summary>
        /// <param name="lineType">The type of line</param>
        /// <param name="line">The line to process</param>
        /// <param name="foregroundColor">Optional foreground ConsoleColor for the output line</param>
        /// <param name="backgroundColor">Optional background ConsoleColor for the output line</param>
        /// <returns>The processed line</returns>
        public abstract string ProcessLine(LineType lineType, string line, out ConsoleColor? foregroundColor, out ConsoleColor? backgroundColor);
    }
}
