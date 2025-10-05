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
        public async Task<PlantSummaryDto> GetPlantSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Plants.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(p => p.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.CreateAt <= endDate.Value);

            int totalPlants = await query.CountAsync();
            // int totalActivePlants = await query.CountAsync(p => p.IsActive);

            return new PlantSummaryDto
            {
                TotalPlants = totalPlants,
                // TotalActivePlants = totalActivePlants,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<UserSummaryDto> GetUserSummaryAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Users.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(u => u.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(u => u.CreateAt <= endDate.Value);

            int totalUsers = await query.CountAsync();

            // Số user mới trong khoảng thời gian
            int newUsers = 0;
            if (startDate.HasValue && endDate.HasValue)
            {
                newUsers = await _context.Users
                    .Where(u => u.CreateAt >= startDate.Value && u.CreateAt <= endDate.Value)
                    .CountAsync();
            }

            return new UserSummaryDto
            {
                TotalUsers = totalUsers,
                NewUsers = newUsers,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<List<CategoryStatDto>> GetPlantCountByCategoryAsync(DateTime? startDate, DateTime? endDate)
        {
            // Lấy danh sách cây theo khoảng thời gian
            var plantQuery = _context.Plants.AsQueryable();
            if (startDate.HasValue)
                plantQuery = plantQuery.Where(p => p.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                plantQuery = plantQuery.Where(p => p.CreateAt <= endDate.Value);

            var plantIds = await plantQuery.Select(p => p.PlantId).ToListAsync();
            var total = plantIds.Count;

            var stats = await _context.Categories
                .Select(c => new CategoryStatDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    PlantCount = c.Plants.Count(pc => plantIds.Contains(pc.PlantId)),
                    Percentage = total > 0 ? Math.Round(100.0 * c.Plants.Count(pc => plantIds.Contains(pc.PlantId)) / total, 1) : 0
                })
                .OrderByDescending(c => c.PlantCount)
                .ToListAsync();

            return stats;
        }

        public async Task<List<FavoriteStatDto>> GetTopFavoritePlantsAsync(int topN, DateTime? startDate, DateTime? endDate)
        {
            var favQuery = _context.Favorites.AsQueryable();
            if (startDate.HasValue)
                favQuery = favQuery.Where(f => f.CreateAt >= startDate.Value);
            if (endDate.HasValue)
                favQuery = favQuery.Where(f => f.CreateAt <= endDate.Value);

            var favGroup = await favQuery
                .GroupBy(f => f.PlantId)
                .Select(g => new
                {
                    PlantId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(topN)
                .ToListAsync();

            var plantIds = favGroup.Select(x => x.PlantId).ToList();
            var plants = await _context.Plants
                .Where(p => plantIds.Contains(p.PlantId))
                .ToListAsync();

            return favGroup
                .Select(x => new FavoriteStatDto
                {
                    PlantId = x.PlantId,
                    PlantName = plants.FirstOrDefault(p => p.PlantId == x.PlantId)?.CommonName ?? "",
                    FavoriteCount = x.Count
                })
                .ToList();
        }

        public async Task<List<KeywordStatDto>> GetTopSearchKeywordsAsync(int topN, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.SearchLogs.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(s => s.SearchDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(s => s.SearchDate <= endDate.Value);

            // Tổng số lượt tìm kiếm để tính phần trăm nếu cần
            var total = await query.CountAsync();

            var topKeywords = await query
                .GroupBy(s => s.Keyword)
                .Select(g => new KeywordStatDto
                {
                    Keyword = g.Key,
                    Count = g.Count(),
                    Percentage = total > 0 ? Math.Round(100.0 * g.Count() / total, 1) : 0
                })
                .OrderByDescending(k => k.Count)
                .Take(topN)
                .ToListAsync();

            return topKeywords;
        }
    }
}