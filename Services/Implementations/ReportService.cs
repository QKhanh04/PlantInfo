using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.DTOs;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<PlantSummaryDto> GetPlantSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetPlantSummaryAsync(startDate, endDate);
        }

        public async Task<UserSummaryDto> GetUserSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetUserSummaryAsync(startDate, endDate);
        }

        public async Task<List<CategoryStatDto>> GetPlantCountByCategoryAsync(DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetPlantCountByCategoryAsync(startDate, endDate);
        }

        public async Task<List<FavoriteStatDto>> GetTopFavoritePlantsAsync(int topN, DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetTopFavoritePlantsAsync(topN, startDate, endDate);
        }

        public async Task<List<KeywordStatDto>> GetTopSearchKeywordsAsync(int topN, DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetTopSearchKeywordsAsync(topN, startDate, endDate);
        }
        public async Task<List<PlantMonthlyStatDto>> GetMonthlyNewPlantStatsAsync(int year)
        {
            return await _reportRepository.GetMonthlyNewPlantStatsAsync(year);
        }

        public async Task<List<UserMonthlyStatDto>> GetMonthlyNewUserStatsAsync(int year)
        {
            return await _reportRepository.GetMonthlyNewUserStatsAsync(year);
        }

        public async Task<List<PlantViewStatDto>> GetTopViewedPlantsAsync(int top, DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetTopViewedPlantsAsync(top, startDate, endDate);
        }
    }
}