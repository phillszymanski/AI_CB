namespace AI_CB_API.Models
{
    public class OpenAiRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string Inputs { get; set; } = string.Empty;
        public string Model { get; set; } =  "gpt-3.5-turbo";
        public int? MaxTokens { get; set; } = 150;
        public double? Temperature { get; set; } = 0.7;
    }
}
// public class OpenAiRequest
// {
//     public string Prompt { get; set; }
// }