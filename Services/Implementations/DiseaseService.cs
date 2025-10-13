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
        private readonly IDiseaseRepository _diseaseRepository;
        public DiseaseService(IDiseaseRepository diseaseRepository)
        {
            _diseaseRepository = diseaseRepository;
        }
        public async Task<ServiceResult<Disease>> CreateDiseaseAsync(Disease disease)
        {
            try
            {
                await _diseaseRepository.AddAsync(disease);
                await _diseaseRepository.SaveChangesAsync();
                return ServiceResult<Disease>.Ok(disease, "Disease created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Disease>.Fail($"Error creating disease: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<Disease>>> GetAllDiseaseAsync()
        {
            var disease = await _diseaseRepository.GetAllAsync();
            return disease == null ? ServiceResult<IEnumerable<Disease>>.Fail("Have No Disease") : ServiceResult<IEnumerable<Disease>>.Ok(disease);
        }

    }
}