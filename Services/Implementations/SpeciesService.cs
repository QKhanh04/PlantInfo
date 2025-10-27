using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

    public class SpeciesService : ISpeciesService
    {
        private readonly ISpeciesRepository _speciesRepository;
        private readonly IMapper _mapper;
        public SpeciesService(ISpeciesRepository speciesRepository, IMapper mapper)
        {
            _speciesRepository = speciesRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<Species>> CreateSpeciesAsync(SpeciesDTO dto)
        {
            try
            {
                var exists = await _speciesRepository.Query()
            .AnyAsync(s => s.ScientificName.ToLower() == dto.ScientificName.ToLower());
                if (exists)
                {
                    return ServiceResult<Species>.Fail("Scientific name already exists. Please choose a different name.");
                }

        

                var species = _mapper.Map<Species>(dto);
                await _speciesRepository.AddAsync(species);
                await _speciesRepository.SaveChangesAsync();
                return ServiceResult<Species>.Ok(species, "Thêm loài thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Species>.Fail($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Species>> UpdateSpeciesAsync(SpeciesDTO dto)
        {
            try
            {
                var existing = await _speciesRepository.GetByIdAsync(dto.SpeciesId);
                if (existing == null)
                    return ServiceResult<Species>.Fail("Không tìm thấy loài");

                _mapper.Map(dto, existing);
                _speciesRepository.Update(existing);
                await _speciesRepository.SaveChangesAsync();
                return ServiceResult<Species>.Ok(existing, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Species>.Fail($"Lỗi: {ex.Message} - {ex.StackTrace}");
            }
        }

        public async Task<ServiceResult<PagedResult<SpeciesDTO>>> GetPagedSpeciesAsync(string? keyword, int page, int pageSize)
        {
            var query = _speciesRepository.Query();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string loweredKeyword = keyword.ToLower();
                query = query.Where(s => s.ScientificName.ToLower().Contains(loweredKeyword));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.ScientificName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SpeciesDTO
                {
                    SpeciesId = s.SpeciesId,
                    ScientificName = s.ScientificName,
                    Genus = s.Genus,
                    Family = s.Family,
                    OrderName = s.OrderName,
                    Description = s.Description
                })
                .ToListAsync();

            var pagedResult = new PagedResult<SpeciesDTO>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<SpeciesDTO>>.Ok(pagedResult);
        }

        public async Task<ServiceResult<Species>> GetByIdAsync(int id)
        {
            var species = await _speciesRepository.GetByIdAsync(id);
            return species == null
                ? ServiceResult<Species>.Fail("Không tìm thấy loài")
                : ServiceResult<Species>.Ok(species);
        }

        public async Task<List<string>> GetDistinctOrderNameAsync()
        {
            var allSpecies = await _speciesRepository.GetAllAsync();
            return allSpecies
                .Select(s => s.OrderName)
                .Where(o => !string.IsNullOrEmpty(o))
                .Distinct()
                .OrderBy(o => o)
                .ToList();
        }


        public async Task<ServiceResult<IEnumerable<Species>>> GetAllSpeciesAsync()
        {
            var species = await _speciesRepository.GetAllAsync();
            return species == null ? ServiceResult<IEnumerable<Species>>.Fail("Have No Species") : ServiceResult<IEnumerable<Species>>.Ok(species);
        }

    }
}