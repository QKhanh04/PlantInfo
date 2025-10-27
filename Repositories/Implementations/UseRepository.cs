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
    public class UseRepository : GenericRepository<Use>, IUseRepository
    {
        public UseRepository(PlantDbContext context) : base(context)
        {
        }
        public async Task<Use?> FindByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(u => EF.Functions.ILike(u.UseName, $"%{name}%"));
        }
    }
}