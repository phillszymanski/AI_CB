using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using AI_CB_API.Models;
using System.IO;

namespace AI_CB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAIController : ControllerBase
    {
        private readonly AI_CB_API.Services.IAIService _aiService;

        public OpenAIController(AI_CB_API.Services.IAIService aiService)
        {
            _aiService = aiService;
        }
        [HttpPost("generate")]  // Endpoint to generate text
        public async Task<IActionResult> GenerateText([FromBody] OpenAiRequest request)
        {
            Console.WriteLine("Received request with prompt: " + request.Prompt + " inputs: " + request.Inputs);

            var (success, content, status) = await _aiService.GenerateAsync(request);
            Console.WriteLine($"AI service responded: {status} - {content}");

            if (success)
                return Ok(content);

            return StatusCode(status, content);
        }

        [HttpGet("status")]  // Endpoint to check API status
        public IActionResult GetStatus()
        {
            return Ok("OpenAI API is reachable.");
        }

        [HttpPost("debug/echo")]
        public async Task<IActionResult> EchoRaw()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("EchoRaw received body: " + body);
            return Ok(new { received = body });
        }
    }
}