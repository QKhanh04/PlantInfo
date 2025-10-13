using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ServiceResult<Category>> CreateCategoryAsync(Category category)
        {
            try
            {
                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();
                return ServiceResult<Category>.Ok(category, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Category>.Fail($"Error creating Category: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Category>>> GetAllCategoryAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories == null ? ServiceResult<IEnumerable<Category>>.Fail("Have No Category") : ServiceResult<IEnumerable<Category>>.Ok(categories);
        }


    }
}