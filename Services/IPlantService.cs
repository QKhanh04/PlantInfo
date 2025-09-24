using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Services
{
    public interface IPlantService
    {
        Task<ServiceResult<PagedResult<PlantDTO>>> GetPagedAsync(string? keyword,
   int page,
   int pageSize,
   int? categoryId);
        Task<ServiceResult<Plant>> GetByIdAsync(int id);
        Task<ServiceResult<Plant>> CreateAsync(Plant plant);
        Task<ServiceResult<Plant>> UpdateAsync(Plant plant);
        Task<ServiceResult<bool>> DeleteAsync(int id);


        Task<ServiceResult<PlantDetailDTO>> GetDetailAsync(int id);
    }
}