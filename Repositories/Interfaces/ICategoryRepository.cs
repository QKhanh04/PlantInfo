using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<List<Plant>> GetPlantsByCategoryIdAsync(int categoryId);
        Task<Category?> FindByNameAsync(string name);

    }
}