// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Dictionary;
using PennyLogger.Internals.Raw;
using PennyLogger.Internals.Reflection;
using System;

namespace PennyLogger.Internals
{
    /// <summary>
    /// Helper class to track the state and handle a single PennyLogger sampler
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IDisposable"/> and is returned by
    /// <see cref="IPennyLogger.Sample(Func{object}, Type, PennySamplerOptions)"/>. The caller may call
    /// <see cref="Dispose"/> to stop the sampler.
    /// </remarks>
    internal class SamplerState : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reflector">Reflector instance used to read the object returned by the sampler lambda</param>
        /// <param name="samplerLambda">Lambda function to execute to sample values to log</param>
        /// <param name="logger">Underyling logging provider where output messages are logged</param>
        /// <param name="timers">Factory used to create timers</param>
        /// <param name="paramOptions">
        /// Sampler configuration options provided via parameters to the
        /// <see cref="IPennyLogger.Event(object, Type, PennyEventOptions)"/> method
        /// </param>
        /// <param name="configOptions">
        /// Sampler configuration options provided via appconfig or other dynamic configuration
        /// </param>
        /// <param name="disposeLambda">Callback to notify the parent class when this instance is disposed</param>
        public SamplerState(SamplerReflector reflector, Func<object> samplerLambda, IPennyLoggerOutput logger,
            TimerManager timers, PennySamplerOptions paramOptions, PennySamplerOptions configOptions,
            Action disposeLambda)
        {
            Reflector = reflector;
            SamplerLambda = samplerLambda;
            Logger = logger;
            Timers = timers;
            DisposeLambda = disposeLambda;

            UpdateConfiguration(paramOptions, configOptions, true);
        }

        private readonly SamplerReflector Reflector;
        private readonly Func<object> SamplerLambda;
        private readonly IPennyLoggerOutput Logger;
        private readonly TimerManager Timers;
        private readonly Action DisposeLambda;

        /// <summary>
        /// Internal helper function to detect changes to <see cref="ParamOptions"/> and/or <see cref="ConfigOptions"/>,
        /// and if either changes, updates <see cref="Config"/> and flushes any child objects that hold configuration.
        /// </summary>
        /// <param name="paramOptions">
        /// Options from parameters to <see cref="IPennyLogger.Sample(Func{object}, Type, PennySamplerOptions)"/>
        /// </param>
        /// <param name="configOptions">Options from ASP.NET Core configuration</param>
        /// <param name="force">Forces configuration to be recalculated, even if options haven't changed</param>
        private void UpdateConfiguration(PennySamplerOptions paramOptions, PennySamplerOptions configOptions,
            bool force = false)
        {
            if (ParamOptions != paramOptions || ConfigOptions != configOptions || force)
            {
                // Stop any existing logging
                SampleTimer?.Dispose();
                SampleTimer = null;

                // RawEvent doesn't handle configuration changes. Let it get recreated on the next call to Flush().
                RawEvent = null;

                // Recompute the event configuration
                ParamOptions = paramOptions;
                ConfigOptions = configOptions;
                Config = PennySamplerConfig.Create(ConfigOptions, ParamOptions, Reflector.SamplerAttribute);

                // Start timers for the current logging settings
                if (Config.Enabled)
                {
                    SampleTimer = Timers.Start(Config.Interval, Flush);
                }
            }
        }

        private PennySamplerOptions ParamOptions;
        private PennySamplerOptions ConfigOptions;
        private PennySamplerConfig Config;

        private IDisposable SampleTimer;

        /// <summary>
        /// Handles dynamic changes to the application's configuration
        /// </summary>
        /// <param name="options">New options for this sampler</param>
        public void OnOptionsChange(PennySamplerOptions options) => UpdateConfiguration(ParamOptions, options);

        private RawEvent RawEvent;

        /// <summary>
        /// Invoked on the sampler's timer (or may be called directly by unit tests) to execute the sampler lambda and
        /// log its output.
        /// </summary>
        public void Flush()
        {
            if (Config != null && Config.Enabled)
            {
                // Lock to ensure we do not have concurrent calls to SamplerLambda, in case it is not thread-safe.
                lock (this)
                {
                    RawEvent ??= Reflector.CreateRawEvent(CreatePropertyConfig);

                    var sampleObject = SamplerLambda();

                    Logger.Log(Config.Level, writer => RawEvent.Serialize(writer, sampleObject));
                }
            }
        }

        private PennyPropertyConfig CreatePropertyConfig(PropertyReflector property)
        {
            var paramOptions = ParamOptions?.Properties?.GetValue(property.Name);
            var configOptions = ConfigOptions?.Properties?.GetValue(property.Name);
            return PennyPropertyConfig.Create(configOptions, paramOptions, property.PropertyAttribute,
                property.EnumerableAttribute, property.ReflectedPropertyType);
        }

        ~SamplerState()
        {
            SampleTimer?.Dispose();
            SampleTimer = null;
            DisposeLambda();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SampleTimer?.Dispose();
            SampleTimer = null;
            DisposeLambda();

            GC.SuppressFinalize(this);
        }
    }
}
