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
    public class PlantReferenceService : IPlantReferenceService
    {
        private readonly IPlantReferenceRepository _repo;
        public PlantReferenceService(IPlantReferenceRepository repo)
        {
            _repo = repo;
        }
        public async Task<ServiceResult<PlantReference>> CreateAsync(PlantReference reference)
        {
            try
            {
                await _repo.AddAsync(reference);
                await _repo.SaveChangesAsync();
                return ServiceResult<PlantReference>.Ok(reference, "Reference created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<PlantReference>.Fail($"Error creating reference: {ex.Message}");
            }
        }

    }
}