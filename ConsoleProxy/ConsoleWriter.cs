// <copyright file="ConsoleWriter.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy
{
    using System;

    /// <summary>
    /// Writes a message to the console, optionally with a specific foreground and/or background color;
    /// </summary>
    internal sealed class ConsoleWriter
    {
        private static object messageLock = new object();

        /// <summary>
        /// Writes a message to the console, optionally with a specific foreground and/or background color;
        /// </summary>
        /// <param name="message">The message to write to the console</param>
        /// <param name="foregroundColor">Optional foreground color</param>
        /// <param name="backgroundColor">Optional background color</param>
        public void WriteMessage(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            if (message == null)
            {
                return;
            }

            ////lock (messageLock)
            ////{
                bool colorchanged = false;
                if (foregroundColor.HasValue)
                {
                    Console.ForegroundColor = foregroundColor.Value;
                    colorchanged = true;
                }

                if (backgroundColor.HasValue)
                {
                    Console.BackgroundColor = backgroundColor.Value;
                    colorchanged = true;
                }

                Console.WriteLine(message);
                if (colorchanged)
                {
                    Console.ResetColor();
                }
            ////}
        }
    }
}
