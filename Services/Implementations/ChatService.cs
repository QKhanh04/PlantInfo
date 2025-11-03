    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using PlantManagement.Data;
    using PlantManagement.Models;
    using PlantManagement.Repositories.Interfaces;
    using PlantManagement.Services.Interfaces;

    namespace PlantManagement.Services.Implementations
    {
        public class ChatService : IChatLogService
        {
            private readonly IChatLogRepository _chatLogRepository;
            // private readonly AiService _aiService;

            public ChatService(IChatLogRepository chatLogRepository/*, AiService aiService*/)
            {
                _chatLogRepository = chatLogRepository;
                // _aiService = aiService;
            }

            public async Task<ChatLog> SaveMessageAsync(ChatLog chatLog)
            {
                await _chatLogRepository.AddAsync(chatLog);
                await _chatLogRepository.SaveChangesAsync();
                return chatLog;
            }

            public async Task<List<ChatLog>> GetChatHistoryAsync(int? userId, string? sessionId)
            {
                IQueryable<ChatLog> query = _chatLogRepository.Query();

                if (userId != null)
                    query = query.Where(l => l.UserId == userId);
                else if (!string.IsNullOrEmpty(sessionId))
                    query = query.Where(l => l.SessionId == sessionId);
                else
                    return new List<ChatLog>();

                return await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(20)
                .Reverse()
                .ToListAsync();
            }

            public async Task MergeChatSessionAsync(int userId, string sessionId)
            {
                if (string.IsNullOrEmpty(sessionId))
                    return;

                var chats = await _chatLogRepository
                    .Query()
                    .Where(c => c.SessionId == sessionId && c.UserId == null)
                    .ToListAsync();

                if (chats.Count == 0)
                    return;

                foreach (var chat in chats)
                {
                    chat.UserId = userId;
                    chat.SessionId = null; // gỡ sessionId sau khi merge
                }

                await _chatLogRepository.SaveChangesAsync();
            }

            /// <summary>
            /// Xóa các chat cũ của khách (session) sau X ngày
            /// </summary>
            // public async Task<int> CleanupOldGuestChatsAsync(int days = 7)
            // {
            //     var cutoff = DateTime.UtcNow.AddDays(-days);
            //     var oldChats = await _chatLogRepository
            //         .Query()
            //         .Where(c => c.UserId == null && c.CreatedAt < cutoff)
            //         .ToListAsync();

            //     if (oldChats.Count == 0)
            //         return 0;

            //     _chatLogRepository.DeleteRange(oldChats);
            //     await _chatLogRepository.SaveChangesAsync();

            //     return oldChats.Count;
            // }




            // public async Task<string> GetBotResponseAsync(string userMessage)
            // {
            //     // Gọi API AI (OpenAI hoặc Gemini)
            //     // string aiReply = await _aiService.GetResponseAsync(userMessage);

            //     // TODO: bạn có thể thêm truy vấn DB ở đây (nếu cần dữ liệu thực)
            //     return aiReply;
            // }

        }
    }