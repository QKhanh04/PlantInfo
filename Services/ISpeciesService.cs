using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;
using PlantManagement.Repositories;

namespace PlantManagement.Services
{
    public interface ISpeciesService
    {
        Task<List<string>> GetDistinctOrderNameAsync();

    }
}