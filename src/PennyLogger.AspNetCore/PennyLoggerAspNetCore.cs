// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PennyLogger
{
    /// <summary>
    /// PennyLogger implementation for ASP.NET Core. Use the
    /// <see cref="ServiceCollectionExtensions.AddPennyLogger(IServiceCollection)"/> extension method to load the
    /// PennyLogger into an ASP.NET Core project.
    /// </summary>
    public class PennyLoggerAspNetCore : PennyLogger
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logging provider</param>
        /// <param name="options">Configuration options</param>
        public PennyLoggerAspNetCore(ILogger<PennyLoggerAspNetCore> logger,
            IOptionsMonitor<PennyLoggerOptions> options) :
            base(new MicrosoftLoggingExtensionsOutput(logger), options.CurrentValue)
        {
            options.OnChange(UpdateOptions);
        }
    }
}
