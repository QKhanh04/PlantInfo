using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<ServiceResult<int>> CountAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var count = await _favoriteRepository.CountAsync(startDate, endDate);
                return ServiceResult<int>.Ok(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Fail(ex.Message);
            }
        }
    }
}