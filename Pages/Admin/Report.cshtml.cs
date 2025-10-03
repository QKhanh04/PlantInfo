using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PlantManagement.DTOs;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReportModel : PageModel
    {
        private readonly IReportService _reportService;
        private readonly IPlantService _plantService;

        public PlantSummaryDto PlantSummary { get; set; }
        public UserSummaryDto UserSummary { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; }
        public List<FavoriteStatDto> TopFavorites { get; set; }
        public List<KeywordStatDto> TopKeywords { get; set; }

        public ReportModel(IReportService reportService, IPlantService plantService)
        {
            _reportService = reportService;
            _plantService = plantService;
        }

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            PlantSummary = await _reportService.GetPlantSummaryAsync(startDate, endDate);
            UserSummary = await _reportService.GetUserSummaryAsync(startDate, endDate);
            CategoryStats = await _reportService.GetPlantCountByCategoryAsync(startDate, endDate);
            TopFavorites = await _reportService.GetTopFavoritePlantsAsync(8, startDate, endDate);
            TopKeywords = await _reportService.GetTopSearchKeywordsAsync(8, startDate, endDate);
        }

    //     public async Task<IActionResult> OnGetExportAllPlantDetails()
    //     {
    //         var plantDetails = await _plantService.GetAllPlantAsync();
    //         if (plantDetails.Data != null)
    //         {
    //             int idx = 0;
    //             foreach (var p in plantDetails.Data.Take(3)) // lấy 3 cây đầu
    //             {
    //                 Console.WriteLine($"--- Cây thứ {idx + 1} ---");
    //                 Console.WriteLine($"ID: {p.PlantId}");
    //                 Console.WriteLine($"Tên thường gọi: {p.CommonName}");
    //                 Console.WriteLine($"Nguồn gốc: {p.Origin}");
    //                 Console.WriteLine($"Mô tả: {p.Description}");
    //                 Console.WriteLine($"Loài: {p.Species?.ScientificName}");
    //                 Console.WriteLine($"Danh mục: {(p.Categories != null ? string.Join(", ", p.Categories.Select(c => c.CategoryName)) : "")}");
    //                 Console.WriteLine($"Công dụng: {(p.Uses != null ? string.Join(", ", p.Uses.Select(u => u.UseName)) : "")}");
    //                 Console.WriteLine($"Bệnh: {(p.Diseases != null ? string.Join(", ", p.Diseases.Select(d => d.DiseaseName)) : "")}");
    //                 if (p.GrowthCondition != null)
    //                 {
    //                     Console.WriteLine($"Điều kiện sinh trưởng: Đất-{p.GrowthCondition.SoilType}, Nước-{p.GrowthCondition.WaterRequirement}, Nhiệt độ-{p.GrowthCondition.TemperatureRange}, Thời tiết-{p.GrowthCondition.Climate}, Ánh sáng-{p.GrowthCondition.Sunlight}");
    //                 }
    //                 Console.WriteLine($"Ảnh: {(p.Images != null ? string.Join(", ", p.Images.Select(i => i.ImageUrl)) : "")}");
    //                 Console.WriteLine($"Tham khảo: {(p.References != null ? string.Join(", ", p.References.Select(r => r.SourceName)) : "")}");
    //                 idx++;
    //             }
    //         }
    //         else
    //         {
    //             Console.WriteLine("plantDetails.Data là null hoặc không có dữ liệu.");
    //         }
    //         var report = new SectionReport();

    //         var reportHeader = new ReportHeader();
    //         var pageHeader = new PageHeader();
    //         var detail = new Detail();
    //         var pageFooter = new PageFooter();
    //         var reportFooter = new ReportFooter();

    //         report.Sections.Add(reportHeader);
    //         report.Sections.Add(pageHeader);
    //         report.Sections.Add(detail);
    //         report.Sections.Add(pageFooter);
    //         report.Sections.Add(reportFooter);

    //         string[] headers = {
    //             "ID", "Tên thường gọi", "Nguồn gốc", "Mô tả", "Loài", "Danh mục",
    //             "Công dụng", "Bệnh", "Điều kiện sinh trưởng", "Ảnh", "Tham khảo"
    //         };

    //         float left = 0f;
    //         float width = 2f;

    //         for (int i = 0; i < headers.Length; i++)
    //         {
    //             var tb = new TextBox
    //             {
    //                 Left = left,
    //                 Top = 0,
    //                 Width = width,
    //                 Height = 0.3f,
    //                 Text = headers[i],
    //                 Style = "font-weight: bold; background-color: LightGray;"
    //             };
    //             pageHeader.Controls.Add(tb);
    //             left += width;
    //         }

    //         left = 0f;
    //         string[] dataFields = {
    //             "PlantId", "CommonName", "Origin", "Description", "ScientificName", "CategoryNames",
    //             "UseNames", "DiseaseNames", "GrowthConditionSummary", "ImageUrls", "ReferenceNames"
    //         };

    //         for (int i = 0; i < dataFields.Length; i++)
    //         {
    //             var tb = new TextBox
    //             {
    //                 Left = left,
    //                 Top = 0,
    //                 Width = width,
    //                 Height = 0.3f,
    //                 DataField = dataFields[i]
    //             };
    //             detail.Controls.Add(tb);
    //             left += width;
    //         }

    //         var dataList = (plantDetails.Data ?? Enumerable.Empty<PlantDetailDTO>())
    //  .Select(p => new
    //  {
    //      PlantId = p.PlantId,
    //      CommonName = p.CommonName ?? "",
    //      Origin = p.Origin ?? "",
    //      Description = p.Description ?? "",
    //      ScientificName = p.Species?.ScientificName ?? "",
    //      CategoryNames = p.Categories != null ? string.Join(", ", p.Categories.Select(c => c.CategoryName)) : "",
    //      UseNames = p.Uses != null ? string.Join(", ", p.Uses.Select(u => u.UseName)) : "",
    //      DiseaseNames = p.Diseases != null ? string.Join(", ", p.Diseases.Select(d => d.DiseaseName)) : "",
    //      GrowthConditionSummary = p.GrowthCondition != null
    //          ? $"Đất: {p.GrowthCondition.SoilType}, Lượng nước: {p.GrowthCondition.WaterRequirement}, Nhiệt độ: {p.GrowthCondition.TemperatureRange}, Thời tiết: {p.GrowthCondition.Climate}, Ánh sáng: {p.GrowthCondition.Sunlight}"
    //          : "",
    //      ImageUrls = p.Images != null ? string.Join(", ", p.Images.Select(i => i.ImageUrl)) : "",
    //      ReferenceNames = p.References != null ? string.Join(", ", p.References.Select(r => r.SourceName)) : ""
    //  }).ToList();
    //         report.DataSource = dataList;

    //         report.Run();

    //         using (var ms = new MemoryStream())
    //         {
    //             var xlsExport = new XlsExport();
    //             xlsExport.Export(report.Document, ms);
    //             ms.Position = 0;
    //             return File(ms.ToArray(),
    //                 "application/vnd.ms-excel",
    //                 "DanhSachChiTietCayTrong.xls");
    //         }
    //     }

        

        
    }
}