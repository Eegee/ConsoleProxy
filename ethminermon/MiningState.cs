// <copyright file="MiningState.cs" company="Eegee">
//   Copyright Erik Jan Meijer
// </copyright>
// <author>Erik Jan Meijer</author>
namespace Eegee.Ethminermon
{
    using Cyotek.Collections.Generic;

    /// <summary>
    /// Information about the state of mining
    /// </summary>
    public class MiningState
    {
        private CircularBuffer<decimal> hashrates;
        private decimal hashrate = 0.0M;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiningState" /> class
        /// </summary>
        public MiningState()
        {
            this.hashrates = new CircularBuffer<decimal>(60 * 5);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a solution has just been mined or not
        /// </summary>
        public bool SolutionFound { get; set; }

        /// <summary>
        /// Gets or sets the last hash rate
        /// </summary>
        public decimal Hashrate
        {
            get
            {
                if (this.hashrates.Size > 0)
                {
                    this.hashrate = this.hashrates.Peek();
                }

                return this.hashrate;
            }

            set
            {
                this.hashrate = value;
                this.hashrates.Put(this.hashrate);
            }
        }

        /// <summary>
        /// Gets the average hash rate from the proxified ethminer
        /// </summary>
        /// <returns>Average hash rate</returns>
        public decimal AverageHashrate()
        {
            decimal result = this.hashrate;
            int bufsize = this.hashrates.Size;
            if (bufsize > 0)
            {
                decimal sum = 0;
                foreach (decimal hashrate in this.hashrates)
                {
                    sum += hashrate;
                }

                result = sum / this.hashrates.Size;
            }

            return result;
        }
    }
}