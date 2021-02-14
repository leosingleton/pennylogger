// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Reflection;
using System;
using System.Text.Json;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Base class for objects that track the aggregate values of a single property
    /// </summary>
    /// <typeparam name="T">Numeric type</typeparam>
    internal abstract class AggregateNumericProperty<T> : AggregateProperty<T>
        where T : struct
    {
        /// <inheritdoc/>
        protected AggregateNumericProperty(PropertyReflector<T> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <summary>
        /// Minimum value, or default(T) if zero values
        /// </summary>
        public T Min { get; protected set; }

        /// <summary>
        /// Maximum value, or default(T) if zero values
        /// </summary>
        public T Max { get; protected set; }

        /// <summary>
        /// Sum of all values, or default(T) if zero values
        /// </summary>
        public T Sum { get; protected set; }

        /// <inheritdoc/>
        public override void Clear()
        {
            Min = default;
            Max = default;
            Sum = default;
        }

        /// <inheritdoc/>
        public override void Serialize(Utf8JsonWriter writer)
        {
            JsonSerializer.Serialize(writer, this);
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="int"/>
    /// </summary>
    internal class AggregateNumericPropertyInt32 : AggregateNumericProperty<int>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyInt32(PropertyReflector<int> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(int value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="long"/>
    /// </summary>
    internal class AggregateNumericPropertyInt64 : AggregateNumericProperty<long>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyInt64(PropertyReflector<long> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(long value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="uint"/>
    /// </summary>
    internal class AggregateNumericPropertyUInt32 : AggregateNumericProperty<uint>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyUInt32(PropertyReflector<uint> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(uint value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="ulong"/>
    /// </summary>
    internal class AggregateNumericPropertyUInt64 : AggregateNumericProperty<ulong>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyUInt64(PropertyReflector<ulong> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(ulong value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="float"/>
    /// </summary>
    internal class AggregateNumericPropertySingle : AggregateNumericProperty<float>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertySingle(PropertyReflector<float> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(float value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="double"/>
    /// </summary>
    internal class AggregateNumericPropertyDouble : AggregateNumericProperty<double>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyDouble(PropertyReflector<double> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(double value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }

    /// <summary>
    /// Objects that tracks the aggregate values of a single <see cref="decimal"/>
    /// </summary>
    internal class AggregateNumericPropertyDecimal : AggregateNumericProperty<decimal>
    {
        /// <inheritdoc/>
        public AggregateNumericPropertyDecimal(PropertyReflector<decimal> property, PennyPropertyConfig config) :
            base(property, config)
        {
        }

        /// <inheritdoc/>
        protected override void AddValue(decimal value, bool first)
        {
            Min = first ? value : Math.Min(Min, value);
            Max = first ? value : Math.Max(Max, value);
            Sum += value;
        }
    }
}
