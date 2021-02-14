// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace PennyLogger.Filters
{
    /// <summary>
    /// Action filter that can be added to controllers to automatically log HTTP request/response data to PennyLogger
    /// </summary>
    public class PennyLoggerFilter : IResourceFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">PennyLogger service instance</param>
        public PennyLoggerFilter(IPennyLogger logger)
        {
            Logger = logger;
        }

        private readonly IPennyLogger Logger;
        
        /// <inheritdoc/>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // The ActionFilter instance can be shared between multiple requests. We must use the HttpContext to store
            // per-request state.
            context.HttpContext.Items[ContextItemKey] = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var startTime = (Stopwatch)context.HttpContext.Items[ContextItemKey];

            Logger.Event(new RequestEvent
            {
                Method = request.Method,
                Path = request.Path,
                Query = request.QueryString.Value,
                Status = response.StatusCode,
                Time = startTime.ElapsedMilliseconds
            });
        }

        private const string ContextItemKey = "PennyLogger.StartTime";

        /// <summary>
        /// Structured event logged by this filter
        /// </summary>
        private class RequestEvent
        {
            /// <summary>
            /// HTTP method
            /// </summary>
            public string Method { get; set; }

            /// <summary>
            /// Path of the HTTP request
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Query string of the HTTP request
            /// </summary>
            public string Query { get; set; }

            /// <summary>
            /// HTTP status code of the response
            /// </summary>
            [PennyPropertyEnumerable]
            public int Status { get; set; }

            /// <summary>
            /// Time to process the HTTP request, in milliseconds
            /// </summary>
            public long Time { get; set; }
        }
    }
}
