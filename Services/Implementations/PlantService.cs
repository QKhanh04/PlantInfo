using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.Data;
using PlantManagement.DTOs;
using PlantManagement.Helper;
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
        private readonly ISearchLogService _searchLogService;
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
           IPlantReferenceRepository referenceRepo,
           ISearchLogService searchLogService, IMapper mapper, PlantDbContext dbContext)
        {
            _plantRepo = plantRepo;
            _speciesRepo = speciesRepo;
            _categoryRepo = categoryRepo;
            _useRepo = useRepo;
            _growthRepo = growthRepo;
            _diseaseRepo = diseaseRepo;
            _imageRepo = imageRepo;
            _referenceRepo = referenceRepo;
            _searchLogService = searchLogService;
            _mapper = mapper;
            _dbContext = dbContext;
        }


        // 1. Danh sách + tìm kiếm + phân trang

        // 1. Danh sách + tìm kiếm + phân trang

        //         public async Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(
        //    string? keyword,
        //    int page,
        //    int pageSize,
        //    int? categoryId,
        //    string? orderName

        // )
        //         {
        //             try
        //             {
        //                 var query = _plantRepo.Query()
        //                     .Include(p => p.Species)
        //                     .Include(p => p.Categories)
        //                     .Include(p => p.PlantImages)
        //                     .AsQueryable();
        //                 if (!string.IsNullOrWhiteSpace(keyword))
        //                 {
        //                     var lowerKeyword = keyword.ToLower().Trim();
        //                     query = query.Where(p =>
        //                         p.CommonName.ToLower().Contains(lowerKeyword) ||
        //                         (p.Species != null && p.Species.ScientificName.ToLower().Contains(lowerKeyword)));
        //                 }
        //                 if (categoryId.HasValue)
        //                 {
        //                     query = query.Where(p => p.Categories.Any(c => c.CategoryId == categoryId.Value));
        //                 }
        //                 if (!string.IsNullOrEmpty(orderName))
        //                 {
        //                     query = query.Where(p => p.Species != null && p.Species.OrderName == orderName);
        //                 }



        //                 var total = await query.CountAsync();
        //                 var data = await query
        //                     .OrderBy(p => p.CommonName)
        //                     .Skip((page - 1) * pageSize)
        //                     .Take(pageSize)
        //                     .ToListAsync();

        //                 var dtoList = _mapper.Map<List<PlantListDTO>>(data);


        //                 var result = new PagedResult<PlantListDTO>
        //                 {
        //                     Items = dtoList,
        //                     CurrentPage = page,
        //                     PageSize = pageSize,
        //                     TotalItems = total,
        //                     TotalPages = (int)Math.Ceiling((double)total / pageSize)
        //                 };
        //                 return ServiceResult<PagedResult<PlantListDTO>>.Ok(result);
        //             }
        //             catch (Exception ex)
        //             {
        //                 return ServiceResult<PagedResult<PlantListDTO>>.Fail($"Lỗi khi lấy dữ liệu: {ex.Message}");
        //             }
        //         }


        public async Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(
                    string? keyword,
                    int page,
                    int pageSize,
                    int? categoryId,
                    string? orderName)
        {
            try
            {
                // --- 1. Định nghĩa bộ lọc (filter)
                Expression<Func<Plant, bool>> filter = p =>
                    (string.IsNullOrEmpty(keyword)
                        || p.CommonName.ToLower().Contains(keyword.ToLower().Trim())
                        || (p.Species != null && p.Species.ScientificName.ToLower().Contains(keyword.ToLower().Trim())))
                    && (!categoryId.HasValue || p.Categories.Any(c => c.CategoryId == categoryId.Value))
                    && (string.IsNullOrEmpty(orderName) || (p.Species != null && p.Species.OrderName == orderName));

                // --- 2. Định nghĩa include (nạp navigation properties)
                Func<IQueryable<Plant>, IQueryable<Plant>> include = q =>
                    q.Include(p => p.Species)
                     .Include(p => p.Categories)
                     .Include(p => p.PlantImages);

                // --- 3. Gọi repository generic
                var pagedResult = await _plantRepo.GetPagedAsync(
                    filter: filter,
                    include: include,
                    orderBy: q => q.OrderBy(p => p.CommonName),
                    page: page,
                    pageSize: pageSize,
                    selector: p => _mapper.Map<PlantListDTO>(p)
                );

                // --- 4. Trả kết quả ServiceResult
                return ServiceResult<PagedResult<PlantListDTO>>.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResult<PlantListDTO>>.Fail($"Lỗi khi lấy dữ liệu: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PagedResult<PlantListDTO>>> GetPagedAsync(
    string? keyword,
    int page,
    int pageSize,
    List<int>? categoryIds,
    List<int>? useIds,
    List<int>? diseaseIds,
    string? orderName,
    bool? isFavorite,
    int? userId
)
        {
            try
            {
                var query = _plantRepo.Query()
                    .Include(p => p.Species)
                    .Include(p => p.Categories)
                    .Include(p => p.PlantImages)
                    .Include(p => p.Favorites)
                    .AsQueryable();

                query = query.Where(p => p.IsActive == true);


                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    string normalized = TextHelper.NormalizeKeyword(keyword);
                    query = query.Where(p =>
    EF.Functions.ILike(p.CommonName, $"%{normalized}%") ||   // keyword nằm trong tên cây
    (p.Species != null && (
        EF.Functions.ILike(p.Species.ScientificName, $"%{normalized}%")
    ))
);

                }

                if (categoryIds?.Any() == true)
                    query = query.Where(p => categoryIds.All(id => p.Categories.Any(c => c.CategoryId == id)));

                if (useIds?.Any() == true)
                    query = query.Where(p => useIds.All(id => p.Uses.Any(u => u.UseId == id)));

                if (diseaseIds?.Any() == true)
                    query = query.Where(p => p.Diseases.Any(d => diseaseIds.Contains(d.DiseaseId)));

                if (!string.IsNullOrEmpty(orderName))
                {
                    query = query.Where(p => p.Species != null && p.Species.OrderName == orderName);
                }
                if (isFavorite == true && userId.HasValue)
                {
                    query = query.Where(p => p.Favorites.Any(f => f.UserId == userId.Value));
                }

                int totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(p => p.CommonName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => _mapper.Map<PlantListDTO>(p))
                    .ToListAsync();

                return ServiceResult<PagedResult<PlantListDTO>>.Ok(new PagedResult<PlantListDTO>
                {
                    Items = items,
                    TotalItems = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize,

                });
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

                var plantExist = await _plantRepo.FindAsync(p => p.CommonName == model.CommonName);
                if (plantExist.Any())
                {
                    return ServiceResult<PlantDTO>.Fail("Cây này đã tồn tại");
                }
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

                if (model.GrowthCondition != null)
                {
                    var gc = _mapper.Map<GrowthCondition>(model.GrowthCondition);
                    gc.PlantId = plant.PlantId;
                    await _growthRepo.AddAsync(gc);
                }
                if (model.DiseaseIds != null)
                {
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

                            var existDis = await _diseaseRepo.FindAsync(ds => ds.DiseaseName == diseaseDto.DiseaseName);
                            if (existDis.Any()) continue; // Nếu đã có thì bỏ qua

                            var disease = _mapper.Map<Disease>(diseaseDto);
                            await _diseaseRepo.AddAsync(disease);
                            await _diseaseRepo.SaveChangesAsync();
                            plant.Diseases.Add(disease);
                        }
                    }
                }
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
                            var existCat = await _categoryRepo.FindAsync(c => c.CategoryName == catDto.CategoryName);
                            if (existCat.Any()) continue; // Có rồi thì bỏ qua

                            var cat = _mapper.Map<Category>(catDto);
                            await _categoryRepo.AddAsync(cat);
                            await _categoryRepo.SaveChangesAsync();
                            plant.Categories.Add(cat);
                        }
                    }
                }

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

                            var existUse = await _useRepo.FindAsync(u => u.UseName == useDto.UseName);
                            if (existUse.Any()) continue; // Có rồi thì bỏ qua

                            var use = _mapper.Map<Use>(useDto);
                            await _useRepo.AddAsync(use);
                            await _useRepo.SaveChangesAsync();
                            plant.Uses.Add(use);
                        }
                    }
                }

                if (model.Images != null)
                {
                    foreach (var img in model.Images)
                    {
                        var entity = _mapper.Map<PlantImage>(img);
                        entity.PlantId = plant.PlantId;
                        await _imageRepo.AddAsync(entity);
                    }
                }

                if (model.References != null)
                {
                    foreach (var r in model.References)
                    {
                        var entity = _mapper.Map<PlantReference>(r);
                        entity.PlantId = plant.PlantId;
                        await _referenceRepo.AddAsync(entity);
                    }
                }
                await _plantRepo.SaveChangesAsync();
                await _searchLogService.RemoveLogsForKeywordAsync(model.CommonName);

                await transaction.CommitAsync();
                return ServiceResult<PlantDTO>.Ok(
                    _mapper.Map<PlantDTO>(plant),
                    "Thêm cây thành công"
                );

            }
            catch (Exception ex)
            {
                return ServiceResult<PlantDTO>.Fail($"Lỗi khi thêm cây: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PlantDTO>> UpdatePlantAsync(PlantUpdateDTO dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
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

                if (!string.IsNullOrWhiteSpace(dto.CommonName))
                    plant.CommonName = dto.CommonName;

                if (!string.IsNullOrWhiteSpace(dto.Origin))
                    plant.Origin = dto.Origin;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    plant.Description = dto.Description;

                if (dto.SpeciesId.HasValue)
                    plant.SpeciesId = dto.SpeciesId.Value;

                plant.UpdateAt = DateTime.Now;

                if (dto.GrowthCondition != null)
                {
                    if (plant.GrowthCondition != null)
                    {
                        _mapper.Map(dto.GrowthCondition, plant.GrowthCondition);
                    }
                    else
                    {
                        var gc = _mapper.Map<GrowthCondition>(dto.GrowthCondition);
                        gc.PlantId = plant.PlantId;
                        await _dbContext.GrowthConditions.AddAsync(gc);
                    }
                }

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
                            var existDis = await _diseaseRepo.FindAsync(ds => ds.DiseaseName == diseaseDto.DiseaseName);
                            if (existDis.Any()) continue; // Nếu đã có thì bỏ qua

                            var disease = _mapper.Map<Disease>(diseaseDto);
                            await _diseaseRepo.AddAsync(disease);
                            await _diseaseRepo.SaveChangesAsync();
                            plant.Diseases.Add(disease);
                        }
                    }
                }

                if (dto.Images != null)
                {
                    var dbImages = plant.PlantImages.ToList();
                    var dtoImageIds = dto.Images.Where(x => x.ImageId.HasValue).Select(x => x.ImageId.Value).ToList();

                    foreach (var dbImg in dbImages)
                    {
                        if (!dtoImageIds.Contains(dbImg.ImageId))
                        {
                            _dbContext.PlantImages.Remove(dbImg);
                        }
                    }

                    foreach (var imgDto in dto.Images)
                    {
                        if (imgDto.ImageId.HasValue && imgDto.ImageId.Value > 0)
                        {
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
                            var newImg = _mapper.Map<PlantImage>(imgDto);
                            newImg.PlantId = plant.PlantId;
                            await _dbContext.PlantImages.AddAsync(newImg);
                        }
                    }
                }

                if (dto.References != null)
                {
                    var dbRefs = plant.PlantReferences.ToList();
                    var dtoRefIds = dto.References.Where(x => x.ReferenceId.HasValue).Select(x => x.ReferenceId.Value).ToList();

                    foreach (var dbRef in dbRefs)
                    {
                        if (!dtoRefIds.Contains(dbRef.ReferenceId))
                        {
                            _dbContext.PlantReferences.Remove(dbRef);
                        }
                    }

                    foreach (var refDto in dto.References)
                    {
                        if (refDto.ReferenceId.HasValue && refDto.ReferenceId.Value > 0)
                        {
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
                            var newRef = _mapper.Map<PlantReference>(refDto);
                            newRef.PlantId = plant.PlantId;
                            await _dbContext.PlantReferences.AddAsync(newRef);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

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
                return ServiceResult<bool>.Ok(plant.IsActive ?? false, msg);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail($"Error deleting plant: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<PlantDetailDTO>>> GetAllPlantAsync()
        {
            var plants = await _plantRepo.Query()
                .Include(p => p.Species)
                .Include(p => p.Categories)
                .Include(p => p.Uses)
                .Include(p => p.Diseases)
                .Include(p => p.GrowthCondition)
                .Include(p => p.PlantImages)
                .Include(p => p.PlantReferences)
                .ToListAsync();

            if (plants == null || plants.Count == 0)
            {
                return ServiceResult<IEnumerable<PlantDetailDTO>>.Fail("Have No Plant");
            }

            var plantDtos = _mapper.Map<List<PlantDetailDTO>>(plants);
            return ServiceResult<IEnumerable<PlantDetailDTO>>.Ok(plantDtos);
        }

    }
}