using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IGrowthConditionService
    {
        Task<ServiceResult<GrowthCondition>> CreateGrowthConditionAsync(GrowthCondition growthCondition);
        Task<ServiceResult<IEnumerable<GrowthCondition>>> GetAllGrowthConditionAsync();
    }
}