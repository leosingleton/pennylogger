// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Estimator.CascadingCuckoo;
using PennyLogger.Internals.Estimator.CountMin;
using PennyLogger.Internals.Estimator.Cuckoo;

namespace PennyLogger.EstimatorTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var estimators = new SimEstimator[]
            {
                new SimEstimator("Zero", new ZeroEstimator()),
                new SimEstimator("CountMinSketch(0.001,0.001)", new CountMinSketch(0.001, 0.001)),
                new SimEstimator("CountMeanMinSketch(0.001,0.001)", new CountMeanMinSketch(0.001, 0.001)),
                new SimEstimator("CuckooFilter2Way<64>(1024)", new CuckooFilter2Way<CuckooBucket64Counting>(1024)),
                new SimEstimator("CascadingCuckooFilter2Way", new CascadingCuckooFilter2Way()),
                new SimEstimator("CascadingCuckooFilter4Way", new CascadingCuckooFilter4Way())
            };

            var sim = new Simulation(5, 1000, 0.25, 0.3, true, false, estimators);
            sim.Run();
        }
    }
}
