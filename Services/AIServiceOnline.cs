using System.Text.Json;
using System.Text;

namespace PsihoApi.Services
{
    public class AIServiceOnline
    {
        private readonly string _apiKey = ""; // Use OpenAI API key

        public async Task<string> AnalyzeText(string userText)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var requestBody = new
            {
                model = "gpt-4o-mini",
                store = true,
                messages = new[]
                {
                new { role = "system", content = "Ești un psiholog AI care oferă sprijin emoțional." },
                new { role = "user", content = userText }
            }
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
    }
}
