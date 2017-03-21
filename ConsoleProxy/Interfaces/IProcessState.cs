// <copyright file="IProcessState.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy.LineProcessor.Interfaces
{
    using System;

    /// <summary>
    /// State information on errors, output and restarts of the proxified application
    /// </summary>
    public interface IProcessState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the proxified process has generated an error
        /// </summary>
        bool HasErrored { get; set; }

        /// <summary>
        /// Gets the last line
        /// </summary>
        string LastLine { get; }
   }
}
