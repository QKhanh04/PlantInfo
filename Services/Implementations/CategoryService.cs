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
                var exists = await _categoryRepository.Query()
           .AnyAsync(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());
                if (exists)
                {
                    return ServiceResult<Category>.Fail("Category name already exists. Please choose a different name.");
                }
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

        public async Task<List<Plant>> GetPlantsByCategoryIdAsync(int categoryId)
        {
            return await _categoryRepository.GetPlantsByCategoryIdAsync(categoryId);
        }


        // public async Task<ServiceResult<bool>> DeleteCategoryAsync(int categoryId)
        // {
        //     var plants = await _categoryRepository.GetPlantsByCategoryIdAsync(categoryId);

        //     if (plants.Any())
        //     {
        //         string plantList = string.Join(", ", plants.Select(p => p.CommonName ?? p.Species?.ScientificName));
        //         return ServiceResult<bool>.Fail(
        //             $"Không thể xóa vì danh mục đang được dùng bởi {plants.Count} cây: {plantList}"
        //         );
        //     }

        //     var category = await _categoryRepository.GetByIdAsync(categoryId);
        //     if (category == null)
        //         return ServiceResult<bool>.Fail("Không tìm thấy danh mục.");

        //     _categoryRepository.Delete(category);
        //     await _categoryRepository.SaveChangesAsync();
        //     return ServiceResult<bool>.Ok(true, "Xóa danh mục thành công.");
        // }

        public async Task<ServiceResult<bool>> DeleteCategoryAsync(int categoryId)
        {
            var plants = await _categoryRepository.GetPlantsByCategoryIdAsync(categoryId);

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return ServiceResult<bool>.Fail("Không tìm thấy danh mục.");

            // ✅ Nếu có cây thì chỉ cảnh báo chứ vẫn cho phép xóa
            string message;
            if (plants.Any())
            {
                string plantList = string.Join(", ", plants.Select(p => p.CommonName ?? p.Species?.ScientificName));
                message = $"Danh mục đã được xóa, mặc dù đang được sử dụng bởi {plants.Count} cây: {plantList}";
            }
            else
            {
                message = "Xóa danh mục thành công.";
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true, message);
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