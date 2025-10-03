using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<PlantSummaryDto> GetPlantSummaryAsync(DateTime? startDate, DateTime? endDate);
        Task<UserSummaryDto> GetUserSummaryAsync(DateTime? startDate, DateTime? endDate);
        Task<List<CategoryStatDto>> GetPlantCountByCategoryAsync(DateTime? startDate, DateTime? endDate);
        Task<List<FavoriteStatDto>> GetTopFavoritePlantsAsync(int topN, DateTime? startDate, DateTime? endDate);
        Task<List<KeywordStatDto>> GetTopSearchKeywordsAsync(int topN, DateTime? startDate, DateTime? endDate);
    }
}