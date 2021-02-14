// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// The PennyLogger library aids in logging of events by distributing the first level of event filtering and
    /// aggregation to the edge, greatly reducing the amount of log data sent to log analysis engines such as Splunk or
    /// Sumo Logic, which generally charge by the gigabyte for log ingress.
    /// </summary>
    public interface IPennyLogger
    {
        /// <summary>
        /// Logs a structured event
        /// </summary>
        /// <param name="eventObject">Event object</param>
        /// <param name="eventType">Type of <paramref name="eventObject"/></param>
        /// <param name="options">Additional event options</param>
        public void Event(object eventObject, Type eventType, PennyEventOptions options = null);

        /// <summary>
        /// Executes a lambda function on a regular interval, logging the properties of the returned object
        /// </summary>
        /// <param name="samplerLambda">
        /// Lambda function executed every interval. The return value is an object with properties to log.
        /// </param>
        /// <param name="samplerType">Type of the object returned by <paramref name="samplerLambda"/></param>
        /// <param name="options">Additional sampler options</param>
        /// <returns><see cref="IDisposable"/> instance. When disposed, the sampler stops.</returns>
        public IDisposable Sample(Func<object> samplerLambda, Type samplerType, PennySamplerOptions options = null);
    }
}
