// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;

namespace PennyLogger
{
    /// <summary>
    /// Custom attribute applied to a PennyLogger event to configure the raw logging of events
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class PennyEventRawLoggingAttribute : Attribute
    {
        /// <inheritdoc cref="PennyEventRawLoggingConfig.Level"/>
        public LogLevel Level { get; set; } = PennyEventRawLoggingConfig.DefaultLevel;

        /// <inheritdoc cref="PennyEventRawLoggingConfig.Max"/>
        public int Max { get; set; } = PennyEventRawLoggingConfig.DefaultMax;

        /// <inheritdoc cref="PennyEventRawLoggingConfig.Interval"/>
        public int Interval { get; set; } = PennyEventRawLoggingConfig.DefaultInterval;
    }
}
