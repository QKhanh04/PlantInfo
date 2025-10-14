using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;

namespace PlantManagement.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<ServiceResult<int>> CountAsync(DateTime? startDate, DateTime? endDate);
        Task<bool> AddFavoriteAsync(int userId, int plantId);
        Task<bool> RemoveFavoriteAsync(int userId, int plantId);

        Task<bool> IsFavoriteAsync(int userId, int plantId);
        Task<List<PlantListDTO>> GetFavoritePlantsAsync(int userId);
    }
}