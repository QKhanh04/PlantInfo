using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult<PagedResult<CategoryDTO>>> GetPagedCategoriesAsync(string? keyword, int page, int pageSize);
        Task<ServiceResult<Category>> CreateCategoryAsync(CategoryDTO dto);

        Task<ServiceResult<Category>> UpdateCategoryAsync(CategoryDTO dto);
        Task<ServiceResult<Category>> GetByIdAsync(int id);
        Task<ServiceResult<IEnumerable<Category>>> GetAllCategoryAsync();

    }
}