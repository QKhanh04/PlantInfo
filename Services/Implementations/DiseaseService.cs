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
        private readonly IMapper _mapper;

        public DiseaseService(IDiseaseRepository diseaseRepository, IMapper mapper)
        {
            _diseaseRepository = diseaseRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<Disease>> CreateDiseaseAsync(DiseasesDTO dto)
        {
            try
            {
                var exists = await _diseaseRepository.Query()
            .AnyAsync(d => d.DiseaseName.ToLower() == dto.DiseaseName.ToLower());
                if (exists)
                {
                    return ServiceResult<Disease>.Fail("Disease name already exists. Please choose a different name.");
                }
                var disease = _mapper.Map<Disease>(dto);
                await _diseaseRepository.AddAsync(disease);
                await _diseaseRepository.SaveChangesAsync();
                return ServiceResult<Disease>.Ok(disease, "Disease created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Error creating Disease: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Disease>> UpdateDiseaseAsync(DiseasesDTO dto)
        {
            try
            {
                var existing = await _diseaseRepository.GetByIdAsync(dto.DiseaseId);
                if (existing == null)
                {
                    return ServiceResult<Disease>.Fail($"Disease not found");
                }
                _mapper.Map(dto, existing);
                _diseaseRepository.Update(existing);
                await _diseaseRepository.SaveChangesAsync();
                return ServiceResult<Disease>.Ok(existing, "Disease updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Error updating Disease: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Disease>>> GetAllDiseaseAsync()
        {
            var diseases = await _diseaseRepository.GetAllAsync();
            return ServiceResult<IEnumerable<Disease>>.Ok(diseases);
        }

        public async Task<ServiceResult<PagedResult<DiseasesDTO>>> GetPagedDiseasesAsync(string? keyword, int page, int pageSize)
        {
            var query = _diseaseRepository.Query();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string loweredKeyword = keyword.ToLower();
                query = query.Where(d => d.DiseaseName.ToLower().Contains(loweredKeyword));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.DiseaseName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => _mapper.Map<DiseasesDTO>(d))
                .ToListAsync();

            var pagedResult = new PagedResult<DiseasesDTO>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<DiseasesDTO>>.Ok(pagedResult);
        }

        public async Task<ServiceResult<Disease>> GetByIdAsync(int id)
        {
            var disease = await _diseaseRepository.GetByIdAsync(id);
            return disease == null
                ? ServiceResult<Disease>.Fail("Disease not found")
                : ServiceResult<Disease>.Ok(disease);
        }


    }
}