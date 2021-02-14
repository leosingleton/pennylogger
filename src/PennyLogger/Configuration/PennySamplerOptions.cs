// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using PennyLogger.Configuration;
using PennyLogger.Internals.Dictionary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Configuration options for a PennyLogger sampler
    /// </summary>
    public class PennySamplerOptions : OptionsBase<PennySamplerOptions>
    {
        /// <inheritdoc cref="PennySamplerConfig.Enabled"/>
        public bool? Enabled { get; set; }

        /// <inheritdoc cref="PennySamplerConfig.Id"/>
        public string Id { get; set; }

        /// <inheritdoc cref="PennySamplerConfig.Level"/>
        public LogLevel? Level { get; set; }

        /// <inheritdoc cref="PennySamplerConfig.Interval"/>
        public int? Interval { get; set; }

        /// <summary>
        /// Configuration options for specific properties in the sampler. The dictionary's keys are the property names,
        /// as strings.
        /// </summary>
        public IDictionary<string, PennyPropertyOptions> Properties { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Enabled, Id, Level, Interval, Properties?.ToValueType());
    }
}
