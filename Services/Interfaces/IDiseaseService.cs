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
        Task<ServiceResult<PagedResult<DiseasesDTO>>> GetPagedDiseasesAsync(string? keyword, int page, int pageSize);
        Task<ServiceResult<Disease>> CreateDiseaseAsync(DiseasesDTO dto);
        Task<ServiceResult<Disease>> UpdateDiseaseAsync(DiseasesDTO dto);
        Task<ServiceResult<Disease>> GetByIdAsync(int id);
        Task<ServiceResult<IEnumerable<Disease>>> GetAllDiseaseAsync();
    }
}