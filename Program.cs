using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Repositories.Implementations;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Implementations;
using PlantManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PlantDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic Repository (nếu bạn dùng)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
 
// Repository
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUseRepository, UseRepository>();
builder.Services.AddScoped<IGrowthConditionRepository, GrowthConditionRepository>();
builder.Services.AddScoped<IDiseaseRepository, DiseaseRepository>();
builder.Services.AddScoped<IPlantImageRepository, PlantImageRepository>();
builder.Services.AddScoped<IPlantReferenceRepository, PlantReferenceRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IViewLogRepository, ViewLogRepository>();
builder.Services.AddScoped<IPlantReviewRepository, PlantReviewRepository>();

// // Nếu có bảng liên kết:
// builder.Services.AddScoped<IPlantCategoryRepository, PlantCategoryRepository>();
// builder.Services.AddScoped<IPlantUseRepository, PlantUseRepository>();

// Service
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUseService, UseService>();
builder.Services.AddScoped<IGrowthConditionService, GrowthConditionService>();
builder.Services.AddScoped<IDiseaseService, DiseaseService>();
builder.Services.AddScoped<IPlantImageService, PlantImageService>();
builder.Services.AddScoped<IPlantReferenceService, PlantReferenceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IViewLogService, ViewLogService>();
builder.Services.AddScoped<IPlantReviewService, PlantReviewService>();

// Nếu có bảng liên kết:
// builder.Services.AddScoped<IPlantCategoryService, PlantCategoryService>();
// builder.Services.AddScoped<IPlantUseService, PlantUseService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Authentication";          // Trang login khi chưa đăng nhập
        options.AccessDeniedPath = "/Auth/Authentication";  // Trang khi không có quyền
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });
// Add AutoMapper for all profiles in the assembly
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSession();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
