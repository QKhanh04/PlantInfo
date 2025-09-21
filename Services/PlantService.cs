using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Services;



namespace PlantManagement.Service
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepo;
        public PlantService(IPlantRepository plantRepo)
        {
            _plantRepo = plantRepo;
        }
        // 1. Danh sách + tìm kiếm + phân trang

        public async Task<ServiceResult<PagedResult<Plant>>> GetPagedAsync(
   string? keyword,
   int page,
   int pageSize,
   int? categoryId = null,
   int? useId = null,
   int? diseaseId = null

)
        {
            try
            {
                var query = _plantRepo.Query()
                    .Include(p => p.Species)
                    .Include(p => p.GrowthCondition)
                    .Include(p => p.Categories)
                    .Include(p => p.Uses)
                    .Include(p => p.Diseases)
                    .AsQueryable();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(p =>
                        p.CommonName.Contains(keyword) ||
                        (p.Species != null && p.Species.ScientificName.Contains(keyword)));
                }
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.Categories.Any(c => c.CategoryId == categoryId.Value));
                }
                if (useId.HasValue)
                {
                    query = query.Where(p => p.Uses.Any(u => u.UseId == useId.Value));
                }
                if (diseaseId.HasValue)
                {
                    query = query.Where(p => p.Diseases.Any(d => d.DiseaseId == diseaseId.Value));
                }
               

                var total = await query.CountAsync();
                var data = await query
                    .OrderBy(p => p.CommonName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<Plant>
                {
                    Items = data,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                };
                return ServiceResult<PagedResult<Plant>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<Plant>>.Fail($"Lỗi khi lấy dữ liệu: {ex.Message}");
            }
        }
        // 2. Lấy chi tiết
        public async Task<ServiceResult<Plant>> GetByIdAsync(int id)
        {
            var plant = await _plantRepo.GetByIdAsync(id);
            return plant == null
                ? ServiceResult<Plant>.Fail("Plant not found")
                : ServiceResult<Plant>.Ok(plant);
        }
        // 3. Thêm
        public async Task<ServiceResult<Plant>> CreateAsync(Plant plant)
        {
            try
            {
                await _plantRepo.AddAsync(plant);
                await _plantRepo.SaveChangesAsync();
                return ServiceResult<Plant>.Ok(plant, "Plant created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Plant>.Fail($"Error creating plant: {ex.Message}");
            }
        }
        // 4. Sửa
        public async Task<ServiceResult<Plant>> UpdateAsync(Plant plant)
        {
            try
            {
                _plantRepo.Update(plant);
                await _plantRepo.SaveChangesAsync();
                return ServiceResult<Plant>.Ok(plant, "Plant updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Plant>.Fail($"Error updating plant: {ex.Message}");
            }
        }
        // 5. Xoá
        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                var plant = await _plantRepo.GetByIdAsync(id);
                if (plant == null)
                    return ServiceResult<bool>.Fail("Plant not found");
                _plantRepo.Remove(plant);
                await _plantRepo.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true, "Plant deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Error deleting plant: {ex.Message}");
            }
        }
    }
}
 
