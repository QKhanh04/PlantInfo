using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IPlantService
    {

        Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(string? keyword,
   int page,
   int pageSize,
   int? categoryId,
   string? orderName

);

        Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(
            string? keyword,
            int page,
            int pageSize,
            List<int>? categoryIds,
            List<int>? useIds,
            List<int>? diseaseIds,
            string? orderName,
            bool? isFavorited,
            int? userId
        );
        Task<ServiceResult<PlantDetailDTO>> GetDetailPlantAsync(int id);
        Task<ServiceResult<Plant>> GetPlantByIdAsync(int id);
        Task<ServiceResult<PlantDTO>> CreatePlantAsync(PlantCreateDTO plant);
        Task<ServiceResult<PlantDTO>> UpdatePlantAsync(PlantUpdateDTO plant);
        Task<ServiceResult<bool>> DeletePlantAsync(int id);
        Task<ServiceResult<IEnumerable<PlantDetailDTO>>> GetAllPlantAsync();
    }

}