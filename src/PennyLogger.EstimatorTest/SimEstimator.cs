// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator;
using System;
using System.Collections.Generic;

namespace PennyLogger.EstimatorTest
{
    internal class SimEstimator
    {
        public SimEstimator(string name, IFrequencyEstimator instance)
        {
            Name = name;
            Instance = instance;
        }

        public void SimulateValue(Hash hash)
        {
            var result = Instance.TryIncrementAndEstimate(hash, out _);
            _ = (result switch
            {
                IncrementResult.Success => Successes++,
                IncrementResult.Overflow => Overflows++,
                IncrementResult.NoCapacity => NoCapacities++,
                _ => throw new Exception()
            });
        }

        public void WriteSummary(IDictionary<Guid, long> actual)
        {
            Console.WriteLine($"{Name}:");
            Console.WriteLine($"  Return Codes:");
            Console.WriteLine($"    Success: {Successes}");
            Console.WriteLine($"    Overflow: {Overflows}");
            Console.WriteLine($"    NoCapacity: {NoCapacities}");

            long totalError = 0L;
            long totalErrorExOverflow = 0L;
            long totalCount = 0L;
            long maxError = 0L;
            long maxErrorExOverflow = 0L;
            foreach (var kvp in actual)
            {
                var hash = Hash.Create(kvp.Key);
                var estimate = Instance.Estimate(hash);
                long expected = kvp.Value;
                long expectedExOverflow = Math.Min(kvp.Value, Instance.MaxCount);
                long error = Math.Abs(estimate - expected);
                long errorExOverflow = Math.Abs(estimate - expectedExOverflow);
                totalError += error;
                totalErrorExOverflow += errorExOverflow;
                totalCount += expected;
                maxError = Math.Max(maxError, error);
                maxErrorExOverflow = Math.Max(maxErrorExOverflow, errorExOverflow);
            }
            long uniqueValues = actual.Count;
            long uncompressedSize = uniqueValues * 24;
            double compressionRatio = ((double)uncompressedSize - Instance.TotalBytes) / uncompressedSize * 100.0;
            double averageError = (double)totalError / uniqueValues;
            double averageErrorExOverflow = (double)totalErrorExOverflow / uniqueValues;

            Console.WriteLine($"  Memory used: {Instance.TotalBytes}");
            Console.WriteLine($"  Load factor: {(double)Instance.BytesUsed / Instance.TotalBytes * 100.0}%");
            Console.WriteLine($"  Unique values: {uniqueValues}");
            Console.WriteLine($"  Average error: {averageError} ({averageErrorExOverflow} excluding overflow)");
            Console.WriteLine($"  Max error: {maxError} ({maxErrorExOverflow} excluding overflow)");
            Console.WriteLine($"  Compression Ratio: {compressionRatio}%");
            Console.WriteLine();
        }

        public void Clear()
        {
            Instance.Clear();
            Successes = 0;
            Overflows = 0;
            NoCapacities = 0;
        }

        private readonly string Name;
        private readonly IFrequencyEstimator Instance;
        private long Successes;
        private long Overflows;
        private long NoCapacities;
    }
}
