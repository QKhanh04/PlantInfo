using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.DTOs;
using PlantManagement.Repositories.Interfaces;

namespace PlantManagement.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly PlantDbContext _context;
        public ReportRepository(PlantDbContext context)
        {
            _context = context;
        }

        // 1. Thống kê theo phân loại
        public async Task<List<CategoryStatDto>> GetCategoryStatsAsync()
        {
            var total = await _context.Plants.CountAsync();

            return await _context.Plants
                .SelectMany(p => p.Categories)
                .GroupBy(c => c.CategoryName)
                .Select(g => new CategoryStatDto
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 1)
                })
                .ToListAsync();
        }

        // 2. Top cây được yêu thích
        public async Task<List<FavoriteStatDto>> GetTopFavoritesAsync()
        {
            return await _context.Favorites
                .GroupBy(f => f.Plant)
                .Select(g => new FavoriteStatDto
                {
                    Name = g.Key.CommonName,
                    Favorites = g.Count()
                })
                .OrderByDescending(x => x.Favorites)
                .Take(10)
                .ToListAsync();
        }

        // 3. Xu hướng tìm kiếm (theo ngày)
        public async Task<List<SearchTrendDto>> GetSearchTrendsAsync()
        {
            return await _context.SearchLogs
                .Where(s => s.SearchDate.HasValue)
                .GroupBy(s => s.SearchDate.Value.Date)
                .Select(g => new SearchTrendDto
                {
                    Date = g.Key,
                    Searches = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        // 4. Cây mới thêm (theo ngày)
        public async Task<List<NewPlantDto>> GetNewPlantsAsync()
        {
            return await _context.Plants
                .GroupBy(p => p.CreateAt.Value.Date)
                .Select(g => new NewPlantDto
                {
                    Date = g.Key,
                    Plants = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        // 5. Top từ khóa tìm kiếm
        public async Task<List<KeywordStatDto>> GetTopKeywordsAsync()
        {
            return await _context.SearchLogs
                .Where(s => !string.IsNullOrEmpty(s.Keyword))
                .GroupBy(s => s.Keyword)
                .Select(g => new KeywordStatDto
                {
                    Keyword = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();
        }

        // 6. Hoạt động người dùng (user mới + active theo ngày)
        public async Task<UserActivityDto> GetUserActivityAsync()
        {
            var userActivity = new UserActivityDto();

            // Người dùng mới
            var newUsers = await _context.Users
                .GroupBy(u => u.CreateAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            userActivity.Dates = newUsers.Select(x => x.Date).ToList();
            userActivity.NewUsers = newUsers.Select(x => x.Count).ToList();

            // Người dùng active (có search log)
            var activeUsers = await _context.SearchLogs
                .Where(s => s.SearchDate.HasValue)
                .GroupBy(s => s.SearchDate.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Select(s => s.UserId).Distinct().Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            userActivity.ActiveUsers = activeUsers.Select(x => x.Count).ToList();

            return userActivity;
        }
    }
}