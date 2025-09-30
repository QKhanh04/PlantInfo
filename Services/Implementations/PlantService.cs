using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.Data;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services;
using PlantManagement.Services.Interfaces;



namespace PlantManagement.Services.Implementations
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepo;
        private readonly ISpeciesRepository _speciesRepo;
        private readonly IGrowthConditionRepository _growthRepo;
        private readonly IDiseaseRepository _diseaseRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IUseRepository _useRepo;
        private readonly IPlantImageRepository _imageRepo;
        private readonly IPlantReferenceRepository _referenceRepo;
        private readonly PlantDbContext _dbContext;
        private readonly IMapper _mapper;

        public PlantService(
            IPlantRepository plantRepo,
            ISpeciesRepository speciesRepo,
            IGrowthConditionRepository growthRepo,
            IDiseaseRepository diseaseRepo,
            ICategoryRepository categoryRepo,
            IUseRepository useRepo,
            IPlantImageRepository imageRepo,
            IPlantReferenceRepository referenceRepo,
            PlantDbContext dbContext,
            IMapper mapper)
        {
            _plantRepo = plantRepo;
            _speciesRepo = speciesRepo;
            _growthRepo = growthRepo;
            _diseaseRepo = diseaseRepo;
            _categoryRepo = categoryRepo;
            _useRepo = useRepo;
            _imageRepo = imageRepo;
            _referenceRepo = referenceRepo;
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public async Task<ServiceResult<PlantDTO>> CreatePlantAsync(PlantCreateDTO model)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Xử lý species
                int speciesId;
                if (model.SpeciesId.HasValue)
                {
                    var exist = await _speciesRepo.GetByIdAsync(model.SpeciesId.Value);
                    if (exist == null)
                        return ServiceResult<PlantDTO>.Fail("Loài không tồn tại");
                    speciesId = model.SpeciesId.Value;
                }
                else if (model.NewSpecies != null)
                {
                    var exist = await _speciesRepo.FindAsync(s => s.ScientificName == model.NewSpecies.ScientificName);
                    if (exist.Any())
                        return ServiceResult<PlantDTO>.Fail("Tên khoa học đã tồn tại");

                    var newSpecies = _mapper.Map<Species>(model.NewSpecies);
                    await _speciesRepo.AddAsync(newSpecies);
                    await _speciesRepo.SaveChangesAsync();
                    speciesId = newSpecies.SpeciesId;
                }
                else
                {
                    return ServiceResult<PlantDTO>.Fail("Bạn phải chọn loài có sẵn hoặc thêm mới loài");
                }
                // 2. Plant
                var plant = new Plant
                {
                    CommonName = model.CommonName,
                    Origin = model.Origin,
                    Description = model.Description,
                    SpeciesId = speciesId,
                    IsActive = true,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now
                };
                await _plantRepo.AddAsync(plant);
                await _plantRepo.SaveChangesAsync();
                // 3. GrowthCondition
                if (model.GrowthCondition != null)
                {
                    var gc = _mapper.Map<GrowthCondition>(model.GrowthCondition);
                    gc.PlantId = plant.PlantId;
                    await _growthRepo.AddAsync(gc);
                }
                // 4. Diseases
                if (model.Diseases != null)
                {
                    foreach (var d in model.Diseases)
                    {
                        var disease = _mapper.Map<Disease>(d);
                        disease.PlantId = plant.PlantId;
                        await _diseaseRepo.AddAsync(disease);
                    }
                }
                // 5. Categories
                if (model.CategoryIds != null)
                {
                    var categories = await _categoryRepo.FindAsync(c => model.CategoryIds.Contains(c.CategoryId));
                    foreach (var cat in categories)
                    {
                        plant.Categories.Add(cat);
                    }
                }

                if (model.NewCategories != null)
                {
                    foreach (var catDto in model.NewCategories)
                    {
                        if (!string.IsNullOrWhiteSpace(catDto.CategoryName))
                        {
                            // Kiểm tra trùng tên phân loại
                            var existCat = await _categoryRepo.FindAsync(c => c.CategoryName == catDto.CategoryName);
                            if (existCat.Any()) continue; // Có rồi thì bỏ qua

                            var cat = _mapper.Map<Category>(catDto);
                            await _categoryRepo.AddAsync(cat);
                            await _categoryRepo.SaveChangesAsync();
                            plant.Categories.Add(cat);
                        }
                    }
                }

                // 6. Uses
                if (model.UseIds != null)
                {
                    var uses = await _useRepo.FindAsync(u => model.UseIds.Contains(u.UseId));
                    foreach (var use in uses)
                    {
                        plant.Uses.Add(use);
                    }
                }

                if (model.NewUses != null)
                {
                    foreach (var useDto in model.NewUses)
                    {
                        if (!string.IsNullOrWhiteSpace(useDto.UseName))
                        {
                            // Kiểm tra trùng tên công dụng
                            var existUse = await _useRepo.FindAsync(u => u.UseName == useDto.UseName);
                            if (existUse.Any()) continue; // Có rồi thì bỏ qua

                            var use = _mapper.Map<Use>(useDto);
                            await _useRepo.AddAsync(use);
                            await _useRepo.SaveChangesAsync();
                            plant.Uses.Add(use);
                        }
                    }
                }
                // 7. Images
                if (model.Images != null)
                {
                    foreach (var img in model.Images)
                    {
                        var entity = _mapper.Map<PlantImage>(img);
                        entity.PlantId = plant.PlantId;
                        await _imageRepo.AddAsync(entity);
                    }
                }
                // 8. References
                if (model.References != null)
                {
                    foreach (var r in model.References)
                    {
                        var entity = _mapper.Map<PlantReference>(r);
                        entity.PlantId = plant.PlantId;
                        await _referenceRepo.AddAsync(entity);
                    }
                }
                // 9. Lưu + Commit transaction
                await _plantRepo.SaveChangesAsync();
                await transaction.CommitAsync();
                return ServiceResult<PlantDTO>.Ok(
                    _mapper.Map<PlantDTO>(plant),
                    "Thêm cây thành công"
                );
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync();
                return ServiceResult<PlantDTO>.Fail($"Lỗi khi thêm cây: {ex.Message}");
            }
        }
        // 1. Danh sách + tìm kiếm + phân trang

        public async Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(
   string? keyword,
   int page,
   int pageSize,
   int? categoryId,
   string? orderName

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
                if (!string.IsNullOrEmpty(orderName))
                {
                    query = query.Where(p => p.Species != null && p.Species.OrderName == orderName);
                }



                var total = await query.CountAsync();
                var data = await query
                    .OrderBy(p => p.CommonName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtoList = _mapper.Map<List<PlantListDTO>>(data);


                var result = new PagedResult<PlantListDTO>
                {
                    Items = dtoList,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                };
                return ServiceResult<PagedResult<PlantListDTO>>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<PlantListDTO>>.Fail($"Lỗi khi lấy dữ liệu: {ex.Message}");
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
                .Include(p => p.PlantImages)
                    .Include(p => p.PlantReferences)

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

