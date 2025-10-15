using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Models;

namespace PlantManagement.Repositories.Interfaces
{
    public interface IPlantReviewRepository : IGenericRepository<PlantReview>
    {
        Task<List<PlantReview>> GetVisibleReviewsByPlantIdAsync(int plantId);
        Task<PlantReview> GetUserReviewAsync(int plantId, int userId);
        Task AddReviewAsync(PlantReview review);
        Task UpdateReviewAsync(PlantReview review);
        Task ToggleVisibilityAsync(int id, bool isVisible);
        Task<List<PlantReview>> GetAllReviewsForAdminAsync(int plantId);
    }
}