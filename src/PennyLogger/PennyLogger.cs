// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PennyLogger.Internals;
using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Reflection;
using System;
using System.Collections.Concurrent;

namespace PennyLogger
{
    /// <summary>
    /// PennyLogger implementation. Use the
    /// <see cref="ServiceCollectionExtensions.AddPennyLogger(IServiceCollection)"/> extension method to load the
    /// PennyLogger into an ASP.NET Core project.
    /// </summary>
    public class PennyLogger : IPennyLogger
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logging provider</param>
        /// <param name="options">Configuration options</param>
        public PennyLogger(ILogger<PennyLogger> logger, IOptionsMonitor<PennyLoggerOptions> options)
        {
            Logger = logger;
            Timers = new TimerManager();

            EventReflectors = new ConcurrentDictionary<Type, EventReflector>();
            EventStates = new ConcurrentDictionary<string, EventState>();
            SamplerStates = new ConcurrentDictionary<string, SamplerState>();

            Options = options.CurrentValue;
            options.OnChange(OnOptionsChange);
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

        private void OnOptionsChange(PennyLoggerOptions options)
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

        private readonly ILogger Logger;
        private readonly TimerManager Timers;
        private readonly ConcurrentDictionary<Type, EventReflector> EventReflectors;
        private readonly ConcurrentDictionary<string, EventState> EventStates;
        private readonly ConcurrentDictionary<string, SamplerState> SamplerStates;
        private PennyLoggerOptions Options;
    }
}
