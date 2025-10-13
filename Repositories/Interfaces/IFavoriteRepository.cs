using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IFavoriteRepository : IGenericRepository<Favorite>
    {
        Task<int> CountAsync(DateTime? startDate, DateTime? endDate);
        Task<bool> IsFavoriteAsync(int userId, int plantId);
        Task<List<Plant>> GetFavoritePlantsAsync(int userId);
        Task<Favorite?> GetFavoriteAsync(int userId, int plantId);
    }
}