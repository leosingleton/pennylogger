// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PennyLogger.Internals.Dictionary.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="TopNDictionary{T}"/>
    /// </summary>
    public class TopNDictionaryTest
    {
        private void AssertOneItem<T>(TopNDictionary<T> d, T value, long count)
        {
            var values = d.AsEnumerable();
            Assert.Single(values);
            Assert.Equal(new KeyValuePair<T, long>(value, count), values.First());
        }

        private void AssertTwoItems<T>(TopNDictionary<T> d, T value1, long count1, T value2, long count2)
        {
            var values = d.AsEnumerable();
            Assert.Equal(2, values.Count());
            Assert.Equal(new KeyValuePair<T, long>(value1, count1), values.First());
            Assert.Equal(new KeyValuePair<T, long>(value2, count2), values.Last());
        }

        /// <summary>
        /// Increments a value and tracks its count
        /// </summary>
        [Fact]
        public void Increments()
        {
            var d = new TopNDictionary<string>(3);

            d.Increment("Test");
            d.Increment("Test");
            d.Increment("Test");

            AssertOneItem(d, "Test", 3);
        }

        /// <summary>
        /// Increment() accepts null as a value
        /// </summary>
        [Fact]
        public void IncrementsNull()
        {
            var d = new TopNDictionary<string>(3);

            d.Increment(null);
            d.Increment(null);
            d.Increment(null);

            AssertOneItem(d, null, 3);
        }

        /// <summary>
        /// Adds a value to the collection
        /// </summary>
        [Fact]
        public void Add()
        {
            var d = new TopNDictionary<string>(3);

            d.Add("Test", 7);

            AssertOneItem(d, "Test", 7);
        }

        /// <summary>
        /// Add() accepts null as a value
        /// </summary>
        [Fact]
        public void AddNull()
        {
            var d = new TopNDictionary<string>(3);

            d.Add(null, 7);

            AssertOneItem(d, null, 7);
        }

        /// <summary>
        /// Throws exception when trying to insert more than N items via Add()
        /// </summary>
        [Fact]
        public void EnforcesLimitOnAdd()
        {
            var d = new TopNDictionary<int>(3);

            d.Add(0, 1);
            d.Add(1, 1);
            Assert.Equal(0, d.MinCount);
            d.Add(2, 1);
            Assert.Equal(1, d.MinCount);
            Assert.Throws<ArgumentException>(() => d.Add(3, 1));
        }

        /// <summary>
        /// Throws exception when trying to insert more than N items via Increment()
        /// </summary>
        [Fact]
        public void EnforcesLimitOnIncrement()
        {
            var d = new TopNDictionary<int>(3);

            d.Increment(0);
            d.Increment(1);
            Assert.Equal(0, d.MinCount); 
            d.Increment(2);
            Assert.Equal(1, d.MinCount); 
            Assert.Throws<ArgumentException>(() => d.Increment(3));
        }

        /// <summary>
        /// Throws exception when trying to insert more than N items via Increment()
        /// </summary>
        [Fact]
        public void EnforcesLimitOnIncrementNull()
        {
            var d = new TopNDictionary<string>(3);

            d.Increment("Test");
            d.Increment("Hello");
            Assert.Equal(0, d.MinCount);
            d.Increment("World");
            Assert.Equal(1, d.MinCount);
            Assert.Throws<ArgumentException>(() => d.Increment(null));
        }

        /// <summary>
        /// Drops lowest value, where N = 1
        /// </summary>
        [Fact]
        public void DropLowest1()
        {
            var d = new TopNDictionary<string>(1);

            d.Add("Test", 3);
            AssertOneItem(d, "Test", 3);
            Assert.Equal(3, d.MinCount);

            d.Add(null, 4);
            AssertOneItem(d, null, 4);
            Assert.Equal(4, d.MinCount);

            d.Add("Hello", 5);
            AssertOneItem(d, "Hello", 5);
            Assert.Equal(5, d.MinCount);

            d.Add("World", 6);
            AssertOneItem(d, "World", 6);
            Assert.Equal(6, d.MinCount);
        }

        /// <summary>
        /// Drops lowest value, where N = 2
        /// </summary>
        [Fact]
        public void DropLowest2()
        {
            var d = new TopNDictionary<string>(2);

            d.Add("Test1", 3);
            AssertOneItem(d, "Test1", 3);
            Assert.Equal(0, d.MinCount);

            d.Add("Test2", 1);
            AssertTwoItems(d, "Test1", 3, "Test2", 1);
            Assert.Equal(1, d.MinCount);

            d.Add(null, 2);
            AssertTwoItems(d, "Test1", 3, null, 2);
            Assert.Equal(2, d.MinCount);

            d.Add("Hello", 6);
            AssertTwoItems(d, "Hello", 6, "Test1", 3);
            Assert.Equal(3, d.MinCount);

            d.Add("World", 5);
            AssertTwoItems(d, "Hello", 6, "World", 5);
            Assert.Equal(5, d.MinCount);
        }

        /// <summary>
        /// Tests the Clear() function
        /// </summary>
        [Fact]
        public void Clear()
        {
            var d = new TopNDictionary<string>(3);

            d.Add("Test", 7);
            AssertOneItem(d, "Test", 7);
            Assert.Equal(1, d.Count);

            d.Clear();
            Assert.Equal(0, d.Count);
            Assert.Equal(0, d.MinCount);
            Assert.Empty(d.AsEnumerable());
        }
    }
}
