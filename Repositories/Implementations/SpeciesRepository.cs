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
    }
}