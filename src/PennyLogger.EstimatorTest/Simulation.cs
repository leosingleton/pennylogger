// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator;
using System;
using System.Collections.Generic;

namespace PennyLogger.EstimatorTest
{
    internal class Simulation
    {
        public Simulation(int iterations, int initialValuesPerIteration, double probabilityNew, double probabilityFinal,
            bool enableOutput, bool useSeededRandom, params SimEstimator[] estimators)
        {
            Iterations = iterations;
            InitialValuesPerIteration = initialValuesPerIteration;
            ProbabilityNew = probabilityNew;
            ProbabilityFinal = probabilityFinal;
            EnableOutput = enableOutput;
            Estimators = estimators;
            Rand = useSeededRandom ? new Random(1) : new Random();
        }

        private readonly bool EnableOutput;
        private readonly SimEstimator[] Estimators;
        private readonly Random Rand;

        public int Iterations { get; private set; }
        public int InitialValuesPerIteration { get; private set; }
        public double ProbabilityNew { get; private set; }
        public double ProbabilityFinal { get; private set; }

        public void Run()
        {
            for (int n = 0; n < Iterations; n++)
            {
                if (EnableOutput)
                {
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine($"Starting Iteration {n}...");
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine();
                }

                RunIteration();
            }
        }

        private void RunIteration()
        {
            var active = new List<Guid>();
            var actual = new Dictionary<Guid, long>();

            for (int n = 0; n < InitialValuesPerIteration; n++)
            {
                active.Add(Guid.NewGuid());
            }

            while (active.Count > 0)
            {
                Guid value = (Rand.NextDouble() < ProbabilityNew) ? Guid.NewGuid() : active[Rand.Next(active.Count)];
                SimulateValue(value);

                if (actual.ContainsKey(value))
                {
                    actual[value]++;
                }
                else
                {
                    actual[value] = 1;
                }

                if (Rand.NextDouble() < ProbabilityFinal)
                {
                    active.Remove(value);
                }
            }

            foreach (var est in Estimators)
            {
                if (EnableOutput)
                {
                    est.WriteSummary(actual);
                }
                est.Clear();
            }
        }

        private void SimulateValue(Guid value)
        {
            var hash = Hash.Create(value);

            foreach (var est in Estimators)
            {
                est.SimulateValue(hash);
            }
        }
    }
}
