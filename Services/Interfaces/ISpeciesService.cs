using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories;

namespace PlantManagement.Services.Interfaces
{
    public interface ISpeciesService
    {
        Task<List<string>> GetDistinctOrderNameAsync();
        Task<ServiceResult<PagedResult<SpeciesDTO>>> GetPagedSpeciesAsync(string? keyword, int page, int pageSize);
        Task<ServiceResult<Species>> CreateSpeciesAsync(SpeciesDTO dto);
        Task<ServiceResult<Species>> UpdateSpeciesAsync(SpeciesDTO dto);
        Task<ServiceResult<Species>> GetByIdAsync(int id);
        Task<List<Plant>> GetPlantsBySpeciesIdAsync(int speciesId);

        Task<ServiceResult<IEnumerable<Species>>> GetAllSpeciesAsync();
        Task<ServiceResult<bool>> DeleteSpeciesAsync(int speciesId);



    }
}