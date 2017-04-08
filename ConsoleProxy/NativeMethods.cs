// <copyright file="NativeMethods.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy
{
    using System.Runtime.InteropServices;

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
    /// Contains P/Invokes and associated types
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// A delegate type to be used as the handler routine for <see cref="SetConsoleCtrlHandler"/>. 
        /// </summary>
        /// <param name="eventType">A CtrlType for the ConsoleEventDelegate</param>
        /// <returns>If the function handles the control signal, it should return TRUE. If it returns FALSE, the next handler function in the list of handlers for this process is used.</returns>
        internal delegate bool ConsoleEventDelegate(CtrlType eventType);

        /// <summary>
        /// <para>Adds or removes an <see cref="ConsoleEventDelegate"/> delegate function 
        /// from the list of handler functions for the calling process.</para>
        /// </summary>
        /// <param name="callback">A delegate to be used as the handler routine for <see cref="SetConsoleCtrlHandler"/></param>
        /// <param name="add"><para>If this parameter is TRUE, the handler is added; if it is FALSE, the handler is removed.</para></param>
        /// <returns><c>true</c> if the function succeeds.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        ////[DllImport("kernel32.dll", SetLastError = true)]
        ////internal static extern bool GenerateConsoleCtrlEvent(CtrlType dwCtrlEvent, uint dwProcessGroupId);
    }
}
