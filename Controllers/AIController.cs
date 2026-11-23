using Microsoft.AspNetCore.Mvc;
using SmartBlood.DTO;
using SmartBlood.Services;

namespace SmartBlood.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIService _aiService;

        public AIController(AIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestPrediction(AIPredictionRequest request)
        {
            try
            {
                var result = await _aiService.PredictAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
        }
    }
}