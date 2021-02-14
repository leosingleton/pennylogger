// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Linq;

namespace PennyLogger.Internals.Estimator.CountMin
{
    /// <summary>
    /// Implementation of the count-min sketch data structure for estimating the frequency of events
    /// </summary>
    /// <remarks>
    /// See https://en.wikipedia.org/wiki/Count-min_sketch
    /// </remarks>
    public class CountMinSketch : IFrequencyEstimator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="epsilon">Relative accuracy factor</param>
        /// <param name="delta">Relative accuracy probability</param>
        public CountMinSketch(double epsilon, double delta)
        {
            Cols = (uint)Math.Ceiling(Math.E / epsilon);
            Rows = (uint)Math.Ceiling(Math.Log(1 / delta));
            Matrix = new ushort[Cols * Rows];

            MaxBytes = TotalBytes;
        }

        /// <summary>
        /// Columns in <see cref="Matrix"/>
        /// </summary>
        protected readonly uint Cols;

        /// <summary>
        /// Rows in <see cref="Matrix"/>
        /// </summary>
        protected readonly uint Rows;

        /// <summary>
        /// The matrix used to store the values for the count-min sketch. This matrix is stored in column-major order
        /// and its dimensions are held in <see cref="Cols"/> and <see cref="Rows"/>.
        /// </summary>
        protected readonly ushort[] Matrix;

        /// <summary>
        /// This property is ignored. The memory usage is completely determined at creation time based on constructor
        /// parameters.
        /// </summary>
        public long MaxBytes { get; set; }

        /// <inheritdoc/>
        public long MaxCount => ushort.MaxValue;

        /// <inheritdoc/>
        public long TotalBytes => Cols * Rows * sizeof(ushort);

        /// <inheritdoc/>
        public long BytesUsed => Matrix.LongCount(entry => entry > 0) * sizeof(ushort);

        /// <inheritdoc/>
        public virtual long Estimate(Hash hash)
        {
            var hashes = hash.CalculateDoubleHashes((int)Rows, Cols - 1);
            ushort result = ushort.MaxValue;
            for (uint row = 0; row < Rows; row++)
            {
                uint index = row * Cols + (uint)hashes[row];
                result = Math.Min(result, Matrix[index]);
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual IncrementResult TryIncrementAndEstimate(Hash hash, out long estimate)
        {
            var hashes = hash.CalculateDoubleHashes((int)Rows, Cols - 1);
            bool overflow = false;

            estimate = Estimate(hash) + 1;

            for (uint row = 0; row < Rows; row++)
            {
                uint index = row * Cols + (uint)hashes[row];
                if (Matrix[index] < ushort.MaxValue)
                {
                    Matrix[index]++;
                }
                else
                {
                    overflow = true;
                }
            }

            return overflow ? IncrementResult.Overflow : IncrementResult.Success;
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            Array.Clear(Matrix, 0, Matrix.Length);
        }

        /// <summary>
        /// Adds the values from another count-min sketch to this estimator
        /// </summary>
        /// <param name="cms">Another count-min sketch instance</param>
        public void Add(CountMinSketch cms)
        {
            if (Rows != cms.Rows || Cols != cms.Cols)
            {
                throw new ArgumentException("Argument has mismatched Rows/Cols", nameof(cms));
            }

            for (uint row = 0; row < Rows; row++)
            {
                for (uint col = 0; col < Cols; col++)
                {
                    uint index = row * Cols + col;
                    int sum = Matrix[index] + cms.Matrix[index];
                    Matrix[index] = (ushort)Math.Min(sum, ushort.MaxValue);
                }
            }
        }
    }
}
