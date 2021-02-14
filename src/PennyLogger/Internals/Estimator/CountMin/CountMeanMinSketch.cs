// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace PennyLogger.Internals.Estimator.CountMin
{
    /// <summary>
    /// The count-mean-min sketch is an improvement on <see cref="CountMinSketch"/> that subtracts an estimate of the
    /// bias to prevent unnecessarily large values due to hash collisions
    /// </summary>
    public class CountMeanMinSketch : CountMinSketch
    {
        /// <inheritdoc/>
        public CountMeanMinSketch(double epsilon, double delta) : base(epsilon, delta)
        {
        }

        private ulong Count;

        /// <inheritdoc/>
        public override long Estimate(Hash hash)
        {
            // Count-mean-min sketch implementation
            var hashes = hash.CalculateDoubleHashes((int)Rows, Cols - 1);
            var values = new List<double>();

            for (uint row = 0; row < Rows; row++)
            {
                uint index = row * Cols + (uint)hashes[row];
                uint count = Matrix[index];
                values.Add((double)count - ((double)Count - count) / ((double)Cols - 1.0));
            }

            // Goyal et al. recommend taking the minimum of both estimates
            return Math.Min(Median(values), base.Estimate(hash));
        }

        /// <inheritdoc/>
        public override IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            Count++;
            return base.TryIncrementAndEstimate(hash, out estimate);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Count = 0;
            base.Clear();
        }

        private static ushort Median(List<double> values)
        {
            values.Sort();
            if (values.Count % 2 == 0)
            {
                double value1 = values[values.Count / 2 - 1];
                double value2 = values[values.Count / 2];
                return (ushort)((value1 + value2) / 2.0);
            }
            else
            {
                return (ushort)Math.Round(values[values.Count / 2]);
            }
        }
    }
}
