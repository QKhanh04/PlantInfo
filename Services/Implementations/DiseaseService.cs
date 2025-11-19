using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly IMapper _mapper;

        public DiseaseService(IDiseaseRepository diseaseRepository, IPlantRepository plantRepository, IMapper mapper)
        {
            _diseaseRepository = diseaseRepository;
            _plantRepository = plantRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<Disease>> CreateDiseaseAsync(DiseaseDTO dto)
        {
            try
            {
                // Kiểm tra plant có tồn tại không
                var plant = await _plantRepository.GetByIdAsync(dto.PlantId);
                if (plant == null)
                {
                    return ServiceResult<Disease>.Fail("Không tìm thấy cây trồng.");
                }

                var disease = new Disease
                {
                    PlantId = dto.PlantId,
                    DiseaseName = dto.DiseaseName,
                    Symptoms = dto.Symptoms,
                    Treatment = dto.Treatment
                };

                await _diseaseRepository.AddAsync(disease);
                await _diseaseRepository.SaveChangesAsync();
                
                return ServiceResult<Disease>.Ok(disease, "Tạo sâu bệnh thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Lỗi khi tạo sâu bệnh: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Disease>> UpdateDiseaseAsync(DiseaseDTO dto)
        {
            try
            {
                var existing = await _diseaseRepository.GetByIdAsync(dto.DiseaseId);
                if (existing == null)
                {
                    return ServiceResult<Disease>.Fail("Không tìm thấy sâu bệnh");
                }

                // Kiểm tra plant có tồn tại không
                var plant = await _plantRepository.GetByIdAsync(dto.PlantId);
                if (plant == null)
                {
                    return ServiceResult<Disease>.Fail("Không tìm thấy cây trồng.");
                }

                existing.PlantId = dto.PlantId;
                existing.DiseaseName = dto.DiseaseName;
                existing.Symptoms = dto.Symptoms;
                existing.Treatment = dto.Treatment;

                _diseaseRepository.Update(existing);
                await _diseaseRepository.SaveChangesAsync();
                
                return ServiceResult<Disease>.Ok(existing, "Cập nhật sâu bệnh thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Lỗi khi cập nhật sâu bệnh: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Disease>>> GetAllDiseasesAsync()
        {
            var diseases = await _diseaseRepository.GetAllAsync();
            return ServiceResult<IEnumerable<Disease>>.Ok(diseases);
        }

        public async Task<ServiceResult<Disease>> GetByIdAsync(int id)
        {
            var disease = await _diseaseRepository.GetDiseasesWithPlant()
                .FirstOrDefaultAsync(d => d.DiseaseId == id);
            
            return disease == null
                ? ServiceResult<Disease>.Fail("Không tìm thấy sâu bệnh")
                : ServiceResult<Disease>.Ok(disease);
        }

        public async Task<ServiceResult<PagedResult<DiseaseDTO>>> GetPagedDiseasesAsync(string? keyword, int? plantId, int page, int pageSize)
        {
            var query = _diseaseRepository.GetDiseasesWithPlant();
            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string loweredKeyword = keyword.ToLower();
                query = query.Where(d => d.DiseaseName.ToLower().Contains(loweredKeyword) 
                                      || (d.Symptoms != null && d.Symptoms.ToLower().Contains(loweredKeyword))
                                      || (d.Plant.CommonName != null && d.Plant.CommonName.ToLower().Contains(loweredKeyword))
                                      || (d.Plant.Species != null && d.Plant.Species.ScientificName.ToLower().Contains(loweredKeyword)));
            }

            if (plantId.HasValue && plantId.Value > 0)
            {
                query = query.Where(d => d.PlantId == plantId.Value);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.DiseaseName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DiseaseDTO
                {
                    DiseaseId = d.DiseaseId,
                    PlantId = d.PlantId,
                    DiseaseName = d.DiseaseName,
                    Symptoms = d.Symptoms,
                    Treatment = d.Treatment,
                    PlantName = d.Plant.CommonName,
                    // PlantScientificName = d.Plant.Species != null ? d.Plant.Species.ScientificName : null
                })
                .ToListAsync();

            var pagedResult = new PagedResult<DiseaseDTO>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<DiseaseDTO>>.Ok(pagedResult);
        }

        public async Task<ServiceResult<bool>> DeleteDiseaseAsync(int diseaseId)
        {
            var disease = await _diseaseRepository.GetByIdAsync(diseaseId);
            if (disease == null)
                return ServiceResult<bool>.Fail("Không tìm thấy sâu bệnh.");

            _diseaseRepository.Delete(disease);
            await _diseaseRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Xóa sâu bệnh thành công.");
        }

        public async Task<ServiceResult<IEnumerable<PlantDTO>>> GetAllPlantsForDropdownAsync()
        {
            var plants = await _plantRepository.Query()
                .Include(p => p.Species)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.CommonName)
                .Select(p => new PlantDTO
                {
                    PlantId = p.PlantId,
                    CommonName = p.CommonName,
                    // ScientificName = p.Species != null ? p.Species.ScientificName : null
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<PlantDTO>>.Ok(plants);
        }
    }
}