using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Repositories.Implementations
{
    public class SpeciesRepository : GenericRepository<Species>, ISpeciesRepository
    {
        private readonly PlantDbContext _context;
        public SpeciesRepository(PlantDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Species?> FindByScientificNameAsync(string name)
        {
            return await _context.Species.FirstOrDefaultAsync(s => EF.Functions.ILike(s.ScientificName, $"%{name}%"));

        }


        public async Task<List<Species>> SearchAsync(string query, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Species>();
            var q = query.Trim().ToLower();
            return await _context.Species
                .Where(s => EF.Functions.Like(s.ScientificName.ToLower(), $"%{q}%")
                         || EF.Functions.Like(s.Description.ToLower(), $"%{q}%"))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Plant>> GetPlantsBySpeciesIdAsync(int speciesId)
        {
            return await _context.Plants
                .Where(p => p.SpeciesId == speciesId)
                .ToListAsync();
        }

    }
}