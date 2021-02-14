// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// Extension methods to <see cref="IPennyLogger"/>
    /// </summary>
    public static class PennyLoggerExtensions
    {
        /// <summary>
        /// Logs a structured event
        /// </summary>
        /// <param name="logger">PennyLogger service instance</param>
        /// <param name="eventObject">Event object</param>
        /// <param name="options">Additional event options</param>
        /// <typeparam name="T">Type of <paramref name="eventObject"/></typeparam>
        public static void Event<T>(this IPennyLogger logger, T eventObject, PennyEventOptions options = null)
        {
            logger.Event(eventObject, typeof(T), options);
        }

        /// <summary>
        /// Executes a lambda function on a regular interval, logging the properties of the returned object
        /// </summary>
        /// <param name="logger">PennyLogger service instance</param>
        /// <param name="samplerLambda">
        /// Lambda function executed every interval. The return value is an object with properties to log.
        /// </param>
        /// <param name="options">Additional sampler options</param>
        /// <returns><see cref="IDisposable"/> instance. When disposed, the sampler stops.</returns>
        /// <typeparam name="T">Type of the object returned by <paramref name="samplerLambda"/></typeparam>
        public static IDisposable Sample<T>(this IPennyLogger logger, Func<T> samplerLambda,
            PennySamplerOptions options = null)
        {
            return logger.Sample(() => samplerLambda(), typeof(T), options);
        }

        /// <summary>
        /// Samples an object's properties on a regular interval, logging their values
        /// </summary>
        /// <param name="logger">PennyLogger service instance</param>
        /// <param name="samplerObject">Object to sample containing properties to log</param>
        /// <param name="options">Additional sampler options</param>
        /// <returns><see cref="IDisposable"/> instance. When disposed, the sampler stops.</returns>
        public static IDisposable Sample<T>(this IPennyLogger logger, T samplerObject,
            PennySamplerOptions options = null)
        {
            return logger.Sample(() => samplerObject, typeof(T), options);
        }
    }
}
