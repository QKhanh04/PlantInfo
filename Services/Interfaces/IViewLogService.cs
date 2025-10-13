using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;

namespace PlantManagement.Services.Interfaces
{
    public interface IViewLogService
    {
        Task<ServiceResult<int>> CountPlantViewsAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task AddPlantViewLogAsync(int plantId, int? userId = null);
    }
}