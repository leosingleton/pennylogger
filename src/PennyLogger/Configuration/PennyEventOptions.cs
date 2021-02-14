// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Configuration;
using PennyLogger.Internals.Dictionary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Configuration options for a PennyLogger event
    /// </summary>
    public class PennyEventOptions : OptionsBase<PennyEventOptions>
    {
        /// <inheritdoc cref="PennyEventConfig.Enabled"/>
        public bool? Enabled { get; set; }

        /// <inheritdoc cref="PennyEventConfig.Id"/>
        public string Id { get; set; }

        /// <inheritdoc cref="PennyEventConfig.AggregateLogging"/>
        public PennyEventAggregateLoggingOptions AggregateLogging { get; set; }

        /// <inheritdoc cref="PennyEventConfig.RawLogging"/>
        public PennyEventRawLoggingOptions RawLogging { get; set; }

        /// <summary>
        /// Configuration options for specific properties in the event. The dictionary's keys are the property names, as
        /// strings.
        /// </summary>
        public IDictionary<string, PennyPropertyOptions> Properties { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Enabled, Id, AggregateLogging, RawLogging, Properties?.ToValueType());
    }
}
