using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PlantInformation.DTOs;
using PlantManagement.DTOs;
using PlantManagement.Models;

namespace PlantManagement.Mappings
{
    public class PlantProfile : Profile
    {
        public PlantProfile()
        {
            CreateMap<Plant, PlantListDTO>()
                .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.ScientificName : null))
                .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName).ToList()))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.PlantImages.Select(img => img.ImageUrl).ToList()));


            CreateMap<Plant, PlantDetailDTO>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.PlantImages.Select(img => img.ImageUrl).ToList()))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PlantImages))
                .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.PlantReferences));

            CreateMap<Disease, DiseasesDTO>();
            // CreateMap<Use, UseDTO>();
            CreateMap<Species, SpeciesDTO>();
            CreateMap<GrowthCondition, GrowthConditionDTO>();
            CreateMap<PlantImage, PlantImageDTO>();
            // CreateMap<Category, CategoryDTO>();

            CreateMap<PlantCreateDTO, Plant>()
                .ForMember(d => d.CreateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdateAt, o => o.MapFrom(_ => DateTime.UtcNow));
            CreateMap<Plant, PlantDTO>();

            CreateMap<GrowthConditionDTO, GrowthCondition>();
            CreateMap<GrowthCondition, GrowthConditionDTO>();

            CreateMap<DiseaseDTO, Disease>();
            CreateMap<Disease, DiseaseDTO>();

            CreateMap<PlantImageDTO, PlantImage>();
            CreateMap<PlantImage, PlantImageDTO>();

            CreateMap<PlantReferenceDTO, PlantReference>();
            CreateMap<PlantReference, PlantReferenceDTO>();

            // create DTO -> entity for categories/uses
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<UseCreateDTO, Use>();

            // entity -> DTO (for listing dropdowns)
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryDTO, Category>();

            CreateMap<Use, UseDTO>();
            CreateMap<UseDTO, Use>();

            CreateMap<SpeciesCreateDTO, Species>();
            CreateMap<PlantUpdateDTO, Plant>();
            CreateMap<Plant, PlantUpdateDTO>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PlantImages))
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.PlantReferences));
            CreateMap<PlantDetailDTO, PlantUpdateDTO>();
            CreateMap<PlantReview, ReviewDTO>()
                        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));
            CreateMap<ReviewDTO, PlantReview>();

            CreateMap<PlantReview, CreateReviewDTO>();
            CreateMap<CreateReviewDTO, PlantReview>();

            CreateMap<PlantReview, UpdateReviewDTO>();
            CreateMap<UpdateReviewDTO, PlantReview>();

        }
    }
}