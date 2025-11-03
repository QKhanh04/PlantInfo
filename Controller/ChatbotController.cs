using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlantManagement.AI;
using PlantManagement.Models;
using PlantManagement.Repositories.Implementations;
using PlantManagement.Services.Implementations;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Controller
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        // private readonly GeminiService _geminiService;
        private readonly IChatLogService _chatLogService;
        private readonly GeminiSqlGenerator _geminiSqlGenerator;
        private readonly NaturalResponse _naturalResponse;

        public ChatbotController(IChatLogService chatLogService, GeminiSqlGenerator geminiSqlGenerator, NaturalResponse naturalResponse)
        {
            // _geminiService = geminiService;
            _chatLogService = chatLogService;
            _geminiSqlGenerator = geminiSqlGenerator;
            _naturalResponse = naturalResponse;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { reply = "Tin nhắn không được để trống." });

            try
            {

                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(idClaim, out var parsedId))
                        userId = parsedId;
                }
                var sessionId = userId == null ? request.SessionId : null;
                // Log user message
                await _chatLogService.SaveMessageAsync(new ChatLog
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Sender = "user",
                    Message = request.Message,
                });

                // ✅ 2. Lấy lịch sử hội thoại gần nhất
                var chatHistory = await _chatLogService.GetChatHistoryAsync(userId, sessionId);
                // Giữ 5 tin gần nhất cho AI
                var recentMessages = chatHistory
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Reverse()
                    .ToList();

                var contextText = string.Join("\n", recentMessages.Select(c => $"{c.Sender}: {c.Message}"));

                // ✅ 3. Tạo prompt đầy đủ
                var fullPrompt = $@"
Đây là lịch sử hội thoại gần đây:
{contextText}

Người dùng vừa hỏi: {request.Message}
Hãy trả lời tự nhiên, ngắn gọn, và nếu có thể hãy dùng dữ liệu thực trong database.
";
                // Gọi Gemini API qua service
                var reply = await _geminiSqlGenerator.RunQueryFlowAsync(fullPrompt);
                // Log ai message

                string botMessage;

                if (!reply.IsSuccess)
                    botMessage = reply.Message;
                else if (reply.Rows.Any())
                    botMessage = JsonSerializer.Serialize(reply.Rows, new JsonSerializerOptions { WriteIndented = true });
                else
                    botMessage = $"✅ SQL đã chạy thành công:\n{reply.GeneratedSql}";

                object dataForAi = reply.Rows.Any() ? reply.Rows : new { note = "Không có dữ liệu trả về" };

                string naturalReply = await _naturalResponse.GenerateResponseAsync(request.Message, dataForAi, useAiForNaturalLanguage: true);
                await _chatLogService.SaveMessageAsync(new ChatLog
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Sender = "bot",
                    Message = naturalReply,
                });

                return Ok(new { reply = naturalReply });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi chatbot: {ex.Message}");
                return StatusCode(500, new { reply = "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại." });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string? sessionId)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsedId))
                    userId = parsedId;
            }

            var logs = await _chatLogService.GetChatHistoryAsync(userId, sessionId);
            return Ok(logs.Select(l => new
            {
                sender = l.Sender,
                message = l.Message,
                createdAt = l.CreatedAt
            }));
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }
}