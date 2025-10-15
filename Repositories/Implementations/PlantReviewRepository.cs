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
    public class PlantReviewRepository : GenericRepository<PlantReview>, IPlantReviewRepository
    {
        private readonly PlantDbContext _context;
        public PlantReviewRepository(PlantDbContext context) : base(context)
        {
            _context = context;
        }
 
        public async Task AddOrUpdateReviewAsync(PlantReview review)
        {
            var existing = await GetUserReviewAsync(review.PlantId, review.UserId);
            if (existing != null)
            {
                existing.Comment = review.Comment;
                existing.Rating = review.Rating;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                review.CreatedAt = DateTime.Now;
                review.UpdatedAt = DateTime.Now;
                review.IsActive = true;
                _context.PlantReviews.Add(review);
            }
            await _context.SaveChangesAsync();
        }
 
        public Task<List<PlantReview>> GetAllReviewsForAdminAsync(int plantId)
        {
            return _context.PlantReviews
            .Include(p => p.User)
            .Where(p => p.PlantId == plantId).OrderByDescending(p => p.UpdatedAt).ToListAsync();
        }
 
        public async Task<PlantReview> GetUserReviewAsync(int plantId, int userId)
        {
            return await _context.PlantReviews.FirstOrDefaultAsync(r => r.PlantId == plantId && r.UserId == userId);
        }
 
        public Task<List<PlantReview>> GetVisibleReviewsByPlantIdAsync(int plantId)
        {
            return _context.PlantReviews
            .Include(p => p.User)
            .Where(p => p.PlantId == plantId && p.IsActive == true).OrderByDescending(p => p.UpdatedAt).ToListAsync();
 
        }
 
        public async Task ToggleVisibilityAsync(int reviewId, bool IsActive)
        {
            var feedback = await _context.PlantReviews.FindAsync(reviewId);
            if (feedback != null)
            {
                feedback.IsActive = IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}