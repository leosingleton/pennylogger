// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace PennyLogger
{
    /// <summary>
    /// Extension methods to add the PennyLogger service to an ASP.NET Core application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the PennyLogger service to an ASP.NET Core application
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddPennyLogger(this IServiceCollection services)
        {
            services.AddSingleton<IPennyLogger, PennyLogger>();

            return services;
        }
    }
}
