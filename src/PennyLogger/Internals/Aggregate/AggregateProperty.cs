// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Internals.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Base class for objects that track the aggregate values of a single property
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    /// This non-generic base class contains the members common across all property types. Additional members are
    /// available on the more specific <see cref="AggregateProperty{T}"/> instead.
    /// </item>
    /// <item>
    /// Although PennyLogger supports dynamically changing configuration at runtime, instances of classes derived
    /// from <see cref="AggregateProperty"/> do not. The instance must be deleted and recreated on configuration
    /// change.
    /// </item>
    /// </list>
    /// </remarks>
    internal abstract class AggregateProperty
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
        /// from <see cref="AggregateProperty"/> do not. The instance must be deleted and recreated on configuration
        /// change, so it is safe to assume the configuration in this property never changes.
        /// </remarks>
        public abstract PennyPropertyConfig Config { get; }

        /// <summary>
        /// Reads the property from an event object and updates the aggregated values to include the newly-read value
        /// </summary>
        /// <param name="eventObject">Event object to read</param>
        /// <param name="first">
        /// True on the first time an event is added to this object since its creation or the last call to
        /// <see cref="Clear"/>. Used by some aggregate properties that use value types where fields are initialized to
        /// default.
        /// </param>
        public abstract void Add(object eventObject, bool first);

        /// <summary>
        /// Clears the aggregated values. For performance, it is preferrable to clear and reuse the same
        /// <see cref="AggregateProperty"/> objects every interval to avoid creating and garbage collecting objects.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Writes the currrent aggregated values to JSON
        /// </summary>
        /// <param name="writer">JSON writer where the output is written</param>
        public abstract void Serialize(Utf8JsonWriter writer);
    }

    /// <inheritdoc/>
    internal abstract class AggregateProperty<T> : AggregateProperty
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">Helper class to read this property's value</param>
        /// <param name="config">Property configuration</param>
        protected AggregateProperty(PropertyReflector<T> property, PennyPropertyConfig config)
        {
            Property = property;
            _Config = config;
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public override string Name => Property.Name;

        /// <inheritdoc/>
        [JsonIgnore]
        public override PennyPropertyConfig Config => _Config;
        private readonly PennyPropertyConfig _Config;

        /// <inheritdoc/>
        public override void Add(object eventObject, bool first)
        {
            T value = Property.GetValue(eventObject);
            if (value != null || !Config.IgnoreNull)
            {
                AddValue(value, first);
            }
        }

        /// <summary>
        /// Derived classes must implement this method to add a new value to the set of aggregated values
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <param name="first">
        /// True on the first time an event is added to this object since its creation or the last call to
        /// <see cref="AggregateProperty.Clear"/>. Used by some aggregate properties that use value types where fields
        /// are initialized to default.
        /// </param>
        protected abstract void AddValue(T value, bool first);

        /// <summary>
        /// Helper class to read this property's value
        /// </summary>
        protected readonly PropertyReflector<T> Property;
    }
}
