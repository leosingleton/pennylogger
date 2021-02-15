// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Exceptions;
using PennyLogger.Mocks.UnitTests;
using System;
using Xunit;

namespace PennyLogger.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="PennyLoggerAspNetCore"/> class
    /// </summary>
    public partial class PennyLoggerTest
    {
        private static void Init(out MockLogger<PennyLoggerAspNetCore> mock, out PennyLogger logger,
            out MockOptionsMonitor options)
        {
            mock = new MockLogger<PennyLoggerAspNetCore>();
            options = new MockOptionsMonitor(new PennyLoggerOptions());
            logger = new PennyLoggerAspNetCore(mock, options);
        }

        private static void Init(out MockLogger<PennyLoggerAspNetCore> mock, out PennyLogger logger)
        {
            Init(out mock, out logger, out var _);
        }

        /// <summary>
        /// Tests <see cref="PennyLoggerExceptionExtensions.Exception(IPennyLogger, Exception)"/>
        /// </summary>
        [Fact]
        public void Exception()
        {
            Init(out var mock, out var logger);

            logger.Exception(new ArgumentException("MyException", "MyParam"));

            Assert.Single(mock.LogHistory);
            string entry = mock.LogHistory[0];
            Assert.Contains("ArgumentException", entry);
            Assert.Contains("MyException", entry);
            Assert.Contains("MyParam", entry);
        }
    }
}
