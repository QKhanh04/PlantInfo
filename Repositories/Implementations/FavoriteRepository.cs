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
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        private readonly PlantDbContext _context;

        public FavoriteRepository(PlantDbContext context) : base(context)
        {
            _context = context;
        }



        public async Task<bool> IsFavoriteAsync(int userId, int plantId)
        {
            return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.PlantId == plantId);
        }

        public async Task<List<Plant>> GetFavoritePlantsAsync(int userId)
        {
            // Trả về danh sách cây đã yêu thích của user
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Join(_context.Plants, f => f.PlantId, p => p.PlantId, (f, p) => p)
                .ToListAsync();
        }

        public async Task<Favorite?> GetFavoriteAsync(int userId, int plantId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlantId == plantId);
        }

        public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Favorites.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(f => f.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(f => f.CreateAt <= endDate.Value);
            return await query.CountAsync();
        }


    }
}