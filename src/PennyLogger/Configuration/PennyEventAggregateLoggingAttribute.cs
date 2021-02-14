// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;

namespace PennyLogger
{
    /// <summary>
    /// Custom attribute applied to a PennyLogger event to configure the aggregate logging of events
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class PennyEventAggregateLoggingAttribute : Attribute
    {
        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.Level"/>
        public LogLevel Level { get; set; } = PennyEventAggregateLoggingConfig.DefaultLevel;

        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.Interval"/>
        public int Interval { get; set; } = PennyEventAggregateLoggingConfig.DefaultInterval;

        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.LogIfZero"/>
        public bool LogIfZero { get; set; } = PennyEventAggregateLoggingConfig.DefaultLogIfZero;
    }
}
