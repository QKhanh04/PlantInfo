using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IChatLogService
    {
        Task<ChatLog> SaveMessageAsync(ChatLog chatLog);
        Task<List<ChatLog>> GetChatHistoryAsync(int? userId, string? sessionId);
        Task MergeChatSessionAsync(int userId, string sessionId);
    }
}