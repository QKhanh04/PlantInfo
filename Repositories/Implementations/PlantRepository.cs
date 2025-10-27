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
    public class PlantRepository : GenericRepository<Plant>, IPlantRepository
    {

        private readonly PlantDbContext _ctx;

        public PlantRepository(PlantDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        // public async Task<Plant?> FindByNameAsync(string name)
        // {
        //     return await _ctx.Plants
        //         .Include(p => p.Species)
        //         .Include(p => p.GrowthCondition)
        //         .Include(p => p.Diseases)
        //         .Include(p => p.Uses)
        //         .Include(p => p.Categories)
        //         .Include(p => p.PlantImages)
        //         .FirstOrDefaultAsync(p => p.CommonName.ToLower().Contains(name.ToLower()));
        // }

        public async Task<Plant?> FindByNameAsync(string keyword)
        {
            return await _dbSet.FirstOrDefaultAsync(p =>
                p.CommonName.ToLower().Contains(keyword.ToLower()) ||
                p.Species.ScientificName.ToLower().Contains(keyword.ToLower()));
        }


        public async Task<List<Plant>> SearchAsync(string query, int limit = 5)
        {
            return await _ctx.Plants
                .Where(p => EF.Functions.Like(p.CommonName.ToLower(), $"%{query.ToLower()}%") ||
                            EF.Functions.Like(p.Description.ToLower(), $"%{query.ToLower()}%"))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Plant?> GetFullPlantInfoAsync(int id)
        {
            return await _ctx.Plants
                .Include(p => p.Species)
                .Include(p => p.GrowthCondition)
                .Include(p => p.Diseases)
                .Include(p => p.Uses)
                .Include(p => p.Categories)
                .Include(p => p.PlantImages)
                .FirstOrDefaultAsync(p => p.PlantId == id);
        }
    }
}