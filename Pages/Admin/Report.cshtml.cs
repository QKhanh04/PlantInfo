using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlantManagement.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken]

    public class ReportModel : PageModel
    {
        private readonly ILogger<ReportModel> _logger;

        private readonly IReportService _reportService;
        private readonly IPlantService _plantService;
        private readonly IFavoriteService _favoriteService;
        private readonly IViewLogService _viewLogService;

        public PlantSummaryDto PlantSummary { get; set; } = new();
        public UserSummaryDto UserSummary { get; set; } = new();
        public List<CategoryStatDto> CategoryStats { get; set; } = new();
        public List<FavoriteStatDto> TopFavorites { get; set; } = new();
        public List<KeywordStatDto> TopKeywords { get; set; } = new();
        public List<PlantMonthlyStatDto> PlantMonthly { get; set; } = new();
        public List<UserMonthlyStatDto> UserMonthly { get; set; } = new();
        public List<PlantViewStatDto> TopPlantView { get; set; } = new();

        public ServiceResult<int> FavoriteTotal { get; set; } = new();
        public ServiceResult<int> TopViewTotal { get; set; } = new();
        private const int top = 10;



        public ReportModel(IReportService reportService, IFavoriteService favoriteService, IPlantService plantService, IViewLogService viewLogService, ILogger<ReportModel> logger)
        {
            _reportService = reportService;
            _plantService = plantService;
            _favoriteService = favoriteService;
            _viewLogService = viewLogService;
            _logger = logger;
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate, int? year)
        {
            PlantSummary = await _reportService.GetPlantSummaryAsync(startDate, endDate);
            UserSummary = await _reportService.GetUserSummaryAsync(startDate, endDate);
            CategoryStats = await _reportService.GetPlantCountByCategoryAsync(startDate, endDate);
            TopFavorites = await _reportService.GetTopFavoritePlantsAsync(top, startDate, endDate);
            TopKeywords = await _reportService.GetTopSearchKeywordsAsync(top, startDate, endDate);
            FavoriteTotal = await _favoriteService.CountAsync(startDate, endDate);
            int selectedYear = year ?? DateTime.Now.Year;
            PlantMonthly = await _reportService.GetMonthlyNewPlantStatsAsync(selectedYear);
            UserMonthly = await _reportService.GetMonthlyNewUserStatsAsync(selectedYear);
            TopPlantView = await _reportService.GetTopViewedPlantsAsync(top, startDate, endDate);
            TopViewTotal = await _viewLogService.CountPlantViewsAsync(startDate, endDate);
        }


        private byte[] GenerateTableReport(string title, string[] headers, List<string[]> rows, byte[]? chartBytes = null)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 🧾 Cấu hình trang
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                    // 🌿 Header
                    page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeColumn().AlignLeft().Column(col =>
                        {
                            col.Item().Text("Plant Management System")
                                .Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                            col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy"))
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        // row.ConstantColumn(60)
                        //     .AlignRight()
                        //     .Height(40)
                        //     .Width(60)
                        //     .Image("wwwroot/images/logo.png", ImageScaling.FitArea); // có thể bỏ nếu không có logo
                    });

                    // 🏷️ Tiêu đề chính
                    page.Content().PaddingVertical(25).Column(col =>
                    {
                        col.Item().AlignCenter()
                            .Text(title)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().PaddingVertical(15);


                        // 📊 Bảng dữ liệu
                        col.Item().Table(table =>
{
    // Cấu trúc cột
    table.ColumnsDefinition(cols =>
    {
        foreach (var _ in headers)
            cols.RelativeColumn();
    });

    // Header bảng (căn giữa hoàn toàn)
    table.Header(header =>
    {
        foreach (var head in headers)
        {
            header.Cell()
                .Background(Colors.Green.Darken1)
                .Border(0.5f)
                .BorderColor(Colors.White)
                .PaddingVertical(8)
                .PaddingHorizontal(5)
                .AlignMiddle()
                .AlignCenter()  // 🔹 căn giữa ngang
                .Text(head)
                .Bold()
                .FontColor(Colors.White);
        }
    });

    // Dòng dữ liệu (căn giữa)
    int index = 0;
    foreach (var rowData in rows)
    {
        var bgColor = (index++ % 2 == 0)
            ? Colors.Grey.Lighten4
            : Colors.White;

        foreach (var cell in rowData)
        {
            table.Cell()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(bgColor)
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .AlignMiddle()   // 🔹 căn giữa dọc
                .AlignCenter()   // 🔹 căn giữa ngang
                .Text(cell ?? string.Empty)
                .FontSize(10);
        }
    }
});


                        // 📌 Ghi chú hoặc tổng kết (tùy chọn)
                        if (rows.Count > 0)
                        {
                            col.Item().PaddingTop(15).AlignRight()
                                .Text($"Tổng số dòng: {rows.Count}")
                                .Italic()
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        }

                        // 🌿 Nếu có ảnh chart

                        // Biểu đồ
                        if (chartBytes != null && chartBytes.Length > 0)
                        {
                            col.Item()
                                .AlignCenter()
                                .PaddingBottom(20)
                                .Element(c => 
                                    c.Height(400)
                                    .Width(400)
                                    .Image(chartBytes)
                                    .FitArea()
                                );
                        }
                        else
                        {
                            col.Item().AlignCenter()
                                .PaddingBottom(20)

                                .Text("(Không có biểu đồ hiển thị)")
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                        }
                        _logger.LogInformation("chartBytes: {Length}", chartBytes?.Length ?? 0);
                    });



                    // 📄 Footer
                    page.Footer()
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .PaddingTop(8)
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Trang ").FontColor(Colors.Grey.Darken1);
                            txt.CurrentPageNumber();
                            txt.Span(" / ");
                            txt.TotalPages();
                        });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }



        public async Task<IActionResult> OnPostExportCategoryReportWithChartAsync(DateTime? startDate, DateTime? endDate)
        {
            var categoryStats = await _reportService.GetPlantCountByCategoryAsync(startDate, endDate);

            var headers = new[] { "Danh mục", "Số lượng cây" };
            var rows = categoryStats
                .Select(s => new[] { s.CategoryName, s.PlantCount.ToString() })
                .ToList();
            // Lấy ảnh chart gửi từ form
            var chartBase64 = Request.Form["chartImage"].ToString();
            byte[] chartBytes = null;
            if (!string.IsNullOrEmpty(chartBase64))
            {
                var base64Data = chartBase64.Split(',')[1];
                chartBytes = Convert.FromBase64String(base64Data);
            }
            _logger.LogInformation("chartBytes: {Length}", chartBytes?.Length ?? 0);

            var pdfBytes = GenerateTableReport("BÁO CÁO PHÂN BỐ DANH MỤC", headers, rows, chartBytes);
            return File(pdfBytes, "application/pdf", "BaoCaoPhanBoDanhMuc.pdf");
        }


        public async Task<IActionResult> OnGetExportTopFavoritesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var topFavorites = await _reportService.GetTopFavoritePlantsAsync(top, startDate, endDate);

            var headers = new[] { "Tên cây", "Lượt yêu thích" };
            var rows = topFavorites
                .Select(s => new[] { s.PlantName, s.FavoriteCount.ToString() })
                .ToList();

            var pdfBytes = GenerateTableReport("TOP CÂY YÊU THÍCH", headers, rows);
            return File(pdfBytes, "application/pdf", "TopCayYeuThich.pdf");
        }


        public async Task<IActionResult> OnGetExportTopViewReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var topViews = await _reportService.GetTopViewedPlantsAsync(top, startDate, endDate);

            var headers = new[] { "Tên cây", "Lượt xem" };
            var rows = topViews
                .Select(s => new[] { s.PlantName, s.ViewCount.ToString() })
                .ToList();

            var pdfBytes = GenerateTableReport("TOP CÂY CÓ LƯỢT XEM CAO NHẤT", headers, rows);
            return File(pdfBytes, "application/pdf", "TopCayCoLuotXemCaoNhat.pdf");
        }



        public async Task<IActionResult> OnGetExportTopKeywordReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var topKeywords = await _reportService.GetTopSearchKeywordsAsync(10, startDate, endDate);

            var headers = new[] { "Từ khóa", "Lượt tìm kiếm" };
            var rows = topKeywords
                .Select(s => new[] { s.Keyword, s.Count.ToString() })
                .ToList();

            var pdfBytes = GenerateTableReport("TOP TỪ KHÓA TÌM KIẾM", headers, rows);
            return File(pdfBytes, "application/pdf", "TopTuKhoaTimKiem.pdf");
        }

    }
}