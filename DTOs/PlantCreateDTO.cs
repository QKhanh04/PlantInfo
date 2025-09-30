using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class SpeciesCreateDTO
    {
        public string? ScientificName { get; set; } = string.Empty;
        public string? Genus { get; set; }
        public string? Family { get; set; }
        public string? OrderName { get; set; }
        public string? Description { get; set; }
    }

    // // DTO cho GrowthConditions
    // public class GrowthConditionDTO
    // {
    //     public string? SoilType { get; set; }
    //     public string? Climate { get; set; }
    //     public string? TemperatureRange { get; set; }
    //     public string? WaterRequirement { get; set; }
    //     public string? Sunlight { get; set; }
    // }

    // DTO cho Disease
    public class DiseaseDTO
    {
        public int? DiseaseId { get; set; }
        public string? DiseaseName { get; set; } = string.Empty;
        public string? Symptoms { get; set; }
        public string? Treatment { get; set; }
    }

    // DTO cho Category
    public class CategoryCreateDTO
    {
        public string? CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // DTO cho Use
    public class UseCreateDTO
    {
        public string? UseName { get; set; } = string.Empty;
        public string? Description { get; set; }

    }

    // DTO cho PlantImage
    public class PlantImageDTO
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPrimary { get; set; }
    }

    // DTO cho PlantReference
    public class PlantReferenceDTO
    {
        public string? SourceName { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? Author { get; set; }
        public int? PublishedYear { get; set; }
    }

    // DTO chính cho Plant (dùng khi thêm mới)
    public class PlantCreateDTO
    {
        public string CommonName { get; set; } = string.Empty;
        public string? Origin { get; set; }
        public string? Description { get; set; }

        public int? SpeciesId { get; set; }   // Có thể chọn sẵn loài
        public SpeciesCreateDTO? NewSpecies { get; set; }


        public List<int>? CategoryIds { get; set; }
        public List<int>? UseIds { get; set; }

        public List<CategoryCreateDTO>? NewCategories { get; set; }
        public List<UseCreateDTO>? NewUses { get; set; }


        public GrowthConditionDTO? GrowthCondition { get; set; }
        public List<DiseaseDTO>? Diseases { get; set; }
        public List<PlantImageDTO>? Images { get; set; }
        public List<PlantReferenceDTO>? References { get; set; }
    }
}