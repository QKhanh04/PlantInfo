using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Repositories;

namespace PlantManagement.Services
{
    
    public class SpeciesService : ISpeciesService
    {
        private readonly ISpeciesRepository _speciesRepository;
        public SpeciesService(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository;
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
    }
}