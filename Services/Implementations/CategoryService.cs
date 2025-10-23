using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<Category>> CreateCategoryAsync(CategoryDTO dto)
        {
            try
            {
                var category = _mapper.Map<Category>(dto);
                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();
                return ServiceResult<Category>.Ok(category, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Category>.Fail($"Error creating Category: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Category>> UpdateCategoryAsync(CategoryDTO dto)
        {
            try
            {
                var existing = await _categoryRepository.GetByIdAsync(dto.CategoryId);
                if (existing == null)
                {
                    return ServiceResult<Category>.Fail($"Category not found");

                }
                _mapper.Map(dto, existing);
                _categoryRepository.Update(existing);
                await _categoryRepository.SaveChangesAsync();
                return ServiceResult<Category>.Ok(existing, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Category>.Fail($"Error updating Category: {ex.Message}");
            }
        }
        public async Task<ServiceResult<IEnumerable<Category>>> GetAllCategoryAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return ServiceResult<IEnumerable<Category>>.Ok(categories);
        }


        public async Task<ServiceResult<PagedResult<CategoryDTO>>> GetPagedCategoriesAsync(string? keyword, int page, int pageSize)
        {
            var query = _categoryRepository.Query();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string loweredKeyword = keyword.ToLower();
                query = query.Where(c => c.CategoryName.ToLower().Contains(loweredKeyword));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.CategoryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => _mapper.Map<CategoryDTO>(c))
                .ToListAsync();

            var pagedResult = new PagedResult<CategoryDTO>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<CategoryDTO>>.Ok(pagedResult);
        }

        public async Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null
                ? ServiceResult<Category>.Fail("Category not found")
                : ServiceResult<Category>.Ok(category);
        }


    }
}