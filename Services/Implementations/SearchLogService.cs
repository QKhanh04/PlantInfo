using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class SearchLogService : ISearchLogService
    {
        private readonly ISearchLogRepository _searchLogRepository;
        public SearchLogService(ISearchLogRepository searchLogRepository)
        {
            _searchLogRepository = searchLogRepository;
        }
        public async Task AddSearchLogAsync(string keyword, int? userId = null)
        {
            var searchLog = new SearchLog
            {
                UserId = userId,
                Keyword = keyword.ToLowerInvariant(),
                SearchDate = DateTime.Now
            };
            await _searchLogRepository.AddAsync(searchLog);
            await _searchLogRepository.SaveChangesAsync();
        }

        public Task<ServiceResult<int>> CountPlantSearchAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }



    }
}