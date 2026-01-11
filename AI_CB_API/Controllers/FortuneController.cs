using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AI_CB_API.Models;

namespace AI_CB_API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FortuneController : ControllerBase
	{
		private readonly AI_CB_API.Services.IAIService _aiService;

		private static readonly string[] QuestionPool = new[]
		{
			"What is your favorite color?",
			"What's your favorite animal?",
			"What city would you love to visit?",
			"What is your favorite food?",
			"Which season do you prefer?",
			"Pick a number between 1 and 100.",
			"What hobby brings you joy?",
			"What's a word that describes you?",
			"What is your lucky number?",
			"Choose: sunrise or sunset?"
		};

		public FortuneController(AI_CB_API.Services.IAIService aiService)
		{
			_aiService = aiService;
		}

		[HttpGet("questions")]
		public IActionResult GetQuestions()
		{
			var rnd = new Random();
			var pool = new List<string>(QuestionPool);
			var selected = new List<string>();
			for (int i = 0; i < 3 && pool.Count > 0; i++)
			{
				var idx = rnd.Next(pool.Count);
				selected.Add(pool[idx]);
				pool.RemoveAt(idx);
			}
			return Ok(selected);
		}

		public class FortuneRequest
		{
			public List<string> Answers { get; set; }
		}

		[HttpPost("generate")]
		public async Task<IActionResult> Generate([FromBody] FortuneRequest req)
		{
			if (req?.Answers == null)
				return BadRequest("Answers are required.");

			// Build a prompt from the answers
			var prompt = "You are a fortune-teller. Based on the user's answers, produce a short, poetic fortune (1-2 sentences).\n" +
						 "Answers:\n" + string.Join('\n', req.Answers);

			var request = new OpenAiRequest { Prompt = prompt, Model = "meta-llama/Meta-Llama-3-8B-Instruct", MaxTokens = 150, Temperature = 0.8 };
			var (success, content, status) = await _aiService.GenerateAsync(request);

			if (!success)
				return StatusCode(status, content);

			// parse content for assistant text (keep compatibility with OpenAI/HF raw responses)
			try
			{
				using var doc = JsonDocument.Parse(content);
				var root = doc.RootElement;
				if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
				{
					var first = choices[0];
					if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var msgContent))
					{
						return Ok(new { fortune = msgContent.GetString() });
					}
					else if (first.TryGetProperty("text", out var text))
					{
						return Ok(new { fortune = text.GetString() });
					}
				}

				return Ok(new { fortune = content });
			}
			catch (JsonException)
			{
				return Ok(new { fortune = content });
			}
		}
	}
}

