using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;

namespace PlantManagement.Services
{
    public interface IPlantService
    {
        Task<ServiceResult<PagedResult<Plant>>> GetPagedAsync(string? keyword,
   int page,
   int pageSize,
   int? categoryId,
   int? useId,
   int? diseaseId);
        Task<ServiceResult<Plant>> GetByIdAsync(int id);
        Task<ServiceResult<Plant>> CreateAsync(Plant plant);
        Task<ServiceResult<Plant>> UpdateAsync(Plant plant);
        Task<ServiceResult<bool>> DeleteAsync(int id);
    }
}
