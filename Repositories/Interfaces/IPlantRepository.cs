using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IPlantRepository : IGenericRepository<Plant>
    {
        Task<Plant?> FindByNameAsync(string name);
        Task<List<Plant>> SearchAsync(string query, int limit = 5);
        Task<Plant?> GetFullPlantInfoAsync(int id);
    }
}