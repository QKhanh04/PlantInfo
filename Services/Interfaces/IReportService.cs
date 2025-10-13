using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.DTOs;

namespace PlantManagement.Services.Interfaces
{
    public interface IReportService
    {
        Task<PlantSummaryDto> GetPlantSummaryAsync(DateTime? startDate, DateTime? endDate);
        Task<UserSummaryDto> GetUserSummaryAsync(DateTime? startDate, DateTime? endDate);
        Task<List<CategoryStatDto>> GetPlantCountByCategoryAsync(DateTime? startDate, DateTime? endDate);
        Task<List<FavoriteStatDto>> GetTopFavoritePlantsAsync(int topN, DateTime? startDate, DateTime? endDate);
        Task<List<KeywordStatDto>> GetTopSearchKeywordsAsync(int topN, DateTime? startDate, DateTime? endDate);
        Task<List<PlantMonthlyStatDto>> GetMonthlyNewPlantStatsAsync(int year);
        Task<List<UserMonthlyStatDto>> GetMonthlyNewUserStatsAsync(int year);
        Task<List<PlantViewStatDto>> GetTopViewedPlantsAsync(int top, DateTime? startDate, DateTime? endDate);
    }
}