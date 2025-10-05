using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantSummaryDto
    {
        public int TotalPlants { get; set; }
        public int TotalActivePlants { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UserSummaryDto
    {
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; } // Số user mới trong khoảng thời gian lọc
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CategoryStatDto
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int PlantCount { get; set; }
        public double Percentage { get; set; }
    }

    public class FavoriteStatDto
    {
        public int PlantId { get; set; }
        public string? PlantName { get; set; }
        public int FavoriteCount { get; set; }
    }

    public class KeywordStatDto
    {
        public string? Keyword { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}