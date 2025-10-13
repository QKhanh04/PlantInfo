using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Repositories.Implementations
{
    public class ViewLogRepository : GenericRepository<ViewLog>, IViewLogRepository
    {
        private readonly PlantDbContext _context;
        public ViewLogRepository(PlantDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ViewLogs.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(f => f.ViewDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(f => f.ViewDate <= endDate.Value);
            return await query.CountAsync();
        }
    }
}