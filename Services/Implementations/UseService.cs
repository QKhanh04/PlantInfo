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
    public class UseService : IUseService
    {
        private readonly IUseRepository _useRepository;
        private readonly IMapper _mapper;

        public UseService(IUseRepository useRepository, IMapper mapper)
        {
            _useRepository = useRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<Use>> CreateUseAsync(UseDTO dto)
        {
            try
            {
                var exists = await _useRepository.Query()
                    .AnyAsync(u => u.UseName.ToLower() == dto.UseName.ToLower());
                
                if (exists)
                {
                    return ServiceResult<Use>.Fail("Tên công dụng đã tồn tại. Vui lòng chọn tên khác.");
                }

                var use = _mapper.Map<Use>(dto);
                await _useRepository.AddAsync(use);
                await _useRepository.SaveChangesAsync();
                
                return ServiceResult<Use>.Ok(use, "Tạo công dụng thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Use>.Fail($"Lỗi khi tạo công dụng: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Use>> UpdateUseAsync(UseDTO dto)
        {
            try
            {
                var existing = await _useRepository.GetByIdAsync(dto.UseId);
                if (existing == null)
                {
                    return ServiceResult<Use>.Fail("Không tìm thấy công dụng");
                }

                // Kiểm tra trùng tên (trừ chính nó)
                var duplicate = await _useRepository.Query()
                    .AnyAsync(u => u.UseName.ToLower() == dto.UseName.ToLower() 
                                && u.UseId != dto.UseId);
                
                if (duplicate)
                {
                    return ServiceResult<Use>.Fail("Tên công dụng đã tồn tại. Vui lòng chọn tên khác.");
                }

                _mapper.Map(dto, existing);
                _useRepository.Update(existing);
                await _useRepository.SaveChangesAsync();
                
                return ServiceResult<Use>.Ok(existing, "Cập nhật công dụng thành công");
            }
            catch (Exception ex)
            {
                return ServiceResult<Use>.Fail($"Lỗi khi cập nhật công dụng: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Use>>> GetAllUsesAsync()
        {
            var uses = await _useRepository.GetAllAsync();
            return ServiceResult<IEnumerable<Use>>.Ok(uses);
        }

        public async Task<ServiceResult<Use>> GetByIdAsync(int id)
        {
            var use = await _useRepository.GetByIdAsync(id);
            return use == null
                ? ServiceResult<Use>.Fail("Không tìm thấy công dụng")
                : ServiceResult<Use>.Ok(use);
        }

        public async Task<ServiceResult<PagedResult<UseDTO>>> GetPagedUsesAsync(string? keyword, int page, int pageSize)
        {
            var query = _useRepository.Query();
            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string loweredKeyword = keyword.ToLower();
                query = query.Where(u => u.UseName.ToLower().Contains(loweredKeyword) 
                                      || (u.Description != null && u.Description.ToLower().Contains(loweredKeyword)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.UseName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => _mapper.Map<UseDTO>(u))
                .ToListAsync();

            var pagedResult = new PagedResult<UseDTO>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<UseDTO>>.Ok(pagedResult);
        }

        public async Task<List<Plant>> GetPlantsByUseIdAsync(int useId)
        {
            return await _useRepository.GetPlantsByUseIdAsync(useId);
        }

        public async Task<ServiceResult<bool>> DeleteUseAsync(int useId)
        {
            var plants = await _useRepository.GetPlantsByUseIdAsync(useId);

            if (plants.Any())
            {
                string plantList = string.Join(", ", plants.Select(p => p.CommonName ?? p.Species?.ScientificName ?? "Không rõ"));
                return ServiceResult<bool>.Fail(
                    $"Không thể xóa vì công dụng đang được dùng bởi {plants.Count} cây: {plantList}"
                );
            }

            var use = await _useRepository.GetByIdAsync(useId);
            if (use == null)
                return ServiceResult<bool>.Fail("Không tìm thấy công dụng.");

            _useRepository.Delete(use);
            await _useRepository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Xóa công dụng thành công.");
        }
    }
}