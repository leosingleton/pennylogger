// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// Custom attribute applied to a property or field in a PennyLogger event to treat it as an enumerable value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PennyPropertyEnumerableAttribute : Attribute
    {
        /// <inheritdoc cref="PennyPropertyEnumerableConfig.Top"/>
        public int Top { get; set; } = PennyPropertyEnumerableConfig.DefaultTop;

        /// <inheritdoc cref="PennyPropertyEnumerableConfig.Approximate"/>
        public bool Approximate { get; set; } = PennyPropertyEnumerableConfig.DefaultApproximate;

        /// <inheritdoc cref="PennyPropertyEnumerableConfig.MaxMemory"/>
        public int MaxMemory { get; set; } = PennyPropertyEnumerableConfig.DefaultMaxMemory;
    }
}
