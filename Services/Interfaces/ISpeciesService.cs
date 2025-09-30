using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories;

namespace PlantManagement.Services.Interfaces
{
    public interface ISpeciesService
    {
        Task<List<string>> GetDistinctOrderNameAsync();
        Task<ServiceResult<Species>> CreateAsync(Species species);
        Task<IEnumerable<Species>> GetAllAsync();
    }
}