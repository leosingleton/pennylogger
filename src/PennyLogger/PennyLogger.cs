// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PennyLogger.Internals;
using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Reflection;
using PennyLogger.Output;
using System;
using System.Collections.Concurrent;

namespace PennyLogger
{
    /// <summary>
    /// Log analysis tools such as Splunk, Sumo Logic, or Logstash are critical for monitoring modern cloud
    /// applications, but the costs of processing millions and billions of log events can become prohibitively
    /// expensive. PennyLogger is a .NET Standard library that offloads the first level of event aggregation and
    /// filtering to the application itself, enabling events to be logged at a fraction of the usual costs.
    /// </summary>
    /// <remarks>
    /// For ASP.NET Core integration, see PennyLoggerAspNetCore in the PennyLogger.AspNetCore package instead.
    /// </remarks>
    public class PennyLogger : IPennyLogger
    {
        /// <summary>
        /// Constructor for use with any configuration and output log provider
        /// </summary>
        /// <param name="logger">Output log provider</param>
        /// <param name="options">
        /// Initial configuration options. May be changed at runtime using
        /// <see cref="UpdateOptions(PennyLoggerOptions)"/>.
        /// </param>
        public PennyLogger(IPennyLoggerOutput logger, PennyLoggerOptions options = null)
        {
            Logger = logger;
            Timers = new TimerManager();

            EventReflectors = new ConcurrentDictionary<Type, EventReflector>();
            EventStates = new ConcurrentDictionary<string, EventState>();
            SamplerStates = new ConcurrentDictionary<string, SamplerState>();

            Options = options;
        }

        /// <summary>
        /// Constructor for use with dependency injection, using <see cref="IOptionsMonitor{TOptions}"/> for
        /// configuration and <see cref="ILogger"/> for output
        /// </summary>
        /// <param name="logger">Output log provider</param>
        /// <param name="options">
        /// Initial configuration options. May be changed at runtime using
        /// <see cref="UpdateOptions(PennyLoggerOptions)"/>.
        /// </param>
        public PennyLogger(ILogger<PennyLogger> logger, IOptionsMonitor<PennyLoggerOptions> options) :
            this(new LoggerOutput(logger), options.CurrentValue)
        {
            options.OnChange(UpdateOptions);
        }

        /// <inheritdoc/>
        public void Event(object eventObject, Type eventType, PennyEventOptions options = null)
        {
            // Construct an EventReflector object to read the properties and attributes to determine the Event ID. Cache
            // the object for further use.
            var reflector = EventReflectors.GetOrAdd(eventType, t => new EventReflector(t, options));
            string eventId = reflector.GetEventId(eventObject);

            // The rest of the logic lives in the per-name EventState object
            var configOptions = Options?.Events?.GetValue(eventId);
            bool created = false;
            var state = EventStates.GetOrAdd(eventId, n =>
            {
                created = true;
                return new EventState(eventId, reflector, Logger, Timers, options, configOptions);
            });
            state.ProcessEvent(eventObject, reflector, options);

            // According to the documentation on ConcurrentDictionary.GetOrAdd(), the lambda to create a new object
            // executes outside of any locks. Therefore, there's a potential race condition where a change in
            // configuration options occurs between the time EventState is created and it gets added to the dictionary.
            // Mitigate this by re-checking the configuration for changes after insertion.
            if (created)
            {
                var configOptions2 = Options?.Events?.GetValue(eventId);
                if (configOptions != configOptions2)
                {
                    state.OnOptionsChange(configOptions2);
                }
            }
        }

        /// <inheritdoc/>
        public IDisposable Sample(Func<object> samplerLambda, Type samplerType, PennySamplerOptions options = null)
        {
            // Construct a SamplerReflector object to read the properties and attributes to determine the Event ID
            var reflector = new SamplerReflector(samplerLambda, samplerType, options);
            string samplerId = reflector.Id;

            // The rest of the logic lives in the per-sampler SamplerState object
            var configOptions = Options?.Samplers?.GetValue(samplerId);
            var state = new SamplerState(reflector, samplerLambda, Logger, Timers, options, configOptions,
                () => SamplerStates.TryRemove(samplerId, out var _));
            if (!SamplerStates.TryAdd(samplerId, state))
            {
                throw new InvalidOperationException($"Each sampler must have a unique ID. Duplicate ID: {samplerId}");
            }

            // There's a potential race condition where a change in configuration options occurs between the time
            // SamplerState is created and it gets added to the dictionary. Mitigate this by re-checking the
            // configuration for changes after insertion.
            var configOptions2 = Options?.Samplers?.GetValue(samplerId);
            if (configOptions != configOptions2)
            {
                state.OnOptionsChange(configOptions2);
            }

            return state;
        }

        /// <summary>
        /// Exposed for unit tests to simulate timer execution on all registered events. Writes any aggregated values to
        /// the output log and resets the counters used for rate-limiting raw events.
        /// </summary>
        internal void Flush()
        {
            foreach (var state in EventStates.Values)
            {
                state.Flush();
            }
        }

        /// <summary>
        /// Updates the configurations options at runtime, replacing the options provided in the constructor
        /// </summary>
        /// <param name="options">New PennyLogger options</param>
        public void UpdateOptions(PennyLoggerOptions options)
        {
            Options = options;

            // Update any existing Event ID state with the new options
            foreach (var kvp in EventStates)
            {
                var id = kvp.Key;
                var state = kvp.Value;
                var eventOptions = options?.Events?.GetValue(id);
                state.OnOptionsChange(eventOptions);
            }

            // Update any existing Sampler ID state with the new options
            foreach (var kvp in SamplerStates)
            {
                var id = kvp.Key;
                var state = kvp.Value;
                var eventOptions = options?.Samplers?.GetValue(id);
                state.OnOptionsChange(eventOptions);
            }
        }

        private readonly IPennyLoggerOutput Logger;
        private readonly TimerManager Timers;
        private readonly ConcurrentDictionary<Type, EventReflector> EventReflectors;
        private readonly ConcurrentDictionary<string, EventState> EventStates;
        private readonly ConcurrentDictionary<string, SamplerState> SamplerStates;
        private PennyLoggerOptions Options;
    }
}
