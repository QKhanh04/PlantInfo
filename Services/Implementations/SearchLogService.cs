using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.Helper;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class SearchLogService : ISearchLogService
    {
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly IPlantRepository _plantRepository;
        public SearchLogService(ISearchLogRepository searchLogRepository, IPlantRepository plantRepository)
        {
            _searchLogRepository = searchLogRepository;
            _plantRepository = plantRepository;
        }
        public async Task AddSearchLogAsync(string keyword, int? userId = null)
        {
            var normalizedKeyword = TextHelper.NormalizeKeyword(keyword);
            var existingPlant = await _plantRepository.FindByNameAsync(normalizedKeyword);
            if (existingPlant != null)
            {
                return;
            }
            var searchLog = new SearchLog
            {
                UserId = userId,
                Keyword = normalizedKeyword,
                SearchDate = DateTime.Now
            };
            await _searchLogRepository.AddAsync(searchLog);
            await _searchLogRepository.SaveChangesAsync();
        }


        public async Task RemoveLogsForKeywordAsync(string plantName)
        {
            // Normalize tên cây để khớp với dữ liệu đã lưu trong bảng SearchLogs
            var normalizedName = TextHelper.NormalizeKeyword(plantName);

            var logsToRemove = await _searchLogRepository.Query()
                .Where(l => l.Keyword == normalizedName)
                .ToListAsync();

            if (logsToRemove.Any())
            {
                _searchLogRepository.RemoveRange(logsToRemove);
                await _searchLogRepository.SaveChangesAsync();
                Console.WriteLine($"[SearchLogService] Removed {logsToRemove.Count} logs for '{plantName}'");
            }
        }



    }
}