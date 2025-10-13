using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class UseService : IUseService
    {
        private readonly IUseRepository _useRepository;
        public UseService(IUseRepository useRepository)
        {
            _useRepository = useRepository;
        }
        public async Task<ServiceResult<Use>> CreateAsync(Use use)
        {
            try
            {
                await _useRepository.AddAsync(use);
                await _useRepository.SaveChangesAsync();
                return ServiceResult<Use>.Ok(use, "Use created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Use>.Fail($"Error creating use: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Use>>> GetAllUsesAsync()
        {
            var uses = await _useRepository.GetAllAsync();
            return uses == null ? ServiceResult<IEnumerable<Use>>.Fail("Have No Use") : ServiceResult<IEnumerable<Use>>.Ok(uses);
        }
    }
}