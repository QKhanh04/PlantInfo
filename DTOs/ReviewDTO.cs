using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
namespace PlantInformation.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int PlantId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string UserName { get; set; }
 
    }
    public class CreateReviewDTO
    {
        public int PlantId { get; set; }
        // public int UserId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
    }
    public class UpdateReviewDTO
    {
        public int ReviewId { get; set; }
        // public int UserId { get; set; }
        public string Comment { get; set; }
        public int? Rating { get; set; }
    }
 
    public class RatingSummaryDTO
    {
        public double AverageRating { get; set; }      
        public int TotalReviews { get; set; }        
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
    }
 
}