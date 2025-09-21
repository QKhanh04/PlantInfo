using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Models;

namespace PlantManagement.Data;

public partial class PlantDbContext : DbContext
{
    public PlantDbContext()
    {
    }

    public PlantDbContext(DbContextOptions<PlantDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Disease> Diseases { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<GrowthCondition> GrowthConditions { get; set; }

    public virtual DbSet<Plant> Plants { get; set; }

    public virtual DbSet<PlantImage> PlantImages { get; set; }

    public virtual DbSet<PlantReference> PlantReferences { get; set; }

    public virtual DbSet<SearchLog> SearchLogs { get; set; }

    public virtual DbSet<Species> Species { get; set; }

    public virtual DbSet<Use> Uses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=PlantDB;Username=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.HasIndex(e => e.CategoryName, "categories_category_name_key").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.DiseaseId).HasName("diseases_pkey");

            entity.ToTable("diseases");

            entity.Property(e => e.DiseaseId).HasColumnName("disease_id");
            entity.Property(e => e.DiseaseName)
                .HasMaxLength(255)
                .HasColumnName("disease_name");
            entity.Property(e => e.PlantId).HasColumnName("plant_id");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.Treatment).HasColumnName("treatment");

            entity.HasOne(d => d.Plant).WithMany(p => p.Diseases)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("diseases_plant_id_fkey");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PlantId }).HasName("favorites_pkey");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.UserId, "idx_favorites_user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.PlantId).HasColumnName("plant_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_at");

            entity.HasOne(d => d.Plant).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PlantId)
                .HasConstraintName("favorites_plant_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("favorites_user_id_fkey");
        });

        modelBuilder.Entity<GrowthCondition>(entity =>
        {
            entity.HasKey(e => e.PlantId).HasName("growth_conditions_pkey");

            entity.ToTable("growth_conditions");

            entity.Property(e => e.PlantId)
                .ValueGeneratedNever()
                .HasColumnName("plant_id");
            entity.Property(e => e.Climate)
                .HasMaxLength(255)
                .HasColumnName("climate");
            entity.Property(e => e.SoilType)
                .HasMaxLength(255)
                .HasColumnName("soil_type");
            entity.Property(e => e.Sunlight)
                .HasMaxLength(255)
                .HasColumnName("sunlight");
            entity.Property(e => e.TemperatureRange)
                .HasMaxLength(255)
                .HasColumnName("temperature_range");
            entity.Property(e => e.WaterRequirement)
                .HasMaxLength(255)
                .HasColumnName("water_requirement");

            entity.HasOne(d => d.Plant).WithOne(p => p.GrowthCondition)
                .HasForeignKey<GrowthCondition>(d => d.PlantId)
                .HasConstraintName("growth_conditions_plant_id_fkey");
        });

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.HasKey(e => e.PlantId).HasName("plants_pkey");

            entity.ToTable("plants");

            entity.HasIndex(e => e.CommonName, "idx_plants_common_name");

            entity.Property(e => e.PlantId).HasColumnName("plant_id");
            entity.Property(e => e.CommonName)
                .HasMaxLength(255)
                .HasColumnName("common_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Origin)
                .HasMaxLength(255)
                .HasColumnName("origin");
            entity.Property(e => e.SpeciesId).HasColumnName("species_id");

            entity.HasOne(d => d.Species).WithMany(p => p.Plants)
                .HasForeignKey(d => d.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("plants_species_id_fkey");

            entity.HasMany(d => d.Categories).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("plant_category_category_id_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("PlantId")
                        .HasConstraintName("plant_category_plant_id_fkey"),
                    j =>
                    {
                        j.HasKey("PlantId", "CategoryId").HasName("plant_category_pkey");
                        j.ToTable("plant_category");
                        j.HasIndex(new[] { "CategoryId" }, "idx_plant_category_category");
                        j.HasIndex(new[] { "PlantId" }, "idx_plant_category_plant");
                        j.IndexerProperty<int>("PlantId").HasColumnName("plant_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });

            entity.HasMany(d => d.Uses).WithMany(p => p.Plants)
                .UsingEntity<Dictionary<string, object>>(
                    "PlantUse",
                    r => r.HasOne<Use>().WithMany()
                        .HasForeignKey("UseId")
                        .HasConstraintName("plant_use_use_id_fkey"),
                    l => l.HasOne<Plant>().WithMany()
                        .HasForeignKey("PlantId")
                        .HasConstraintName("plant_use_plant_id_fkey"),
                    j =>
                    {
                        j.HasKey("PlantId", "UseId").HasName("plant_use_pkey");
                        j.ToTable("plant_use");
                        j.HasIndex(new[] { "PlantId" }, "idx_plant_use_plant");
                        j.HasIndex(new[] { "UseId" }, "idx_plant_use_use");
                        j.IndexerProperty<int>("PlantId").HasColumnName("plant_id");
                        j.IndexerProperty<int>("UseId").HasColumnName("use_id");
                    });
        });

        modelBuilder.Entity<PlantImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("plant_images_pkey");

            entity.ToTable("plant_images");

            entity.HasIndex(e => new { e.PlantId, e.IsPrimary }, "idx_plant_images_primary");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.Caption).HasColumnName("caption");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.PlantId).HasColumnName("plant_id");

            entity.HasOne(d => d.Plant).WithMany(p => p.PlantImages)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("plant_images_plant_id_fkey");
        });

        modelBuilder.Entity<PlantReference>(entity =>
        {
            entity.HasKey(e => e.ReferenceId).HasName("plant_references_pkey");

            entity.ToTable("plant_references");

            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Author)
                .HasMaxLength(255)
                .HasColumnName("author");
            entity.Property(e => e.PlantId).HasColumnName("plant_id");
            entity.Property(e => e.PublishedYear).HasColumnName("published_year");
            entity.Property(e => e.SourceName)
                .HasMaxLength(255)
                .HasColumnName("source_name");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.Plant).WithMany(p => p.PlantReferences)
                .HasForeignKey(d => d.PlantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("plant_references_plant_id_fkey");
        });

        modelBuilder.Entity<SearchLog>(entity =>
        {
            entity.HasKey(e => e.SearchId).HasName("search_logs_pkey");

            entity.ToTable("search_logs");

            entity.HasIndex(e => e.UserId, "idx_search_logs_user");

            entity.Property(e => e.SearchId).HasColumnName("search_id");
            entity.Property(e => e.Keyword)
                .HasMaxLength(500)
                .HasColumnName("keyword");
            entity.Property(e => e.SearchDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("search_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.SearchLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("search_logs_user_id_fkey");
        });

        modelBuilder.Entity<Species>(entity =>
        {
            entity.HasKey(e => e.SpeciesId).HasName("species_pkey");

            entity.ToTable("species");

            entity.HasIndex(e => e.ScientificName, "idx_species_scientific_name");

            entity.HasIndex(e => e.ScientificName, "species_scientific_name_key").IsUnique();

            entity.Property(e => e.SpeciesId).HasColumnName("species_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Family)
                .HasMaxLength(255)
                .HasColumnName("family");
            entity.Property(e => e.Genus)
                .HasMaxLength(255)
                .HasColumnName("genus");
            entity.Property(e => e.OrderName)
                .HasMaxLength(255)
                .HasColumnName("order_name");
            entity.Property(e => e.ScientificName)
                .HasMaxLength(255)
                .HasColumnName("scientific_name");
        });

        modelBuilder.Entity<Use>(entity =>
        {
            entity.HasKey(e => e.UseId).HasName("uses_pkey");

            entity.ToTable("uses");

            entity.HasIndex(e => e.UseName, "uses_use_name_key").IsUnique();

            entity.Property(e => e.UseId).HasColumnName("use_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UseName)
                .HasMaxLength(255)
                .HasColumnName("use_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "idx_users_username");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'User'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
