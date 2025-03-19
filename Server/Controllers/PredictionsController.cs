using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Server.Controllers
{
    
        [ApiController]
        [Route("api/predictions")]
    public class PredictionsController : ControllerBase
    {
        private static Dictionary<string, float> latestPredictions = new Dictionary<string, float>();
        private static bool isPostRequestCompleted = false;

        [HttpPost]
        public IActionResult ReceivePredictions([FromBody] Dictionary<string, float> predictions)
        {
            if (predictions == null || predictions.Count == 0)
            {
                return BadRequest("Invalid predictions data.");
            }

            latestPredictions = predictions;
            isPostRequestCompleted = true;

            return Ok(new { message = "Predictions received successfully", count = predictions.Count });
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestPredictions()
        {
            int timeoutMilliseconds = 10000;
            int checkIntervalMilliseconds = 500;
            int elapsedTime = 0;
            while (!isPostRequestCompleted && elapsedTime < timeoutMilliseconds)
            {
                await Task.Delay(checkIntervalMilliseconds);
                elapsedTime += checkIntervalMilliseconds;
            }

            if (!isPostRequestCompleted)
            {
                return BadRequest("POST request has not completed yet on the other application, timeout exceeded.");
            }

            if (latestPredictions.Count == 0)
            {
                return NotFound("No predictions available.");
            }

            return Ok(latestPredictions);
        }
    }
}
