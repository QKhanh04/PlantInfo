using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PlantManagement.Common.Results;

namespace PlantManagement.AI
{
    public class GeminiModeration
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiKey = null!;
        private readonly ILogger<GeminiModeration> _logger;

        public GeminiModeration(IConfiguration config, HttpClient httpClient, ILogger<GeminiModeration> logger)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ModerationKey"] ?? _apiKey;
            _logger = logger;
        }

        public async Task<ServiceResult<bool>> IsCommentAllowedAsync(string comment)
        {
            string model = "gemini-2.0-flash"; // model nhanh, chi phí thấp
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            // 🔹 Prompt kiểm duyệt chi tiết
            var requestBody = new
            {
                contents = new[]
                {
            new {
                role = "user",
                parts = new[] {
                    new {
                        text = $@"
Bạn là hệ thống kiểm duyệt bình luận. 
Nhiệm vụ: Phân tích nội dung bình luận: '{comment}' và chỉ TRẢ VỀ DUY NHẤT một JSON hợp lệ (không kèm chữ nào khác).

Yêu cầu JSON:
{{
  ""allow"": boolean,                    
  ""category"": string,                  
  ""severity"": string,                  
  ""reason"": string,                    
  ""evidence"": [string],                
  ""suggestedAction"": string            
}}

Quy tắc:
1. Nếu chứa ngôn từ thù ghét, xúc phạm, tục tĩu, spam hoặc tấn công cá nhân → allow=false
2. Nếu bình luận bình thường → allow=true, category=""safe"", severity=""low""
3. Nếu không chắc chắn → allow=true, reason=""unable_to_classify""
4. Chỉ trả về JSON, không có giải thích hoặc ký tự khác.
5. Nếu bình luận không có ý nghĩa (chỉ gồm ký tự lặp, không phải từ ngữ tự nhiên) → allow=false, category=""nonsense"", reason=""Bình luận vô nghĩa"".


Ví dụ:
Input: 'Bạn thật ngu quá!'
Output:
{{""allow"": false, ""category"": ""harassment"", ""severity"": ""medium"", ""reason"": ""Xúc phạm cá nhân"", ""evidence"": [""ngu""], ""suggestedAction"": ""block""}}
"
                    }
                }
            }
        },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.0,
                    maxOutputTokens = 300
                }
            };

            try
            {
                // 🟡 Gửi request
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("=== Gemini API Raw Response ===");
                _logger.LogInformation(json);
                _logger.LogInformation("===============================");

                // Kiểm tra lỗi HTTP
                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult<bool>.Ok(true, $"Gemini API lỗi: {response.StatusCode}");
                }

                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                {
                    return ServiceResult<bool>.Ok(true, "Phản hồi không chứa candidates — cho phép tạm");
                }

                var text = candidates[0].GetProperty("content")
                                        .GetProperty("parts")[0]
                                        .GetProperty("text").GetString();

                _logger.LogInformation("=== Gemini Parsed Text ===");
                _logger.LogInformation(text ?? "(null)");
                _logger.LogInformation("==========================");

                if (string.IsNullOrWhiteSpace(text))
                    return ServiceResult<bool>.Ok(true, "Phản hồi rỗng — cho phép tạm");

                // ✳️ Bước 1: loại bỏ ký tự không cần thiết nếu AI lỡ trả kèm dấu ```json ... ```
                text = text.Trim().Trim('`').Trim();

                // ✳️ Bước 2: parse phần text trả về thành JSON
                using var innerJson = JsonDocument.Parse(text);
                bool allow = innerJson.RootElement.GetProperty("allow").GetBoolean();
                string? reason = innerJson.RootElement.TryGetProperty("reason", out var reasonProp)
                    ? reasonProp.GetString()
                    : null;

                return ServiceResult<bool>.Ok(allow, reason ?? (allow ? "Bình luận hợp lệ" : "Bình luận không phù hợp"));
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"[GeminiModerationService] JSON parse error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GeminiModerationService] Unexpected error");
            }

            // 🟢 fallback — cho phép tạm thời nếu có lỗi
            return ServiceResult<bool>.Ok(true, "Không phân tích được phản hồi Gemini — cho phép tạm thời");
        }

    }
}