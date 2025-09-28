using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Implementations
{
    public class PlantReferenceRepository : GenericRepository<PlantReference>, Interfaces.IPlantReferenceRepository
    {
        public PlantReferenceRepository(DbContext context) : base(context)
        {
        }
    }
}