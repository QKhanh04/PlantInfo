using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface ISpeciesRepository : IGenericRepository<Species>
    {
        Task<Species?> FindByScientificNameAsync(string name);
        Task<List<Species>> SearchAsync(string query, int limit = 5);

       Task<List<Plant>> GetPlantsBySpeciesIdAsync(int speciesId);

    }
}