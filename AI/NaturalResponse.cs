using System.Text.Json;
using System.Threading.Tasks;
using PlantManagement.Services.Implementations;

namespace PlantManagement.AI
{
    public class NaturalResponse
    {
        private readonly GeminiService _geminiService;

        public NaturalResponse(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public async Task<string> GenerateResponseAsync(string userQuestion, object? data, bool useAiForNaturalLanguage = true)
        {
            if (!useAiForNaturalLanguage || data == null)
                return "Xin lỗi, tôi không tìm thấy dữ liệu phù hợp để trả lời.";

            string jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            string prompt = @$"
Bạn là trợ lý ảo nông nghiệp thông minh 🌱.
Hãy trả lời câu hỏi của người dùng dựa trên dữ liệu dưới đây, bằng **tiếng Việt tự nhiên**, rõ ràng và dễ đọc.

**Yêu cầu:**
- Không nói 'theo dữ liệu bạn cung cấp' hoặc 'theo dữ liệu trong hệ thống'.
- Trả lời ngắn gọn, súc tích, tập trung vào câu hỏi.
- Cuối cùng có thể viết một đoạn tóm tắt ngắn gọn nếu cần thiết.
- Dùng định dạng Markdown đẹp

**Câu hỏi:** {userQuestion}

**Dữ liệu JSON:**
{jsonData}
";

            var response = await _geminiService.AskGeminiAsync(prompt);

            return response ?? "Xin lỗi, tôi chưa thể tạo phản hồi tự nhiên cho yêu cầu này.";
        }
    }
}
