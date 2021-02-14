// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Text.Json;

namespace PennyLogger.Internals.Raw
{
    /// <summary>
    /// Helper class for serializing an event object to JSON as a raw log event
    /// </summary>
    /// <remarks>
    /// Although PennyLogger supports dynamically changing configuration at runtime, instances of <see cref="RawEvent"/>
    /// and its child <see cref="RawProperty"/> instances do not. The instance must be deleted and recreated on
    /// configuration change.
    /// </remarks>
    internal class RawEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="properties">Object properties</param>
        public RawEvent(RawProperty[] properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Reads an event object and serializes its properties to a JSON writer
        /// </summary>
        /// <param name="writer">JSON writer where the output is written</param>
        /// <param name="eventObject">Event object to read</param>
        public void Serialize(Utf8JsonWriter writer, object eventObject)
        {
            writer.WriteStartObject();

            foreach (var property in Properties)
            {
                property.Serialize(writer, eventObject);
            }

            writer.WriteEndObject();
        }

        private readonly RawProperty[] Properties;
    }
}
