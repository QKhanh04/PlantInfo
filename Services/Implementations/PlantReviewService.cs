using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlantInformation.DTOs;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class PlantReviewService : IPlantReviewService
    {
        private readonly IPlantReviewRepository _plantReviewRepository;
        private readonly IMapper _mapper;
        public PlantReviewService(IPlantReviewRepository plantReviewRepository, IMapper mapper)
        {
            _plantReviewRepository = plantReviewRepository;
            _mapper = mapper;
        }
        public async Task<ServiceResult<bool>> AddReviewAsync(int userId, CreateReviewDTO dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return ServiceResult<bool>.Fail("Bạn phải chọn sao đánh giá!");

            var review = _mapper.Map<PlantReview>(dto);
            review.UserId = userId;
            await _plantReviewRepository.AddReviewAsync(review);
            return ServiceResult<bool>.Ok(true, "Thêm review thành công!");
        }

        public async Task<ServiceResult<List<ReviewDTO>>> GetAllReviewsForAdminAsync(int plantId)
        {
            var reviews = await _plantReviewRepository.GetAllReviewsForAdminAsync(plantId);
            var reviewDTOs = reviews.Select(r => _mapper.Map<ReviewDTO>(r)).ToList();
            return ServiceResult<List<ReviewDTO>>.Ok(reviewDTOs);
        }

        public async Task<ServiceResult<ReviewDTO>> GetUserReviewAsync(int plantId, int userId)
        {
            var review = await _plantReviewRepository.GetUserReviewAsync(plantId, userId);
            if (review == null)
                return ServiceResult<ReviewDTO>.Fail("Review không tồn tại!");
            var reviewDTO = _mapper.Map<ReviewDTO>(review);
            return ServiceResult<ReviewDTO>.Ok(reviewDTO);
        }

        public async Task<ServiceResult<List<ReviewDTO>>> GetVisibleReviewsByPlantIdAsync(int plantId)
        {
            var reviews = await _plantReviewRepository.GetVisibleReviewsByPlantIdAsync(plantId);
            var reviewDTOs = reviews.Select(r => _mapper.Map<ReviewDTO>(r)).ToList();
            return ServiceResult<List<ReviewDTO>>.Ok(reviewDTOs);
        }

        public async Task<ServiceResult<bool>> ToggleVisibilityAsync(int reviewId, bool isActive)
        {
            var review = await _plantReviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return ServiceResult<bool>.Fail("Review không tồn tại!");

            await _plantReviewRepository.ToggleVisibilityAsync(reviewId, isActive);
            return ServiceResult<bool>.Ok(true, "Đã cập nhật trạng thái review!");
        }

        public async Task<ServiceResult<bool>> UpdateReviewAsync(int userId, UpdateReviewDTO dto)
        {
            var review = await _plantReviewRepository.GetUserReviewAsync(dto.PlantId, userId);
            if (review == null)
                return ServiceResult<bool>.Fail("Review không tồn tại hoặc user không có quyền sửa!");

            if (dto.Rating.HasValue)
                review.Rating = dto.Rating.Value;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.Now;
            await _plantReviewRepository.UpdateReviewAsync(review);
            return ServiceResult<bool>.Ok(true, "Cập nhật review thành công!");
        }

        public async Task<ServiceResult<RatingSummaryDTO>> GetRatingSummaryAsync(int plantId)
        {
            var reviews = await _plantReviewRepository.GetVisibleReviewsByPlantIdAsync(plantId);
            if (reviews == null || reviews.Count == 0)
                return ServiceResult<RatingSummaryDTO>.Ok(new RatingSummaryDTO
                {
                    AverageRating = 0,
                    TotalReviews = 0,
                    FiveStars = 0,
                    FourStars = 0,
                    ThreeStars = 0,
                    TwoStars = 0,
                    OneStar = 0
                });

            var ratings = reviews.Select(r => r.Rating).ToList();
            double average = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;
            var summary = new RatingSummaryDTO
            {
                AverageRating = average,
                TotalReviews = ratings.Count,
                FiveStars = ratings.Count(r => r == 5),
                FourStars = ratings.Count(r => r == 4),
                ThreeStars = ratings.Count(r => r == 3),
                TwoStars = ratings.Count(r => r == 2),
                OneStar = ratings.Count(r => r == 1),
            };

            return ServiceResult<RatingSummaryDTO>.Ok(summary);
        }
    }
}