using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface IUseService
    {
        Task<ServiceResult<Use>> CreateUseAsync(UseDTO dto);
        Task<ServiceResult<Use>> UpdateUseAsync(UseDTO dto);
        Task<ServiceResult<IEnumerable<Use>>> GetAllUsesAsync();
        Task<ServiceResult<Use>> GetByIdAsync(int id);
        Task<ServiceResult<PagedResult<UseDTO>>> GetPagedUsesAsync(string? keyword, int page, int pageSize);
        Task<List<Plant>> GetPlantsByUseIdAsync(int useId);
        Task<ServiceResult<bool>> DeleteUseAsync(int useId);

    }
}