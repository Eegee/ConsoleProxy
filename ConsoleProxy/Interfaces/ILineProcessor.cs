// <copyright file="ILineProcessor.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>

namespace Eegee.ConsoleProxy.LineProcessor.Interfaces
{
    using System;

    /// <summary>
    /// An interface for a processing class which processes a text line
    /// </summary>
    public interface ILineProcessor
    {
        /// <summary>
        /// Gets a string with command line usage information.
        /// </summary>
        string CommandLineUsage { get; }

        /// <summary>
        /// Gets a string with vanity information.
        /// </summary>
        string Vanity { get; }

        /// <summary>
        /// Gets a window title for this processor
        /// </summary>
        /// <returns>The window title</returns>
        string WindowTitle { get; }

        /// <summary>
        /// Processes a received line
        /// </summary>
        /// <param name="lineType">The type of line</param>
        /// <param name="line">The line to process</param>
        /// <param name="foregroundColor">Optional foreground ConsoleColor for the output line</param>
        /// <param name="backgroundColor">Optional background ConsoleColor for the output line</param>
        /// <returns>The processed line</returns>
        string ProcessLine(LineType lineType, string line, out ConsoleColor? foregroundColor, out ConsoleColor? backgroundColor);
    }
}
