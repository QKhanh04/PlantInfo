using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlantManagement.Services.Implementations
{
    public class GeminiService
    {

        private readonly string _apiKey = null!;
        private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ChatbotKey"] ?? _apiKey;
        }
        public async Task<string> AskGeminiAsync(string prompt)
        {
            using (var client = new HttpClient())
            {
                var url = $"{_endpoint}?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine("==== RESPONSE FROM AI ====");
                Console.WriteLine(responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error {response.StatusCode}: {responseBody}");
                }

                try
                {
                    using var doc = JsonDocument.Parse(responseBody);

                    if (doc.RootElement.TryGetProperty("candidates", out var candidates)
                        && candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        var extractedText = text ?? "Không có phản hồi từ Gemini.";
                        Console.WriteLine("==== AI CONTENT (Extracted) ====");
                        Console.WriteLine(extractedText);

                        Console.WriteLine("==== JSON REQUEST SENT ====");
                        Console.WriteLine(json);

                        return extractedText;
                    }

                    return "Gemini không trả về nội dung.";
                }
                catch (Exception ex)
                {
                    return $"Lỗi khi phân tích phản hồi AI: {ex.Message}";
                }
            }
        }
    }
}
