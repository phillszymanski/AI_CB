using AI_CB_API.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AI_CB_API.Services
{
    public interface IAIService
    {
        Task<(bool Success, string Content, int StatusCode)> GenerateAsync(OpenAiRequest request);
    }

    public class AIService : IAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AIService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<(bool Success, string Content, int StatusCode)> GenerateAsync(OpenAiRequest request)
        {
            var provider = _config["AIProvider"] ?? Environment.GetEnvironmentVariable("AI_PROVIDER") ?? "OpenAI";
            var httpClient = _httpClientFactory.CreateClient();

            if (provider.Equals("HuggingFace", StringComparison.OrdinalIgnoreCase))
            {
                var hfKey = _config["HuggingFace:ApiKey"] ?? Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY");
                httpClient.BaseAddress = new Uri("https://router.huggingface.co/");
                if (!string.IsNullOrEmpty(hfKey))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfKey);
            }
            else
            {
                var apiKey = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                httpClient.BaseAddress = new Uri("https://api.openai.com/");
                if (!string.IsNullOrEmpty(apiKey))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            // Accept HF-style `inputs` as alternative to `prompt`
            if (string.IsNullOrEmpty(request.Prompt) && !string.IsNullOrEmpty(request.Inputs))
            {
                request.Prompt = request.Inputs;
            }

            var model = string.IsNullOrEmpty(request.Model) ? "gpt-3.5-turbo" : request.Model;

            try
            {
                HttpResponseMessage response;

                if (provider.Equals("HuggingFace", StringComparison.OrdinalIgnoreCase))
                {
                    var hfModel = model;
                    if (string.IsNullOrEmpty(hfModel) || hfModel.StartsWith("gpt-3") || hfModel.StartsWith("gpt-"))
                        hfModel = "gpt2";

                    var lower = hfModel.ToLowerInvariant();
                    var isChatLike = lower.Contains("llama") || lower.Contains("instruct") || lower.Contains("chat") || lower.Contains("alpaca") || lower.Contains("vicuna");

                    if (isChatLike)
                    {
                        var hfPayload = new
                        {
                            model = hfModel,
                            messages = new[]
                            {
                                new { role = "system", content = "You are a helpful assistant." },
                                new { role = "user", content = request.Prompt }
                            },
                            max_tokens = request.MaxTokens,
                            temperature = request.Temperature
                        };

                        response = await httpClient.PostAsJsonAsync($"v1/chat/completions", hfPayload);
                    }
                    else
                    {
                        var hfPayload = new
                        {
                            inputs = string.IsNullOrEmpty(request.Prompt) ? request.Inputs : request.Prompt,
                            parameters = new
                            {
                                max_new_tokens = request.MaxTokens ?? 150,
                                temperature = request.Temperature ?? 0.7
                            }
                        };

                        response = await httpClient.PostAsJsonAsync($"models/{hfModel}", hfPayload);
                    }
                }
                else
                {
                    if (model.StartsWith("gpt-") || model.Contains("turbo"))
                    {
                        var chatPayload = new
                        {
                            model = model,
                            messages = new[] { new { role = "user", content = request.Prompt } },
                            max_tokens = request.MaxTokens,
                            temperature = request.Temperature
                        };
                        response = await httpClient.PostAsJsonAsync("v1/chat/completions", chatPayload);
                    }
                    else
                    {
                        var payload = new
                        {
                            model = model,
                            prompt = request.Prompt,
                            max_tokens = request.MaxTokens,
                            temperature = request.Temperature
                        };

                        response = await httpClient.PostAsJsonAsync("v1/chat/completions", payload);
                    }
                }

                var respText = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, respText, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, 500);
            }
        }
    }
}
