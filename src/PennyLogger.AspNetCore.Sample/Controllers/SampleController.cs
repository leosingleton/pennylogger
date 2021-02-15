// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger;
using PennyLogger.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace PennyLogger.Sample.Controllers
{
    /// <summary>
    /// Sample ASP.NET Core controller leveraging the PennyLogger service loaded via dependency injection
    /// </summary>
    [ApiController]
    [Route("api")]
    public class SampleController : ControllerBase
    {
        public SampleController(IPennyLogger logger)
        {
            Logger = logger;
        }

        private readonly IPennyLogger Logger;

        [HttpGet("square/{value}")]
        public ActionResult<IntegerResult> Square(int value)
        {
            int result = value * value;

            // Log an event to the PennyLogger service
            Logger.Event(new
            {
                Value = value,
                Result = result
            }, new PennyEventOptions { Id = "Sample.SquareEvent" });

            return Ok(new IntegerResult
            {
                Result = result
            });
        }
    }
}
