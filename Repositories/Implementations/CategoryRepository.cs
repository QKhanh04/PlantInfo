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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly PlantDbContext _context;
        public CategoryRepository(PlantDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Plant>> GetPlantsByCategoryIdAsync(int categoryId)
        {
            return await _context.Plants
                .Where(p => p.Categories.Any(pc => pc.CategoryId == categoryId))
                .Include(p => p.Species)
                .ToListAsync();
        }
        public async Task<Category?> FindByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => EF.Functions.ILike(c.CategoryName, $"%{name}%"));
        }
    }
}