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
    public class ViewLogService : IViewLogService
    {
        private readonly IViewLogRepository _viewLogRepository;

        public ViewLogService(IViewLogRepository viewLogRepository)
        {
            _viewLogRepository = viewLogRepository;
        }

        public async Task AddPlantViewLogAsync(int plantId, int? userId = null)
        {
            var viewLog = new ViewLog
            {
                PlantId = plantId,
                UserId = userId,
                ViewDate = DateTime.Now
            };
            await _viewLogRepository.AddAsync(viewLog);
            await _viewLogRepository.SaveChangesAsync();
        }

        public async Task<ServiceResult<int>> CountPlantViewsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var count = await _viewLogRepository.CountAsync(startDate, endDate);
                return ServiceResult<int>.Ok(count);

            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Fail(ex.Message);
            }

        }

    }



}