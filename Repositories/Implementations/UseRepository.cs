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
    public class UseRepository : GenericRepository<Use>, IUseRepository
    {
        public UseRepository(PlantDbContext context) : base(context)
        {
        }
        public async Task<Use?> FindByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(u => EF.Functions.ILike(u.UseName, $"%{name}%"));
        }

         public async Task<List<Plant>> GetPlantsByUseIdAsync(int useId)
        {
            // Truy vấn qua navigation property
            var use = await _context.Uses
                .Include(u => u.Plants)
                    .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(u => u.UseId == useId);
            
            return use?.Plants.ToList() ?? new List<Plant>();
        }
    }
}