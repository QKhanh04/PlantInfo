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
    public class DiseaseService : IDiseaseService
    {
        private readonly IDiseaseRepository _repo;
        public DiseaseService(IDiseaseRepository repo)
        {
            _repo = repo;
        }
        public async Task<ServiceResult<Disease>> CreateAsync(Disease disease)
        {
            try
            {
                await _repo.AddAsync(disease);
                await _repo.SaveChangesAsync();
                return ServiceResult<Disease>.Ok(disease, "Disease created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Error creating disease: {ex.Message}");
            }
        }
    }
}