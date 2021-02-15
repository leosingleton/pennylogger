// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PennyLogger
{
    public class PennyLoggerAspNetCore : PennyLogger
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logging provider</param>
        /// <param name="options">Configuration options</param>
        public PennyLoggerAspNetCore(ILogger<PennyLogger> logger, IOptionsMonitor<PennyLoggerOptions> options) :
            base (logger, options)
        {
        }
    }
}
