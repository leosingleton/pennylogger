// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// Identifies an object used as a PennyLogger event and specifies configuration. Public properties in this object
    /// will be serialized to JSON and logged.
    /// </summary>
    /// <remarks>
    /// In addition to this attribute, PennyLogger event objects can use the
    /// <see cref="PennyEventAggregateLoggingAttribute"/> and <see cref="PennyEventRawLoggingAttribute"/> attributes to
    /// specify logging configuration.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class PennyEventAttribute : Attribute
    {
        /// <inheritdoc cref="PennyEventConfig.Enabled"/>
        public bool Enabled { get; set; } = PennyEventConfig.DefaultEnabled;

        /// <inheritdoc cref="PennyEventConfig.Id"/>
        public string Id { get; set; } = PennyEventConfig.DefaultId;
    }
}
