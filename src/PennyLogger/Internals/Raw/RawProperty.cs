// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Reflection;
using System;
using System.Text.Json;

namespace PennyLogger.Internals.Raw
{
    /// <summary>
    /// Base class for helpers that serialize a single property to JSON as part of a raw log event
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    /// This non-generic base class contains the members common across all property types. Additional members are
    /// available on the more specific <see cref="RawProperty{T}"/> instead.
    /// </item>
    /// <item>
    /// Although PennyLogger supports dynamically changing configuration at runtime, instances of classes derived
    /// from <see cref="RawProperty"/> do not. The instance must be deleted and recreated on configuration change.
    /// </item>
    /// </list>
    /// </remarks>
    internal abstract class RawProperty
    {
        /// <summary>
        /// Property name written to the output log
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Property configuration
        /// </summary>
        /// <remarks>
        /// Although PennyLogger supports dynamically changing configuration at runtime, instances of classes derived
        /// from <see cref="RawProperty"/> do not. The instance must be deleted and recreated on configuration change,
        /// so it is safe to assume the configuration in this property never changes.
        /// </remarks>
        public abstract PennyPropertyConfig Config { get; }

        /// <summary>
        /// Reads an event object and serializes one property to a JSON writer
        /// </summary>
        /// <param name="writer">JSON writer where the output is written</param>
        /// <param name="eventObject">Event object to read</param>
        public abstract void Serialize(Utf8JsonWriter writer, object eventObject);
    }

    /// <inheritdoc/>
    internal abstract class RawProperty<T> : RawProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">Helper class to read this property's value</param>
        /// <param name="config">Property configuration</param>
        protected RawProperty(PropertyReflector<T> property, PennyPropertyConfig config)
        {
            Property = property;
            _Config = config;
        }

        /// <inheritdoc/>
        public override string Name => Property.Name;

        /// <inheritdoc/>
        public override PennyPropertyConfig Config => _Config;
        private readonly PennyPropertyConfig _Config;

        /// <summary>
        /// Writes a property value to JSON
        /// </summary>
        /// <param name="writer">Output JSON writer</param>
        /// <param name="value">Value to write</param>
        public abstract void WriteProperty(Utf8JsonWriter writer, T value);

        /// <inheritdoc/>
        public override void Serialize(Utf8JsonWriter writer, object eventObject)
        {
            T value = Property.GetValue(eventObject);
            WriteProperty(writer, value);
        }

        /// <summary>
        /// Helper class to read this property's value
        /// </summary>
        protected readonly PropertyReflector<T> Property;
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="string"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyString : RawProperty<string>
    {
        /// <inheritdoc/>
        public RawPropertyString(PropertyReflector<string> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, string value)
        {
            if (Config.IgnoreNull && value == null)
            {
                return;
            }
            if (Config.IgnoreEmpty && value == "")
            {
                return;
            }
            if (value.Length > Config.MaxLength)
            {
                value = value.Substring(0, Config.MaxLength) + "...";
            }

            writer.WriteString(Name, value);
        }
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="int"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyInt32 : RawProperty<int>
    {
        /// <inheritdoc/>
        public RawPropertyInt32(PropertyReflector<int> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, int value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="long"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyInt64 : RawProperty<long>
    {
        /// <inheritdoc/>
        public RawPropertyInt64(PropertyReflector<long> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, long value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="uint"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyUInt32 : RawProperty<uint>
    {
        /// <inheritdoc/>
        public RawPropertyUInt32(PropertyReflector<uint> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, uint value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="ulong"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyUInt64 : RawProperty<ulong>
    {
        /// <inheritdoc/>
        public RawPropertyUInt64(PropertyReflector<ulong> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, ulong value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="float"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertySingle : RawProperty<float>
    {
        /// <inheritdoc/>
        public RawPropertySingle(PropertyReflector<float> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, float value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="double"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyDouble : RawProperty<double>
    {
        /// <inheritdoc/>
        public RawPropertyDouble(PropertyReflector<double> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, double value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="decimal"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyDecimal : RawProperty<decimal>
    {
        /// <inheritdoc/>
        public RawPropertyDecimal(PropertyReflector<decimal> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, decimal value) => writer.WriteNumber(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="bool"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyBool : RawProperty<bool>
    {
        /// <inheritdoc/>
        public RawPropertyBool(PropertyReflector<bool> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, bool value) => writer.WriteBoolean(Name, value);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="Type"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyType : RawProperty<Type>
    {
        /// <inheritdoc/>
        public RawPropertyType(PropertyReflector<Type> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, Type value) => writer.WriteString(Name, value.Name);
    }

    /// <summary>
    /// Helper class for serializing a single <see cref="Guid"/> property to JSON as part of a raw log event
    /// </summary>
    internal class RawPropertyGuid : RawProperty<Guid>
    {
        /// <inheritdoc/>
        public RawPropertyGuid(PropertyReflector<Guid> property, PennyPropertyConfig config) : base(property, config)
        {
        }

        /// <inheritdoc/>
        public override void WriteProperty(Utf8JsonWriter writer, Guid value) =>
            writer.WriteString(Name, value.ToString());
    }
}
