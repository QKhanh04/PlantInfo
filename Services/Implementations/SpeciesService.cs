using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{

    public class SpeciesService : ISpeciesService
    {
        private readonly ISpeciesRepository _speciesRepository;
        public SpeciesService(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        public async Task<ServiceResult<Species>> CreateAsync(Species species)
        {
            try
            {
                await _speciesRepository.AddAsync(species);
                await _speciesRepository.SaveChangesAsync();
                return ServiceResult<Species>.Ok(species, "Species created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<Species>.Fail($"Error creating species: {ex.Message}");
            }
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


        public async Task<ServiceResult<IEnumerable<Species>>> GetAllSpeciesAsync() {
            var species = await _speciesRepository.GetAllAsync();
            return species == null ? ServiceResult<IEnumerable<Species>>.Fail("Have No Species") : ServiceResult<IEnumerable<Species>>.Ok(species);
        }

    }
}