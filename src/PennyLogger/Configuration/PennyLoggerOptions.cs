// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Configuration;
using PennyLogger.Internals.Dictionary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Service-level configuration options for PennyLogger
    /// </summary>
    public class PennyLoggerOptions : OptionsBase<PennyLoggerOptions>
    {
        /// <summary>
        /// Per-event configuration overrides. This value is a map of string Event IDs to
        /// <see cref="PennyEventOptions"/>. Options set here will override any configuration options set in the code
        /// via attributes or parameters.
        /// </summary>
        public IDictionary<string, PennyEventOptions> Events { get; set; }

        /// <summary>
        /// Per-sampler configuration overrides. This value is a map of string Event IDs to
        /// <see cref="PennySamplerOptions"/>. Options set here will override any configuration options set in the code
        /// via attributes or parameters.
        /// </summary>
        public IDictionary<string, PennySamplerOptions> Samplers { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Events?.ToValueType(), Samplers?.ToValueType());
    }
}
