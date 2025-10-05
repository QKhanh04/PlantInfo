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
        private readonly PlantDbContext _dbContext;

        public FavoriteRepository(PlantDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<int> CountAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _dbContext.Favorites.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(f => f.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(f => f.CreateAt <= endDate.Value);
            return await query.CountAsync();
        }
    }
}