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
    public class PlantImageRepository : GenericRepository<PlantImage>, IPlantImageRepository
    {
        public PlantImageRepository(PlantDbContext context) : base(context)
        {
        }
    }
}