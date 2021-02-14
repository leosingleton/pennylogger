// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PennyLogger.Internals
{
    /// <summary>
    /// <para>
    /// This class is a workaround for https://github.com/dotnet/runtime/issues/33598
    /// </para>
    /// <para>
    /// Unfortunately, the <see cref="JsonSerializer.Serialize{TValue}(TValue, JsonSerializerOptions)"/> methods in
    /// System.Text.Json use an non-public buffer writer that performs better than a standard MemoryStream, so it is
    /// not possible to construct an optimized <see cref="Utf8JsonWriter"/> using the public constructor.
    /// </para>
    /// </summary>
    internal static class Utf8JsonSerializer
    {
        /// <summary>
        /// Serializes an object to a string using a <see cref="Utf8JsonWriter"/>
        /// </summary>
        /// <param name="writeLambda">
        /// Lambda function which receives the <see cref="Utf8JsonWriter"/> and writes using its methods
        /// </param>
        /// <returns>String output of <see cref="Utf8JsonWriter"/></returns>
        public static string Write(Action<Utf8JsonWriter> writeLambda)
        {
            var obj = new DummyType { WriteLambda = writeLambda };
            return JsonSerializer.Serialize(obj);
        }

        [JsonConverter(typeof(DummyConverter))]
        private struct DummyType
        {
            public Action<Utf8JsonWriter> WriteLambda;
        }

        private class DummyConverter : JsonConverter<DummyType>
        {
            public override DummyType Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options) => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, DummyType value, JsonSerializerOptions options)
            {
                value.WriteLambda(writer);
            }
        }
    }
}
