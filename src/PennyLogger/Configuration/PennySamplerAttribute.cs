// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;

namespace PennyLogger
{
    /// <summary>
    /// Identifies an object used as a PennyLogger sampler and specifies configuration. Public properties in this object
    /// will be sampled every <see cref="Interval"/>, serialized to JSON, and logged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class PennySamplerAttribute : Attribute
    {
        /// <inheritdoc cref="PennySamplerConfig.Enabled"/>
        public bool Enabled { get; set; } = PennySamplerConfig.DefaultEnabled;

        /// <inheritdoc cref="PennySamplerConfig.Id"/>
        public string Id { get; set; } = PennySamplerConfig.DefaultId;

        /// <inheritdoc cref="PennySamplerConfig.Level"/>
        public LogLevel Level { get; set; } = PennySamplerConfig.DefaultLevel;

        /// <inheritdoc cref="PennySamplerConfig.Interval"/>
        public int Interval { get; set; } = PennySamplerConfig.DefaultInterval;
    }
}
