using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace PlantManagement.Pages.Api
{
    public class ChatBotModel : PageModel
    {
        private readonly ILogger<ChatBotModel> _logger;

        public ChatBotModel(ILogger<ChatBotModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public class ChatRequest
        {
            public string Question { get; set; }
        }

        public async Task<IActionResult> OnPostAsync([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return new JsonResult(new { success = false, answer = "Vui lòng nhập câu hỏi." });

            // 🔹 Gọi AI thật ở đây (ví dụ OpenAI API hoặc mô phỏng)
            await Task.Delay(300); // mô phỏng độ trễ phản hồi
            var answer = $"Tôi đang tìm hiểu về '{request.Question}'. Tôi sẽ cập nhật câu trả lời chi tiết trong phiên bản tới!";

            return new JsonResult(new { success = true, answer });
        }
    }
}