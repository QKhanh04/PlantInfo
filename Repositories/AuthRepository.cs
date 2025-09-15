using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Models;

namespace PlantManagement.Repositories
{
    public class AuthRepository : GenericRepository<User>, IAuthRepository
    {
        private readonly GenericRepository<User> _genericRepository;
        private readonly PlantDbContext _context;

        public AuthRepository(DbContext context) : base(context)
        {
            _context = (PlantDbContext)context;
            _genericRepository = new GenericRepository<User>(context);
        }

        public async Task<User?> GetUserByUsernameOrEmail(string usernameOrEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        }
    }
}