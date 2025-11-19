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
    public class DiseaseRepository : GenericRepository<Disease>, IDiseaseRepository
    {
        public DiseaseRepository(PlantDbContext context) : base(context)
        {
        }
        public IQueryable<Disease> GetDiseasesWithPlant()
        {
            return _context.Diseases
                .Include(d => d.Plant)
                    .ThenInclude(p => p.Species);
        }
    }
}