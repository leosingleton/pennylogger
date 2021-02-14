// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Configuration;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Per-property PennyLogger enumerable configuration
    /// </summary>
    public class PennyPropertyEnumerableOptions : OptionsBase<PennyPropertyEnumerableOptions>
    {
        /// <inheritdoc cref="PennyPropertyEnumerableConfig.Top"/>
        public int? Top { get; set; }

        /// <inheritdoc cref="PennyPropertyEnumerableConfig.Approximate"/>
        public bool? Approximate { get; set; }

        /// <inheritdoc cref="PennyPropertyEnumerableConfig.MaxMemory"/>
        public int? MaxMemory { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Top, Approximate, MaxMemory);
    }
}
