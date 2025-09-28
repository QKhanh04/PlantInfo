using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Implementations
{
    public class DiseaseRepository : GenericRepository<Disease>, Interfaces.IDiseaseRepository
    {
        public DiseaseRepository(DbContext context) : base(context)
        {
        }
    }
}