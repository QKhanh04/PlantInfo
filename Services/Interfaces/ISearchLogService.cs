using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;

namespace PlantManagement.Services.Interfaces
{
    public interface ISearchLogService
    {
        Task AddSearchLogAsync(string keyword, int? userId = null);

        Task<ServiceResult<int>> CountPlantSearchAsync(DateTime? startDate = null, DateTime? endDate = null);

    }
}