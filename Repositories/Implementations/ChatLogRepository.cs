using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Data;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Repositories.Implementations
{
    public class ChatLogRepository : GenericRepository<ChatLog>, IChatLogRepository
    {
        public ChatLogRepository(PlantDbContext context) : base(context)
        {
        }
    }
}