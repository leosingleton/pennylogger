// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Aggregate;
using PennyLogger.Internals.Estimator;
using PennyLogger.Internals.Raw;
using System;
using System.Reflection;

namespace PennyLogger.Internals.Reflection
{
    /// <summary>
    /// Base class for helpers that read a single property via reflection
    /// </summary>
    /// <remarks>
    /// Note that despite the name, derived classes support both properties and fields via reflection. Also, this
    /// non-generic base class contains the members common across all property types. Additional members are available
    /// on the more specific <see cref="PropertyReflector{T}"/> instead.
    /// </remarks>
    internal abstract class PropertyReflector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">Property to read</param>
        /// <param name="name">
        /// Optional string to override the name of a property in output. If null, the name of
        /// <paramref name="property"/> is used in output.
        /// </param>
        protected PropertyReflector(MemberInfo property, string name = null)
        {
            Name = name ?? property.Name;
            ReflectedProperty = property;
            PropertyAttribute = property?.GetCustomAttribute<PennyPropertyAttribute>();
            EnumerableAttribute = property?.GetCustomAttribute<PennyPropertyEnumerableAttribute>();
        }

        /// <summary>
        /// Name of the property used in output log messages
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// <see cref="MemberInfo"/> used to read the property via reflection
        /// </summary>
        public MemberInfo ReflectedProperty { get; private set; }

        /// <summary>
        /// [PennyProperty] attribute. Null if the property definition does not have this attribute.
        /// </summary>
        public PennyPropertyAttribute PropertyAttribute { get; private set; }

        /// <summary>
        /// [PennyPropertyEnumerable] attribute. Null if the property definition does not have this attribute.
        /// </summary>
        public PennyPropertyEnumerableAttribute EnumerableAttribute { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="AggregateProperty"/>
        /// </summary>
        /// <param name="config">Property configuration</param>
        /// <returns>New <see cref="AggregateProperty"/> instance</returns>
        public virtual AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            return null;
        }

        /// <summary>
        /// Reads this property's value from an event object and adds its value to an <see cref="AggregateProperty"/>
        /// instance
        /// </summary>
        /// <param name="agg"><see cref="AggregateProperty"/> instance</param>
        /// <param name="eventObject">Event object to read</param>
        /// <param name="first">
        /// True on the first time an event is added to an <see cref="AggregateProperty"/> since its creation or the
        /// last call to <see cref="AggregateProperty.Clear"/>. Used by some aggregate properties that use value types
        /// where fields are initialized to default.
        /// </param>
        public abstract void AddToAggregateProperty(AggregateProperty agg, object eventObject, bool first);

        /// <summary>
        /// Creates an instance of <see cref="RawProperty"/>
        /// </summary>
        /// <param name="config">Property configuration</param>
        /// <returns>New <see cref="RawProperty"/> instance</returns>
        public abstract RawProperty CreateRawProperty(PennyPropertyConfig config);

        /// <summary>
        /// Property type
        /// </summary>
        public Type ReflectedPropertyType => ReflectedProperty switch
        {
            PropertyInfo pi => pi.PropertyType,
            FieldInfo fi => fi.FieldType,
            _ => null
        };

