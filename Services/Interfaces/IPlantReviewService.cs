using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantInformation.DTOs;
using PlantManagement.Common.Results;

namespace PlantManagement.Services.Interfaces
{
    public interface IPlantReviewService
    {
        Task<ServiceResult<List<ReviewDTO>>> GetVisibleReviewsByPlantIdAsync(int plantId);
 
        Task<ServiceResult<ReviewDTO>> GetUserReviewAsync(int plantId, int userId);
 
        Task<ServiceResult<bool>> AddOrUpdateReviewAsync(int userId, CreateReviewDTO dto);
 
        Task<ServiceResult<bool>> UpdateReviewAsync(int UserId, UpdateReviewDTO dto);
 
        Task<ServiceResult<bool>> ToggleVisibilityAsync(int reviewId, bool isActive);
 
        Task<ServiceResult<List<ReviewDTO>>> GetAllReviewsForAdminAsync(int plantId);
 
        Task<ServiceResult<RatingSummaryDTO>> GetRatingSummaryAsync(int plantId);
    }
}