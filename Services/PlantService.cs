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
using PlantManagement.Services;



namespace PlantManagement.Service
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepo;
        private readonly IMapper _mapper;
        public PlantService(IPlantRepository plantRepo, IMapper mapper)
        {
            _plantRepo = plantRepo;
            _mapper = mapper;
        }
        // 1. Danh sách + tìm kiếm + phân trang

            public async Task<ServiceResult<PagedResult<PlantDTO>>> GetPagedAsync(
   string? keyword,
   int page,
   int pageSize,
   int? categoryId = null
)
        {
            try
            {
                var query = _plantRepo.Query()
                    .Include(p => p.Species)
                    .Include(p => p.Categories)
                    .Include(p => p.PlantImages)
                    .AsQueryable();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var lowerKeyword = keyword.ToLower();
                    query = query.Where(p =>
                        p.CommonName.ToLower().Contains(lowerKeyword) ||
                        (p.Species != null && p.Species.ScientificName.ToLower().Contains(lowerKeyword)));
                }
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.Categories.Any(c => c.CategoryId == categoryId.Value));
                }
 
 
                var total = await query.CountAsync();
                var data = await query
                    .OrderBy(p => p.CommonName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
 
                var dtoList = _mapper.Map<List<PlantDTO>>(data);
 
 
                var result = new PagedResult<PlantDTO>
                {
                    Items = dtoList,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                };
                return ServiceResult<PagedResult<PlantDTO>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<PlantDTO>>.Fail($"Lỗi khi lấy dữ liệu: {ex.Message}");
            }
        }
 

        public async Task<ServiceResult<PlantDetailDTO>> GetDetailAsync(int id)
        {
            var plant = await _plantRepo.Query()
                .Include(p => p.Species)
                .Include(p => p.Categories)
                .Include(p => p.PlantImages)
                .Include(p => p.Uses)
                .Include(p => p.Diseases)
                .Include(p => p.GrowthCondition)
                .FirstOrDefaultAsync(p => p.PlantId == id);
 
            if (plant == null)
                return ServiceResult<PlantDetailDTO>.Fail("Không tìm thấy cây!");
 
            var dtoList = _mapper.Map<PlantDetailDTO>(plant);
 
 
            return ServiceResult<PlantDetailDTO>.Ok(dtoList);
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
 
