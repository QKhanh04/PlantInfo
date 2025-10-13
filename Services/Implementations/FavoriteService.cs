using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IMapper _mapper;
        public FavoriteService(IFavoriteRepository favoriteRepository, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _mapper = mapper;
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

        public async Task<bool> AddFavoriteAsync(int userId, int plantId)
        {
            if (await IsFavoriteAsync(userId, plantId))
                return false;

            var favorite = new Favorite
            {
                UserId = userId,
                PlantId = plantId,
                CreateAt = DateTime.Now
            };
            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();
            return true;
        }

        // Bỏ yêu thích
        public async Task<bool> RemoveFavoriteAsync(int userId, int plantId)
        {
            var favorite = await _favoriteRepository.GetFavoriteAsync(userId, plantId);
            if (favorite == null)
                return false;
            _favoriteRepository.Delete(favorite);
            await _favoriteRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavoriteAsync(int userId, int plantId)
        {
            return await _favoriteRepository.IsFavoriteAsync(userId, plantId);
        }

        public async Task<ServiceResult<List<PlantListDTO>>> GetFavoritePlantsAsync(int userId)
        {
            try
            {
                var plants = await _favoriteRepository.GetFavoritePlantsAsync(userId);

                if (plants == null)
                    return ServiceResult<List<PlantListDTO>>.Fail("Have No Favorite Plant");
                var plantDtos = _mapper.Map<List<PlantListDTO>>(plants);

                return ServiceResult<List<PlantListDTO>>.Ok(plantDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<PlantListDTO>>.Fail(ex.Message);
            }

        }
    }
}