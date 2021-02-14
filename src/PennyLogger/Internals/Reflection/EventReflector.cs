// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PennyLogger.Internals.Reflection
{
    /// <summary>
    /// Helper class for reading event objects using reflection
    /// </summary>
    internal class EventReflector : ObjectReflector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventType">Type of object to reflect</param>
        /// <param name="paramOptions">
        /// Event options, supplied via parameter to <see cref="IPennyLogger.Event(object, Type, PennyEventOptions)"/>.
        /// Note that this value may be null, and does not include options passed via configuration, which may have
        /// higher prority.
        /// </param>
        public EventReflector(Type eventType, PennyEventOptions paramOptions = null)
        {
            var properties = new List<PropertyReflector>();

            // Reflect object-level attributes
            EventAttribute = eventType.GetCustomAttribute<PennyEventAttribute>();
            AggregateLoggingAttribute = eventType.GetCustomAttribute<PennyEventAggregateLoggingAttribute>();
            RawLoggingAttribute = eventType.GetCustomAttribute<PennyEventRawLoggingAttribute>();

            // If an Event ID was specified in the attributes or parameters, use that. We construct a "miniConfig" to do
            // this, which only calculates the Id property. It doesn't take into account other attributes or
            // configuration, so all other properties may be incorrect.
            var miniConfig = PennyEventConfig.Create(null, paramOptions, EventAttribute);
            if (miniConfig.Id != null)
            {
                StaticIdString = miniConfig.Id;
                properties.Add(new PropertyReflectorConstantString("Event", miniConfig.Id));
            }

            // Reflect properties
            var props = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties.AddRange(props.Select(p => CreatePropertyReflector(p)).OfType<PropertyReflector>());

            // Reflect fields
            var fields = eventType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            properties.AddRange(fields.Select(f => CreatePropertyReflector(f)).OfType<PropertyReflector>());

            // Ensure there is exactly one event name property. If none is set, use the type's name.
            var nameMembers = properties.Where(m => m.Name == "Event");
            int nameMembersCount = nameMembers.Count();
            if (nameMembersCount == 0)
            {
                string name = eventType.Name;
                if (name.EndsWith("Event"))
                {
                    name = name.Substring(0, name.Length - "Event".Length);
                }

                DynamicIdProperty = new PropertyReflectorConstantString("Event", name);
                properties.Insert(0, DynamicIdProperty);
            }
            else if (nameMembersCount == 1)
            {
                DynamicIdProperty = nameMembers.First() as PropertyReflector<string>;
                if (DynamicIdProperty == null)
                {
                    throw new ArgumentException("Properties named \"Event\" must be of type string or enum");
                }
            }
            else
            {
                throw new ArgumentException($"{eventType.FullName} has multiple Event ID properties");
            }

            _Properties = properties.ToArray();
        }

        protected override PropertyReflector[] Properties => _Properties;
        private readonly PropertyReflector[] _Properties;

        /// <summary>
        /// [PennyEvent] attribute. Null if the class definition does not have this attribute.
        /// </summary>
        public PennyEventAttribute EventAttribute { get; private set; }

        /// <summary>
        /// [PennyEventAggregateLogging] attribute. Null if the class definition does not have this attribute.
        /// </summary>
        public PennyEventAggregateLoggingAttribute AggregateLoggingAttribute { get; private set; }

        /// <summary>
        /// [PennyEventRawLogging] attribute. Null if the class definition does not have this attribute.
        /// </summary>
        public PennyEventRawLoggingAttribute RawLoggingAttribute { get; private set; }

        /// <summary>
        /// Returns the Event ID. If the ID is not specified in the attributes or options, an instance of the event
        /// object may need to be reflected to look for a property named &quot;Event&quot;.
        /// </summary>
        /// <param name="eventObject">Event object to reflect to read the &quot;Event&quot; property</param>
        /// <returns>Event ID</returns>
        public string GetEventId(object eventObject) => StaticIdString ?? DynamicIdProperty?.GetValue(eventObject);

        // Only one of the following properties is set, depending on whether the Event ID is static or dynamic. The
        // other is always null.
        private readonly string StaticIdString;
        private readonly PropertyReflector<string> DynamicIdProperty;
    }
}
