using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IDiseaseService
    {
        Task<ServiceResult<Disease>> CreateDiseaseAsync(DiseaseDTO dto);
        Task<ServiceResult<Disease>> UpdateDiseaseAsync(DiseaseDTO dto);
        Task<ServiceResult<IEnumerable<Disease>>> GetAllDiseasesAsync();
        Task<ServiceResult<Disease>> GetByIdAsync(int id);
        Task<ServiceResult<PagedResult<DiseaseDTO>>> GetPagedDiseasesAsync(string? keyword, int? plantId, int page, int pageSize);
        Task<ServiceResult<bool>> DeleteDiseaseAsync(int diseaseId);
        Task<ServiceResult<IEnumerable<PlantDTO>>> GetAllPlantsForDropdownAsync();
    }
}