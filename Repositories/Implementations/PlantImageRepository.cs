using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Implementations
{
    public class PlantImageRepository : GenericRepository<PlantImage>, Interfaces.IPlantImageRepository
    {
        public PlantImageRepository(DbContext context) : base(context)
        {
        }
    }
}