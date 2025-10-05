using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Export.Excel.Section;
using GrapeCity.ActiveReports.Export.Pdf.Section;
using GrapeCity.ActiveReports.SectionReportModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.Common.Results;
using PlantManagement.DTOs;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly IPlantService _plantService;
        private readonly IFavoriteService _favoriteService;

        public PlantSummaryDto PlantSummary { get; set; }
        public UserSummaryDto UserSummary { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; }
        public List<FavoriteStatDto> TopFavorites { get; set; }
        public List<KeywordStatDto> TopKeywords { get; set; }

        public ServiceResult<int> FavoriteTotal { get; set; }


        public ReportModel(IReportService reportService, IFavoriteService favoriteService, IPlantService plantService)
        {
            _reportService = reportService;
            _plantService = plantService;
            _favoriteService = favoriteService;
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            PlantSummary = await _reportService.GetPlantSummaryAsync(startDate, endDate);
            UserSummary = await _reportService.GetUserSummaryAsync(startDate, endDate);
            CategoryStats = await _reportService.GetPlantCountByCategoryAsync(startDate, endDate);
            TopFavorites = await _reportService.GetTopFavoritePlantsAsync(5, startDate, endDate);
            TopKeywords = await _reportService.GetTopSearchKeywordsAsync(8, startDate, endDate);
            FavoriteTotal = await _favoriteService.CountAsync(startDate, endDate);
        }



        public async Task<IActionResult> OnGetExportCategoryReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var categoryStats = await _reportService.GetPlantCountByCategoryAsync(startDate, endDate);

            var report = new SectionReport();

            var reportHeader = new ReportHeader();
            var pageHeader = new PageHeader { Height = 0.3f };
            var detail = new Detail { Height = 0.3f };
            var pageFooter = new PageFooter();
            var reportFooter = new ReportFooter();

            report.Sections.Add(reportHeader);
            report.Sections.Add(pageHeader);
            report.Sections.Add(detail);
            report.Sections.Add(pageFooter);
            report.Sections.Add(reportFooter);

            // Header
            float left = 0f, width = 3f;
            var headers = new[] { "Danh mục", "Số lượng cây" };
            for (int i = 0; i < headers.Length; i++)
            {
                var tb = new TextBox
                {
                    Left = left,
                    Top = 0,
                    Width = width,
                    Height = 0.3f,
                    Text = headers[i],
                    Style = "font-weight: bold; background-color: LightGray;"
                };
                pageHeader.Controls.Add(tb);
                left += width;
            }

            // Detail
            left = 0f;
            var dataFields = new[] { "CategoryName", "PlantCount" };
            for (int i = 0; i < dataFields.Length; i++)
            {
                var tb = new TextBox
                {
                    Left = left,
                    Top = 0,
                    Width = width,
                    Height = 0.3f,
                    DataField = dataFields[i]
                };
                detail.Controls.Add(tb);
                left += width;
            }

            // Data source
            var dataList = categoryStats.Select(s => new
            {
                CategoryName = s.CategoryName,
                PlantCount = s.PlantCount
            }).ToList();
            report.DataSource = dataList;

            report.Run();

            using (var ms = new MemoryStream())
            {
                var pdfExport = new GrapeCity.ActiveReports.Export.Pdf.Section.PdfExport();
                pdfExport.Export(report.Document, ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", "BaoCaoPhanBoDanhMuc.pdf");
            }
        }
        public async Task<IActionResult> OnGetExportTopFavoritesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var topFavorites = await _reportService.GetTopFavoritePlantsAsync(5, startDate, endDate);

            var report = new SectionReport();

            var reportHeader = new ReportHeader();
            var pageHeader = new PageHeader { Height = 0.3f };
            var detail = new Detail { Height = 0.3f };
            var pageFooter = new PageFooter();
            var reportFooter = new ReportFooter();

            report.Sections.Add(reportHeader);
            report.Sections.Add(pageHeader);
            report.Sections.Add(detail);
            report.Sections.Add(pageFooter);
            report.Sections.Add(reportFooter);

            // Header
            float left = 0f, width = 3f;
            var headers = new[] { "Tên cây", "Lượt yêu thích" };
            for (int i = 0; i < headers.Length; i++)
            {
                var tb = new TextBox
                {
                    Left = left,
                    Top = 0,
                    Width = width,
                    Height = 0.3f,
                    Text = headers[i],
                    Style = "font-weight: bold; background-color: LightGray;"
                };
                pageHeader.Controls.Add(tb);
                left += width;
            }

            // Detail
            left = 0f;
            var dataFields = new[] { "PlantName", "FavoriteCount" };
            for (int i = 0; i < dataFields.Length; i++)
            {
                var tb = new TextBox
                {
                    Left = left,
                    Top = 0,
                    Width = width,
                    Height = 0.3f,
                    DataField = dataFields[i]
                };
                detail.Controls.Add(tb);
                left += width;
            }

            // Data source
            var dataList = topFavorites.Select(s => new
            {
                PlantName = s.PlantName,
                FavoriteCount = s.FavoriteCount
            }).ToList();
            report.DataSource = dataList;

            report.Run();

            using (var ms = new MemoryStream())
            {
                var pdfExport = new PdfExport();
                pdfExport.Export(report.Document, ms);
                ms.Position = 0;
                return File(ms.ToArray(), "application/pdf", "TopCayYeuThich.pdf");
            }
        }

    }

}