        /// <summary>
        /// Reads this property's value from an object instance via reflection, and returns it as an untyped value
        /// </summary>
        /// <param name="eventObject">Event object to read</param>
        /// <returns>Property value</returns>
        protected object GetUntypedValue(object eventObject) => ReflectedProperty switch
        {
            PropertyInfo pi => pi.GetValue(eventObject),
            FieldInfo fi => fi.GetValue(eventObject),
            _ => throw new InvalidOperationException("Unreachable code")
        };
    }

    /// <inheritdoc/>
    internal abstract class PropertyReflector<T> : PropertyReflector
    {
        /// <inheritdoc/>
        protected PropertyReflector(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <summary>
        /// Reads this property's value from an object instance via reflection, and returns it as a typed value
        /// </summary>
        /// <param name="eventObject">Event object to read</param>
        /// <returns>Property value</returns>
        public abstract T GetValue(object eventObject);

        /// <summary>
        /// Calculates a 128-bit hash from a value of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="value">Value to hash</param>
        /// <returns>128-bit hash</returns>
        public abstract Hash GetHashValue(T value);

        /// <inheritdoc/>
        public override void AddToAggregateProperty(AggregateProperty agg, object eventObject, bool first)
        {
            var aggTyped = (AggregateProperty<T>)agg;
            aggTyped.Add(GetValue(eventObject), first);
        }
    }

    /// <summary>
    /// Dummy implementation of <see cref="PropertyReflector{T}"/> that acts as a placeholder for a property with a
    /// hardcoded string value
    /// </summary>
    /// <remarks>
    /// This class is used to add an &quot;Event&quot; property to objects that do not have one. Despite the name, it
    /// doesn't use reflection, and the methods simply return the hardcoded values provided in the constructor.
    /// </remarks>
    internal class PropertyReflectorConstantString : PropertyReflector<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        public PropertyReflectorConstantString(string name, string value) : base(null, name)
        {
            Value = value;
        }

        private readonly string Value;

        /// <inheritdoc/>
        public override string GetValue(object eventObject) => Value;

        /// <inheritdoc/>
        public override Hash GetHashValue(string value) =>
            throw new InvalidOperationException("GetHashValue not supported");

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyString(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="string"/> property via reflection
    /// </summary>
    internal class PropertyReflectorString : PropertyReflector<string>
    {
        public PropertyReflectorString(MemberInfo property, string name = null) : base(property, name)
        {
        }

        public override string GetValue(object eventObject) => GetUntypedValue(eventObject) as string;

        /// <inheritdoc/>
        public override Hash GetHashValue(string value) => Hash.Create(value);

        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<string>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<string>(this, config);
                }
            }
            else
            {
                // By default, strings are ignored for aggregation
                return null;
            }
        }

        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyString(this, config);
    }

    /// <summary>
    /// Helper for reading an <see cref="Enum"/> property via reflection
    /// </summary>
    internal class PropertyReflectorEnum : PropertyReflector<string>
    {
        public PropertyReflectorEnum(MemberInfo property, string name = null) : base(property, name)
        {
        }

        public override string GetValue(object eventObject) => GetUntypedValue(eventObject).ToString();

        /// <inheritdoc/>
        public override Hash GetHashValue(string value) => Hash.Create(value);

        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable.Approximate)
            {
                return new AggregateEnumerablePropertyLossy<string>(this, config);
            }
            else
            {
                return new AggregateEnumerablePropertyLossless<string>(this, config);
            }
        }

        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyString(this, config);
    }

    /// <summary>
    /// Helper for reading an <see cref="int"/> property via reflection
    /// </summary>
    internal class PropertyReflectorInt32 : PropertyReflector<int>
    {
        /// <inheritdoc/>
        public PropertyReflectorInt32(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override int GetValue(object eventObject) => (int)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(int value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<int>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<int>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyInt32(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyInt32(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="long"/> property via reflection
    /// </summary>
    internal class PropertyReflectorInt64 : PropertyReflector<long>
    {
        /// <inheritdoc/>
        public PropertyReflectorInt64(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override long GetValue(object eventObject) => (long)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(long value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<long>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<long>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyInt64(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyInt64(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="uint"/> property via reflection
    /// </summary>
    internal class PropertyReflectorUInt32 : PropertyReflector<uint>
    {
        /// <inheritdoc/>
        public PropertyReflectorUInt32(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override uint GetValue(object eventObject) => (uint)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(uint value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<uint>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<uint>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyUInt32(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyUInt32(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="ulong"/> property via reflection
    /// </summary>
    internal class PropertyReflectorUInt64 : PropertyReflector<ulong>
    {
        /// <inheritdoc/>
        public PropertyReflectorUInt64(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override ulong GetValue(object eventObject) => (ulong)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(ulong value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<ulong>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<ulong>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyUInt64(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyUInt64(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="float"/> property via reflection
    /// </summary>
    internal class PropertyReflectorSingle : PropertyReflector<float>
    {
        /// <inheritdoc/>
        public PropertyReflectorSingle(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override float GetValue(object eventObject) => (float)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(float value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<float>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<float>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertySingle(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertySingle(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="double"/> property via reflection
    /// </summary>
    internal class PropertyReflectorDouble : PropertyReflector<double>
    {
        /// <inheritdoc/>
        public PropertyReflectorDouble(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override double GetValue(object eventObject) => (double)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(double value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<double>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<double>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyDouble(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyDouble(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="decimal"/> property via reflection
    /// </summary>
    internal class PropertyReflectorDecimal : PropertyReflector<decimal>
    {
        /// <inheritdoc/>
        public PropertyReflectorDecimal(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override decimal GetValue(object eventObject) => (decimal)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(decimal value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable != null)
            {
                if (config.Enumerable.Approximate)
                {
                    return new AggregateEnumerablePropertyLossy<decimal>(this, config);
                }
                else
                {
                    return new AggregateEnumerablePropertyLossless<decimal>(this, config);
                }
            }
            else
            {
                return new AggregateNumericPropertyDecimal(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) =>
            new RawPropertyDecimal(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="bool"/> property via reflection
    /// </summary>
    internal class PropertyReflectorBool : PropertyReflector<bool>
    {
        /// <inheritdoc/>
        public PropertyReflectorBool(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override bool GetValue(object eventObject) => (bool)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(bool value) =>
            throw new InvalidOperationException("GetHashValue not supported");

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            return new AggregateEnumerablePropertyLossless<bool>(this, config);
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) => new RawPropertyBool(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="Type"/> property via reflection
    /// </summary>
    internal class PropertyReflectorType : PropertyReflector<Type>
    {
        /// <inheritdoc/>
        public PropertyReflectorType(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override Type GetValue(object eventObject) => (Type)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(Type value) => Hash.Create(value.FullName);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable.Approximate)
            {
                return new AggregateEnumerablePropertyLossy<Type>(this, config);
            }
            else
            {
                return new AggregateEnumerablePropertyLossless<Type>(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) => new RawPropertyType(this, config);
    }

    /// <summary>
    /// Helper for reading a <see cref="Guid"/> property via reflection
    /// </summary>
    internal class PropertyReflectorGuid : PropertyReflector<Guid>
    {
        /// <inheritdoc/>
        public PropertyReflectorGuid(MemberInfo property, string name = null) : base(property, name)
        {
        }

        /// <inheritdoc/>
        public override Guid GetValue(object eventObject) => (Guid)GetUntypedValue(eventObject);

        /// <inheritdoc/>
        public override Hash GetHashValue(Guid value) => Hash.Create(value);

        /// <inheritdoc/>
        public override AggregateProperty CreateAggregateProperty(PennyPropertyConfig config)
        {
            if (config.Enumerable.Approximate)
            {
                return new AggregateEnumerablePropertyLossy<Guid>(this, config);
            }
            else
            {
                return new AggregateEnumerablePropertyLossless<Guid>(this, config);
            }
        }

        /// <inheritdoc/>
        public override RawProperty CreateRawProperty(PennyPropertyConfig config) => new RawPropertyGuid(this, config);
    }
}
