// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Text.Json;

namespace PennyLogger.Internals.Aggregate
{
    /// <summary>
    /// Object that tracks the aggregate values of an event object
    /// </summary>
    /// <remarks>
    /// Although PennyLogger supports dynamically changing configuration at runtime, instances of
    /// <see cref="AggregateEvent"/> and its child <see cref="AggregateProperty"/> instances do not. The instance must
    /// be deleted and recreated on configuration change.
    /// </remarks>
    internal class AggregateEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="properties">Object properties</param>
        public AggregateEvent(string id, AggregateProperty[] properties)
        {
            Id = id;
            Properties = properties;
        }

        /// <summary>
        /// Reads the properties from an event object and updates their aggregated values to include the newly-read
        /// object
        /// </summary>
        /// <param name="eventObject">Event object to read</param>
        public void Add(object eventObject)
        {
            bool first = (_Count == 0);
            _Count++;

            foreach (var property in Properties)
            {
                property.Add(eventObject, first);
            }
        }

        /// <summary>
        /// Clears the aggregated values. For performance, it is preferrable to clear and reuse the same
        /// <see cref="AggregateEvent"/> objects every interval to avoid creating and garbage collecting objects.
        /// </summary>
        public void Clear()
        {
            _Count = 0;

            foreach (var property in Properties)
            {
                property.Clear();
            }
        }

        /// <summary>
        /// Writes the currrent aggregated values to JSON
        /// </summary>
        /// <param name="writer">JSON writer where the output is written</param>
        public void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString("Event", Id);
            writer.WriteNumber("Count", _Count);

            foreach (var property in Properties)
            {
                writer.WritePropertyName(property.Name);
                property.Serialize(writer);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Number of event objects that have been aggregated
        /// </summary>
        /// <remarks>
        /// For space-efficiency, this count is kept once on the <see cref="AggregateEvent"/> instance instead of on
        /// each individual <see cref="AggregateProperty"/>.
        /// </remarks>
        public long Count => _Count;

        private readonly string Id;
        private long _Count;
        private readonly AggregateProperty[] Properties;
    }
}
