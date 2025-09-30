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
    public class GrowthConditionService : IGrowthConditionService
    {
        private readonly IGrowthConditionRepository _repo;
        public GrowthConditionService(IGrowthConditionRepository repo)
        {
            _repo = repo;
        }
        public async Task<ServiceResult<GrowthCondition>> CreateAsync(GrowthCondition gc)
        {
            try
            {
                await _repo.AddAsync(gc);
                await _repo.SaveChangesAsync();
                return ServiceResult<GrowthCondition>.Ok(gc, "Growth condition created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<GrowthCondition>.Fail($"Error creating growth condition: {ex.Message}");
            }
        }
        public async Task<IEnumerable<GrowthCondition>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}