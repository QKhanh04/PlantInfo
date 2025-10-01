using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class CategoryStatDto
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class FavoriteStatDto
    {
        public string Name { get; set; }
        public int Favorites { get; set; }
    }

    public class SearchTrendDto
    {
        public DateTime Date { get; set; }   // Ngày thống kê
        public int Searches { get; set; }    // Số lượt tìm kiếm
    }
    
    public class NewPlantDto
    {
        public DateTime Date { get; set; }   // Ngày thêm cây
        public int Plants { get; set; }
    }

    public class KeywordStatDto
    {
        public string Keyword { get; set; }
        public int Count { get; set; }
    }

    public class UserActivityDto
    {
        public List<DateTime> Dates { get; set; } = new();   // Mốc thời gian
        public List<int> NewUsers { get; set; } = new();     // Người dùng mới theo ngày
        public List<int> ActiveUsers { get; set; } = new();  // Người dùng active theo ngày
    }

    public class ReportDataDto
    {
        public List<CategoryStatDto> Categories { get; set; } = new();
        public List<FavoriteStatDto> TopFavorites { get; set; } = new();
        public List<SearchTrendDto> SearchTrends { get; set; } = new();
        public List<NewPlantDto> NewPlants { get; set; } = new();
        public List<KeywordStatDto> TopKeywords { get; set; } = new();
        public UserActivityDto UserActivity { get; set; } = new();
    }
}