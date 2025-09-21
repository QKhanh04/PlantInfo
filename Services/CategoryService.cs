using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories;

namespace PlantManagement.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<ServiceResult<IEnumerable<Category>>> GetAllCategories()
        {
            return ServiceResult<IEnumerable<Category>>.Ok(await _categoryRepository.GetAllAsync(), "Categories retrieved successfully");
        }
    }
}