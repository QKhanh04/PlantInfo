using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Implementations
{
    public class GrowthConditionRepository : GenericRepository<GrowthCondition>, Interfaces.IGrowthConditionRepository
    {
        public GrowthConditionRepository(DbContext context) : base(context)
        {
        }
    }
}