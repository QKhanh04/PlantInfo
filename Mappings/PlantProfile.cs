using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Mappings
{
    public class PlantProfile : Profile
    {
        public PlantProfile()
        {
            CreateMap<Plant, PlantDTO>()
                .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.ScientificName : null))
                .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName).ToList()));


            CreateMap<Plant, PlantDetailDTO>()
                .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName).ToList()))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.PlantImages.Select(img => img.ImageUrl).ToList()));
            CreateMap<Disease, DiseasesDTO>();
            CreateMap<Use, UsesDTO>();
            CreateMap<Species, SpeciesDTO>();
            CreateMap<GrowthCondition, GrowthConditionDTO>();


            CreateMap<PlantCreateDTO, Plant>()
                           .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                           .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // GrowthCondition
            CreateMap<GrowthConditionDTO, GrowthCondition>();

            // Disease
            CreateMap<DiseaseDTO, Disease>();

            // PlantImage
            CreateMap<PlantImageDTO, PlantImage>();

            // PlantReference
            CreateMap<PlantReferenceDTO, PlantReference>();


        }
    }
}