// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Aggregate;
using PennyLogger.Internals.Raw;
using System;
using System.Linq;
using System.Reflection;

namespace PennyLogger.Internals.Reflection
{
    /// <summary>
    /// Common code for <see cref="EventReflector"/> and <see cref="SamplerReflector"/>
    /// </summary>
    internal abstract class ObjectReflector
    {
        /// <summary>
        /// Creates an instance of <see cref="PropertyReflector"/>
        /// </summary>
        /// <param name="property">Property of which to create a reflector instance</param>
        /// <returns>Property reflector instance</returns>
        protected PropertyReflector CreatePropertyReflector(MemberInfo property)
        {
            // Determine the member type
            Type t = property switch
            {
                PropertyInfo pi => pi.PropertyType,
                FieldInfo fi => fi.FieldType,
                _ => throw new InvalidOperationException("Unreachable code")
            };

            if (t.IsEnum)
            {
                return new PropertyReflectorEnum(property);
            }
            else
            {
                return t.FullName switch
                {
                    "System.String" => new PropertyReflectorString(property),
                    "System.Int32" => new PropertyReflectorInt32(property),
                    "System.Int64" => new PropertyReflectorInt64(property),
                    "System.UInt32" => new PropertyReflectorUInt32(property),
                    "System.UInt64" => new PropertyReflectorUInt64(property),
                    "System.Single" => new PropertyReflectorSingle(property),
                    "System.Double" => new PropertyReflectorDouble(property),
                    "System.Decimal" => new PropertyReflectorDecimal(property),
                    "System.Boolean" => new PropertyReflectorBool(property),
                    "System.Type" => new PropertyReflectorType(property),
                    "System.Guid" => new PropertyReflectorGuid(property),
                    _ => null
                };
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="AggregateEvent"/>
        /// </summary>
        /// <param name="id">
        /// Event ID for the aggregate event. Note that we generally use a suffix ($) to distinguish aggregate events
        /// from raw events in the output log.
        /// </param>
        /// <param name="createConfigLambda">
        /// Lambda function to create a <see cref="PennyPropertyConfig"/> from a <see cref="PropertyReflector"/>
        /// </param>
        /// <param name="ignoreProperties">Optional list of property names to ignore</param>
        /// <returns>New <see cref="AggregateEvent"/> instance</returns>
        public AggregateEvent CreateAggregateEvent(string id,
            Func<PropertyReflector, PennyPropertyConfig> createConfigLambda, params string[] ignoreProperties)
        {
            // Convert PropertyReflector[] => AggregateProperty[]
            var properties = Properties
                .Where(p => !ignoreProperties.Contains(p.Name))
                .Select(p => p.CreateAggregateProperty(createConfigLambda(p)))
                .OfType<AggregateProperty>()
                .Where(p => p.Config.Enabled)
                .ToArray();

            return new AggregateEvent(id, properties);
        }

        /// <summary>
        /// Creates an instance of <see cref="RawEvent"/>
        /// </summary>
        /// <param name="createConfigLambda">
        /// Lambda function to create a <see cref="PennyPropertyConfig"/> from a <see cref="PropertyReflector"/>
        /// </param>
        /// <returns>New <see cref="RawEvent"/> instance</returns>
        public RawEvent CreateRawEvent(Func<PropertyReflector, PennyPropertyConfig> createConfigLambda)
        {
            // Convert PropertyReflector[] => RawProperty[]
            var properties = Properties
                .Select(p => p.CreateRawProperty(createConfigLambda(p)))
                .OfType<RawProperty>()
                .Where(p => p.Config.Enabled)
                .ToArray();

            return new RawEvent(properties);
        }

        /// <summary>
        /// Array of reflector instances to read each of the object's properties
        /// </summary>
        protected abstract PropertyReflector[] Properties { get; }
    }
}
