// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using PennyLogger.Internals.Aggregate;
using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Raw;
using PennyLogger.Internals.Reflection;
using System;

namespace PennyLogger.Internals
{
    /// <summary>
    /// Helper class to track the state and handle a single PennyLogger event
    /// </summary>
    internal class EventState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventId">Unique Event ID</param>
        /// <param name="reflector">Reflector instance used to read the event object</param>
        /// <param name="logger">Underyling logging provider where output messages are logged</param>
        /// <param name="timers">Factory used to create timers</param>
        /// <param name="paramOptions">
        /// Event configuration options provided via parameters to the
        /// <see cref="IPennyLogger.Event(object, Type, PennyEventOptions)"/> method
        /// </param>
        /// <param name="configOptions">
        /// Event configuration options provided via appconfig or other dynamic configuration
        /// </param>
        public EventState(string eventId, EventReflector reflector, ILogger logger, TimerManager timers,
            PennyEventOptions paramOptions, PennyEventOptions configOptions)
        {
            EventId = eventId;
            Reflector = reflector;
            Logger = logger;
            Timers = timers;

            UpdateConfiguration(paramOptions, configOptions, true);
        }

        private readonly string EventId;
        private readonly EventReflector Reflector;
        private readonly ILogger Logger;
        private readonly TimerManager Timers;

        /// <summary>
        /// Internal helper function to detect changes to <see cref="ParamOptions"/> and/or <see cref="ConfigOptions"/>,
        /// and if either changes, updates <see cref="Config"/> and flushes any child objects that hold configuration.
        /// </summary>
        /// <param name="paramOptions">
        /// Options from parameters to <see cref="IPennyLogger.Event(object, Type, PennyEventOptions)"/>
        /// </param>
        /// <param name="configOptions">Options from ASP.NET Core configuration</param>
        /// <param name="force">Forces configuration to be recalculated, even if options haven't changed</param>
        private void UpdateConfiguration(PennyEventOptions paramOptions, PennyEventOptions configOptions,
            bool force = false)
        {
            if (ParamOptions != paramOptions || ConfigOptions != configOptions || force)
            {
                // Stop any existing logging and flush pending events
                AggregateLoggingTimer?.Dispose();
                AggregateLoggingTimer = null;

                RawLoggingTimer?.Dispose();
                RawLoggingTimer = null;

                FlushAggregateTimer();
                AggregateEvent = null;

                FlushRawTimer();
                RawEvent = null;

                // Recompute the event configuration
                ParamOptions = paramOptions;
                ConfigOptions = configOptions;
                Config = PennyEventConfig.Create(ConfigOptions, ParamOptions, Reflector.EventAttribute,
                    Reflector.AggregateLoggingAttribute, Reflector.RawLoggingAttribute);

                // Start timers for the current logging settings
                if (Config.AggregateLogging != null)
                {
                    AggregateLoggingTimer = Timers.Start(Config.AggregateLogging.Interval, OnAggregateTimer);
                }
                if (Config.RawLogging != null)
                {
                    RawLoggingTimer = Timers.Start(Config.RawLogging.Interval, OnRawTimer);
                }
            }
        }

        private PennyEventOptions ParamOptions;
        private PennyEventOptions ConfigOptions;
        private PennyEventConfig Config;

        private IDisposable AggregateLoggingTimer;
        private IDisposable RawLoggingTimer;

        /// <summary>
        /// Processes a new event from <see cref="IPennyLogger.Event(object, Type, PennyEventOptions)"/>
        /// </summary>
        /// <param name="eventObject">Event to process</param>
        /// <param name="reflector">
        /// Reflector instance used to read the Event ID. <see cref="EventState"/> already contains a reference to this
        /// reflector instance, however we pass it explicitly to catch a potentially hard-to-debug issue where two
        /// reflector instances output the same Event ID, and try to map to a single <see cref="EventState"/> instance.
        /// </param>
        /// <param name="paramOptions">Event options provided as a parameter. May be null.</param>
        public void ProcessEvent(object eventObject, EventReflector reflector, PennyEventOptions paramOptions)
        {
            lock (this)
            {
                if (Reflector != reflector)
                {
                    throw new ArgumentException(
                        $"Event name {EventId} is not unique and in use by different object types",
                        nameof(eventObject));
                }

                UpdateConfiguration(paramOptions, ConfigOptions);

                if (!Config.Enabled)
                {
                    return; // Ignore disabled events
                }

                if (Config.AggregateLogging != null)
                {
                    ProcessEventAggregate(eventObject);
                }

                if (Config.RawLogging != null)
                {
                    ProcessEventRaw(eventObject);
                }
            }
        }

        /// <summary>
        /// Exposed for unit tests to simulate timer execution. Writes any aggregated values to the output log and
        /// resets the counters used for rate-limiting raw events.
        /// </summary>
        public void Flush()
        {
            lock (this)
            {
                FlushAggregateTimer();
                FlushRawTimer();
            }
        }

        /// <summary>
        /// Handles dynamic changes to the application's configuration
        /// </summary>
        /// <param name="options">New options for this event</param>
        public void OnOptionsChange(PennyEventOptions options) => UpdateConfiguration(ParamOptions, options);

        private void ProcessEventAggregate(object eventObject)
        {
            // On the first event after the timer cleared out the data, create a new aggregate event object
            AggregateEvent ??= Reflector.CreateAggregateEvent($"{EventId}$", CreatePropertyConfig, "Event");
            AggregateEvent.Add(eventObject);
        }

        private AggregateEvent AggregateEvent;

        private void OnAggregateTimer()
        {
            lock (this)
            {
                FlushAggregateTimer();
            }
        }

        private void FlushAggregateTimer()
        {
            if (AggregateEvent != null && (Config.AggregateLogging.LogIfZero || AggregateEvent.Count > 0))
            {
                var message = Utf8JsonSerializer.Write(writer => AggregateEvent.Serialize(writer));
                Logger.Log(Config.AggregateLogging.Level, message);
                AggregateEvent.Clear();
            }
        }

        private void ProcessEventRaw(object eventObject)
        {
            RawEvent ??= Reflector.CreateRawEvent(CreatePropertyConfig);

            if (++RawEventCount <= Config.RawLogging.Max)
            {
                var message = Utf8JsonSerializer.Write(writer => RawEvent.Serialize(writer, eventObject));
                Logger.Log(Config.RawLogging.Level, message);
            }
        }

        private RawEvent RawEvent;

        private void OnRawTimer()
        {
            lock (this)
            {
                FlushRawTimer();
            }
        }

        private void FlushRawTimer()
        {
            RawEventCount = 0;
        }

        /// <summary>
        /// Counter used for rate-limiting raw events
        /// </summary>
        private long RawEventCount = 0;

        /// <summary>
        /// Gets the merged configuration of a property. Combines attributes along with options specified via parameters
        /// and application configutation.
        /// </summary>
        /// <param name="property">Property of which to get the configuration</param>
        /// <returns>Property configuration</returns>
        private PennyPropertyConfig CreatePropertyConfig(PropertyReflector property)
        {
            var paramOptions = ParamOptions?.Properties?.GetValue(property.Name);
            var configOptions = ConfigOptions?.Properties?.GetValue(property.Name);
            return PennyPropertyConfig.Create(configOptions, paramOptions, property.PropertyAttribute,
                property.EnumerableAttribute, property.ReflectedPropertyType);
        }
    }
}
