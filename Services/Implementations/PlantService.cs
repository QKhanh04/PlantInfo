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
        private readonly ICategoryRepository _categoryRepo;
        private readonly IUseRepository _useRepo;
        private readonly IGrowthConditionRepository _growthRepo;
        private readonly IDiseaseRepository _diseaseRepo;
        private readonly IPlantImageRepository _imageRepo;
        private readonly IPlantReferenceRepository _referenceRepo;
        private readonly IMapper _mapper;
        private readonly PlantDbContext _dbContext;
 
        public PlantService(
           IPlantRepository plantRepo,
           ISpeciesRepository speciesRepo,
           ICategoryRepository categoryRepo,
           IUseRepository useRepo,
           IGrowthConditionRepository growthRepo,
           IDiseaseRepository diseaseRepo,
           IPlantImageRepository imageRepo,
           IPlantReferenceRepository referenceRepo, IMapper mapper, PlantDbContext dbContext)
        {
            _plantRepo = plantRepo;
            _speciesRepo = speciesRepo;
            _categoryRepo = categoryRepo;
            _useRepo = useRepo;
            _growthRepo = growthRepo;
            _diseaseRepo = diseaseRepo;
            _imageRepo = imageRepo;
            _referenceRepo = referenceRepo;
            _mapper = mapper;
            _dbContext = dbContext;
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
                    var lowerKeyword = keyword.ToLower().Trim();
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
 
        public async Task<ServiceResult<PlantDetailDTO>> GetDetailPlantAsync(int id)
        {
            var plant = await _plantRepo.Query()
                .Include(p => p.Species)
                .Include(p => p.Categories)
                .Include(p => p.PlantImages)
                .Include(p => p.Uses)
                .Include(p => p.Diseases)
                .Include(p => p.GrowthCondition)
                .Include(p => p.PlantReferences)
                .FirstOrDefaultAsync(p => p.PlantId == id);
 
            if (plant == null)
                return ServiceResult<PlantDetailDTO>.Fail("Không tìm thấy cây!");
 
            var dtoList = _mapper.Map<PlantDetailDTO>(plant);
 
 
            return ServiceResult<PlantDetailDTO>.Ok(dtoList);
        }
 
 
        // 2. Lấy chi tiết
        public async Task<ServiceResult<Plant>> GetPlantByIdAsync(int id)
        {
            var plant = await _plantRepo.GetByIdAsync(id);
            return plant == null
                ? ServiceResult<Plant>.Fail("Plant not found")
                : ServiceResult<Plant>.Ok(plant);
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
                if (model.DiseaseIds != null)
                {
                    // Chỉ liên kết bệnh cũ đã có
                    var diseases = await _diseaseRepo.FindAsync(d => model.DiseaseIds.Contains(d.DiseaseId));
                    foreach (var d in diseases)
                    {
                        plant.Diseases.Add(d);
                    }
                }
 
                if (model.NewDiseases != null)
                {
                    foreach (var diseaseDto in model.NewDiseases)
                    {
                        if (!string.IsNullOrWhiteSpace(diseaseDto.DiseaseName))
                        {
                            // Kiểm tra trùng tên bệnh
                            var existDis = await _diseaseRepo.FindAsync(ds => ds.DiseaseName == diseaseDto.DiseaseName);
                            if (existDis.Any()) continue; // Nếu đã có thì bỏ qua
 
                            var disease = _mapper.Map<Disease>(diseaseDto);
                            await _diseaseRepo.AddAsync(disease);
                            await _diseaseRepo.SaveChangesAsync();
                            plant.Diseases.Add(disease);
                        }
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
 
        // 4. Sửa
        public async Task<ServiceResult<PlantDTO>> UpdatePlantAsync(PlantUpdateDTO dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy cây cần cập nhật
                var plant = await _dbContext.Plants
                    .Include(p => p.Categories)
                    .Include(p => p.Uses)
                    .Include(p => p.PlantImages)
                    .Include(p => p.Diseases)
                    .Include(p => p.GrowthCondition)
                    .Include(p => p.PlantReferences)
                    .FirstOrDefaultAsync(p => p.PlantId == dto.PlantId);
 
                if (plant == null)
                    return ServiceResult<PlantDTO>.Fail("Không tìm thấy cây!");
 
                // 2. Cập nhật các trường cơ bản
                if (!string.IsNullOrWhiteSpace(dto.CommonName))
                    plant.CommonName = dto.CommonName;
 
                if (!string.IsNullOrWhiteSpace(dto.Origin))
                    plant.Origin = dto.Origin;
 
                if (!string.IsNullOrWhiteSpace(dto.Description))
                    plant.Description = dto.Description;
 
                if (dto.SpeciesId.HasValue)
                    plant.SpeciesId = dto.SpeciesId.Value;
 
                plant.UpdateAt = DateTime.Now;
 
                // 3. Cập nhật điều kiện sinh trưởng
                if (dto.GrowthCondition != null)
                {
                    if (plant.GrowthCondition != null)
                    {
                        // Sửa điều kiện
                        _mapper.Map(dto.GrowthCondition, plant.GrowthCondition);
                    }
                    else
                    {
                        var gc = _mapper.Map<GrowthCondition>(dto.GrowthCondition);
                        gc.PlantId = plant.PlantId;
                        await _dbContext.GrowthConditions.AddAsync(gc);
                    }
                }
 
                // 4. Cập nhật danh mục
                if (dto.CategoryIds != null)
                {
                    var newCats = await _dbContext.Categories
                        .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                        .ToListAsync();
                    plant.Categories.Clear();
                    foreach (var cat in newCats)
                    {
                        plant.Categories.Add(cat);
                    }
                }
                if (dto.NewCategories?.Any() == true)
                {
                    foreach (var catDto in dto.NewCategories)
                    {
                        if (!string.IsNullOrWhiteSpace(catDto.CategoryName))
                        {
                            var existCat = await _dbContext.Categories
                                .FirstOrDefaultAsync(c => c.CategoryName == catDto.CategoryName);
                            if (existCat == null)
                            {
                                var cat = _mapper.Map<Category>(catDto);
                                await _dbContext.Categories.AddAsync(cat);
                                await _dbContext.SaveChangesAsync();
                                plant.Categories.Add(cat);
                            }
                        }
                    }
                }
 
                // 5. Cập nhật công dụng
                if (dto.UseIds != null)
                {
                    var newUses = await _dbContext.Uses
                        .Where(u => dto.UseIds.Contains(u.UseId))
                        .ToListAsync();
                    plant.Uses.Clear();
                    foreach (var use in newUses)
                    {
                        plant.Uses.Add(use);
                    }
                }
                if (dto.NewUses?.Any() == true)
                {
                    foreach (var useDto in dto.NewUses)
                    {
                        if (!string.IsNullOrWhiteSpace(useDto.UseName))
                        {
                            var existUse = await _dbContext.Uses
                                .FirstOrDefaultAsync(u => u.UseName == useDto.UseName);
                            if (existUse == null)
                            {
                                var use = _mapper.Map<Use>(useDto);
                                await _dbContext.Uses.AddAsync(use);
                                await _dbContext.SaveChangesAsync();
                                plant.Uses.Add(use);
                            }
                        }
                    }
                }
 
                // 6. Cập nhật bệnh
                if (dto.DiseaseIds != null)
                {
                    var newDiseases = await _dbContext.Diseases
                        .Where(u => dto.DiseaseIds.Contains(u.DiseaseId))
                        .ToListAsync();
                    plant.Diseases.Clear();
                    foreach (var d in newDiseases)
                    {
                        var disease = _mapper.Map<Disease>(d);
                        disease.PlantId = plant.PlantId;
                        plant.Diseases.Add(disease);
                    }
                }
                if (dto.NewDiseases != null)
                {
                    foreach (var diseaseDto in dto.NewDiseases)
                    {
                        if (!string.IsNullOrWhiteSpace(diseaseDto.DiseaseName))
                        {
                            // Kiểm tra trùng tên bệnh
                            var existDis = await _diseaseRepo.FindAsync(ds => ds.DiseaseName == diseaseDto.DiseaseName);
                            if (existDis.Any()) continue; // Nếu đã có thì bỏ qua
 
                            var disease = _mapper.Map<Disease>(diseaseDto);
                            await _diseaseRepo.AddAsync(disease);
                            await _diseaseRepo.SaveChangesAsync();
                            plant.Diseases.Add(disease);
                        }
                    }
                }
 
                // 7. Cập nhật ảnh
                if (dto.Images != null)
                {
                    var dbImages = plant.PlantImages.ToList();
                    var dtoImageIds = dto.Images.Where(x => x.ImageId.HasValue).Select(x => x.ImageId.Value).ToList();
 
                    // XÓA các ảnh cũ không còn trong DTO
                    foreach (var dbImg in dbImages)
                    {
                        if (!dtoImageIds.Contains(dbImg.ImageId))
                        {
                            _dbContext.PlantImages.Remove(dbImg);
                        }
                    }
 
                    // UPDATE hoặc INSERT từng ảnh
                    foreach (var imgDto in dto.Images)
                    {
                        if (imgDto.ImageId.HasValue && imgDto.ImageId.Value > 0)
                        {
                            // UPDATE
                            var dbImg = dbImages.FirstOrDefault(x => x.ImageId == imgDto.ImageId.Value);
                            if (dbImg != null)
                            {
                                dbImg.ImageUrl = imgDto.ImageUrl;
                                dbImg.Caption = imgDto.Caption;
                                dbImg.IsPrimary = imgDto.IsPrimary;
                            }
                        }
                        else
                        {
                            // INSERT
                            var newImg = _mapper.Map<PlantImage>(imgDto);
                            newImg.PlantId = plant.PlantId;
                            await _dbContext.PlantImages.AddAsync(newImg);
                        }
                    }
                }
 
                // 8. Cập nhật tài liệu tham khảo
                // 8. Cập nhật tài liệu tham khảo
                if (dto.References != null)
                {
                    var dbRefs = plant.PlantReferences.ToList();
                    var dtoRefIds = dto.References.Where(x => x.ReferenceId.HasValue).Select(x => x.ReferenceId.Value).ToList();
 
                    // XÓA các reference cũ không còn trong DTO
                    foreach (var dbRef in dbRefs)
                    {
                        if (!dtoRefIds.Contains(dbRef.ReferenceId))
                        {
                            _dbContext.PlantReferences.Remove(dbRef);
                        }
                    }
 
                    // UPDATE hoặc INSERT từng reference
                    foreach (var refDto in dto.References)
                    {
                        if (refDto.ReferenceId.HasValue && refDto.ReferenceId.Value > 0)
                        {
                            // UPDATE
                            var dbRef = dbRefs.FirstOrDefault(x => x.ReferenceId == refDto.ReferenceId.Value);
                            if (dbRef != null)
                            {
                                dbRef.SourceName = refDto.SourceName;
                                dbRef.Url = refDto.Url;
                                dbRef.Author = refDto.Author;
                                dbRef.PublishedYear = refDto.PublishedYear;
                            }
                        }
                        else
                        {
                            // INSERT
                            var newRef = _mapper.Map<PlantReference>(refDto);
                            newRef.PlantId = plant.PlantId;
                            await _dbContext.PlantReferences.AddAsync(newRef);
                        }
                    }
                }
 
                // 9. Lưu thay đổi
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
 
                // 10. Map sang DTO để trả về
                var plantDTO = _mapper.Map<PlantDTO>(plant);
                return ServiceResult<PlantDTO>.Ok(plantDTO, "Cập nhật cây thành công!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<PlantDTO>.Fail("Có lỗi xảy ra khi cập nhật cây: " + ex.Message);
            }
        }
 
 
        // 5. Xoá
        public async Task<ServiceResult<bool>> DeletePlantAsync(int id)
        {
            try
            {
                var plant = await _plantRepo.GetByIdAsync(id);
                if (plant == null)
                    return ServiceResult<bool>.Fail("Plant not found");
                plant.IsActive = !plant.IsActive;
 
                await _plantRepo.SaveChangesAsync();
                string msg = (plant.IsActive ?? false) ? "Đã hiện cây thành công!" : "Đã ẩn cây thành công!";
                return ServiceResult<bool>.Ok(true, msg);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Error deleting plant: {ex.Message}");
            }
        }
 
        public async Task<ServiceResult<IEnumerable<Plant>>> GetAllPlantAsync()
        {
            var plants = await _plantRepo.GetAllAsync();
            return plants == null ? ServiceResult<IEnumerable<Plant>>.Fail("Have No Plant") : ServiceResult<IEnumerable<Plant>>.Ok(plants);
        }
    }
}

