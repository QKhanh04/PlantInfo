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
        Task<ServiceResult<IEnumerable<Category>>> GetAllCategoryAsync();
        Task<ServiceResult<Category>> CreateCategoryAsync(Category category);
    }
}