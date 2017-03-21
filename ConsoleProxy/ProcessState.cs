// <copyright file="ProcessState.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.ConsoleProxy
{
    using System;
    using Cyotek.Collections.Generic;
    using Eegee.ConsoleProxy.LineProcessor.Interfaces;

    /// <summary>
    /// State information on errors, output and restarts of the proxified application
    /// </summary>
    internal sealed class ProcessState : IProcessState
    {
        private DateTime restartTime = DateTime.MinValue;
        private int restarts = 0;
        private CircularBuffer<string> lastLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessState" /> class
        /// </summary>
        public ProcessState()
        {
            this.lastLines = new CircularBuffer<string>(50);
        }

        /// <summary>
        /// Gets or sets the last line
        /// </summary>
        public string LastLine
        {
            get
            {
                return this.lastLines.PeekLast();
            }

            set
            {
                this.lastLines.Put(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the proxified process must be restarted
        /// </summary>
        public bool MustRestart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the proxified process has generated an error
        /// </summary>
        public bool HasErrored { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the proxified process is closing or being closed
        /// </summary>
        public bool IsClosing { get; set; }

        /// <summary>
        /// Gets a value indicating whether the proxified process has been restarted too many times in too short a time.
        /// </summary>
        public bool TooManyRestarts
        {
            get
            {
                bool result = this.restarts > 3 && DateTime.Now - this.restartTime <= TimeSpan.FromSeconds(120);
                if (result)
                {
                    this.MustRestart = false;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the DateTime of the last line output
        /// </summary>
        internal DateTime? LastOutput { get; set; }

        /// <summary>
        /// Sets properties to indicate the proxified process will restart very soon
        /// </summary>
        public void RegisterRestart()
        {
            this.restartTime = DateTime.Now;
            this.restarts++;
            this.MustRestart = false;
        }
    }
}
