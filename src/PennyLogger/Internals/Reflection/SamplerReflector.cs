// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PennyLogger.Internals.Reflection
{
    /// <summary>
    /// Helper class for reading an object returned by a sampler lambda
    /// </summary>
    internal class SamplerReflector : ObjectReflector
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplerLambda">
        /// Lambda function executed every interval. The return value is an object with properties to log.
        /// </param>
        /// <param name="samplerType">Type of object to reflect</param>
        /// <param name="paramOptions">
        /// Event options, supplied via parameter to
        /// <see cref="IPennyLogger.Sample(Func{object}, Type, PennySamplerOptions)"/>. Note that this value may be
        /// null, and does not include options passed via configuration, which may have higher prority.
        /// </param>
        public SamplerReflector(Func<object> samplerLambda, Type samplerType, PennySamplerOptions paramOptions = null)
        {
            var properties = new List<PropertyReflector>();

            // Reflect object-level attributes
            SamplerAttribute = samplerType.GetCustomAttribute<PennySamplerAttribute>();

            // If an Event ID was specified in the attributes or parameters, use that. We construct a "miniConfig" to do
            // this, which only calculates the Id property. It doesn't take into account other attributes or
            // configuration, so all other properties may be incorrect.
            var miniConfig = PennySamplerConfig.Create(null, paramOptions, SamplerAttribute);
            if (miniConfig.Id != null)
            {
                Id = miniConfig.Id;
                properties.Add(new PropertyReflectorConstantString("Event", miniConfig.Id));
            }

            // Reflect properties
            var props = samplerType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            properties.AddRange(props.Select(p => CreatePropertyReflector(p)).OfType<PropertyReflector>());

            // Reflect fields
            var fields = samplerType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            properties.AddRange(fields.Select(f => CreatePropertyReflector(f)).OfType<PropertyReflector>());

            // Ensure there is exactly one event name property. If none is set, use the type's name.
            var nameMembers = properties.Where(m => m.Name == "Event");
            int nameMembersCount = nameMembers.Count();
            if (nameMembersCount == 0)
            {
                string name = samplerType.Name;
                if (name.EndsWith("Sampler"))
                {
                    name = name.Substring(0, name.Length - "Sampler".Length);
                }

                Id = name;
                var dynamicIdProperty = new PropertyReflectorConstantString("Event", name);
                properties.Insert(0, dynamicIdProperty);
            }
            else if (nameMembersCount == 1)
            {
                var dynamicIdProperty = nameMembers.First() as PropertyReflector<string>;
                if (dynamicIdProperty == null)
                {
                    throw new ArgumentException("Properties named \"Event\" must be of type string or enum");
                }

                var eventObject = samplerLambda();
                Id = dynamicIdProperty.GetValue(eventObject);
            }
            else
            {
                throw new ArgumentException($"{samplerType.FullName} has multiple Event ID properties");
            }

            _Properties = properties.ToArray();
        }

        protected override PropertyReflector[] Properties => _Properties;
        private readonly PropertyReflector[] _Properties;

        /// <summary>
        /// [PennySampler] attribute. Null if the class definition does not have this attribute.
        /// </summary>
        public PennySamplerAttribute SamplerAttribute { get; private set; }

        /// <summary>
        /// Event ID
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="EventReflector"/>, samplers do not allow a single type to dynamically change its Event ID
        /// using an &quot;Event&quot; property. Thus, this is a property unlike the
        /// <see cref="EventReflector.GetEventId(object)"/> method.
        /// </remarks>
        public string Id { get; private set; }
    }
}
