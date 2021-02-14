// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PennyLogger.Mocks.UnitTests
{
    /// <summary>
    /// An empty event with ID derived from the class name
    /// </summary>
    [PennyEventRawLogging]
    class MockEmptyEvent
    {
        public string GetExpectedJson() => "{\"Event\":\"MockEmpty\"}";
    }

    /// <summary>
    /// An empty event with ID overridden via attributes
    /// </summary>
    [PennyEvent(Id = "MockIDAttributeEvent")]
    [PennyEventRawLogging]
    class MockIDAttributeEvent
    {
        public string GetExpectedJson() => "{\"Event\":\"MockIDAttributeEvent\"}";
    }

    /// <summary>
    /// An event with a dynamically-specified ID property
    /// </summary>
    [PennyEventRawLogging(Max = 5, Interval = 300)]
    class MockDynamicIDEvent
    {
        public string Event { get; set; }

        public string GetExpectedJson() => "{\"Event\":\"" + Event + "\"}";
    }

    /// <summary>
    /// An event containing an integer that is aggregated with default settings
    /// </summary>
    class MockIntEvent
    {
        public int Value { get; set; }

        public static string GetExpectedJson(int count, int min, int max, int sum) =>
            "{\"Event\":\"MockInt$\",\"Count\":" + count + ",\"Value\":{\"Min\":" + min + ",\"Max\":" + max +
            ",\"Sum\":" + sum + "}}";
    }

    /// <summary>
    /// An event containing an enum that is aggregated with default settings
    /// </summary>
    class MockEnumEvent
    {
        public MockEnumValue Value;

        public static string GetExpectedJson(int count, IDictionary<string, int> valueCounts) =>
            "{\"Event\":\"MockEnum$\",\"Count\":" + count + ",\"Value\":" +
            JsonSerializer.Serialize(valueCounts) + "}";
    }

    enum MockEnumValue
    {
        Value1,
        Value2,
        Value3
    }

    /// <summary>
    /// An event containing an enumerable string that is aggregated with Top-5
    /// </summary>
    class MockStringTop5Event
    {
        [PennyProperty(IgnoreNull = false)]
        [PennyPropertyEnumerable(Top = 5)]
        public string Value { get; set; }

        public static string GetExpectedJson(int count, IDictionary<string, int> valueCounts) =>
            "{\"Event\":\"MockStringTop5$\",\"Count\":" + count + ",\"Value\":" +
            JsonSerializer.Serialize(valueCounts) + "}";
    }

    /// <summary>
    /// An event containing a string with a short truncation
    /// </summary>
    [PennyEventRawLogging]
    class MockTruncateStringEvent
    {
        [PennyProperty(MaxLength = 10)]
        public string Value { get; set; }

        public static string GetExpectedJson(string value) =>
            "{\"Event\":\"MockTruncateString\",\"Value\":\"" + value + "\"}";
    }

    /// <summary>
    /// An event to test all supported types
    /// </summary>
    [PennyEventAggregateLogging]
    [PennyEventRawLogging]
    class MockAllTypesEvent
    {
        public string String { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public uint UInt32 { get; set; }
        public ulong UInt64 { get; set; }
        public float Single { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public bool Bool { get; set; }
        public Type Type { get; set; }
        public Guid Guid { get; set; }
    }

    /// <summary>
    /// An event disabled via attributes
    /// </summary>
    [PennyEvent(Enabled = false)]
    [PennyEventAggregateLogging]
    [PennyEventRawLogging]
    class MockDisableEvent
    {
        public string Value { get; set; }
    }

    /// <summary>
    /// An event with one property disabled via attributes
    /// </summary>
    [PennyEventAggregateLogging]
    [PennyEventRawLogging]
    class MockDisablePropertyEvent
    {
        public int Number { get; set; }

        [PennyProperty(Enabled = false)]
        public string String { get; set; }
    }
}
