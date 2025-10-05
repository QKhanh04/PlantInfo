using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;

namespace PlantManagement.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<ServiceResult<int>> CountAsync(DateTime? startDate, DateTime? endDate);
    }
}