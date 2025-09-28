using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<ServiceResult<IEnumerable<Category>>> GetAllCategories();
        Task<ServiceResult<Category>> CreateAsync(Category category);

    }
}