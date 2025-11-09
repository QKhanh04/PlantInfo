using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Helper;
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


        public async Task<Plant?> FindByNameAsync(string keyword)
        {
            keyword = TextHelper.NormalizeKeyword(keyword);

            return await _dbSet
                .Include(p => p.Species)
                .FirstOrDefaultAsync(p =>
                    EF.Functions.ILike(p.CommonName, $"%{keyword}%") ||
                    EF.Functions.ILike(p.Species.ScientificName, $"%{keyword}%"));
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