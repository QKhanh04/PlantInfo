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
            // Plant <-> PlantListDTO, PlantDetailDTO, PlantDTO, PlantCreateDTO, PlantUpdateDTO
            CreateMap<Plant, PlantListDTO>()
                .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.ScientificName : null))
                .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.Categories.Select(c => c.CategoryName).ToList()))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.PlantImages.Select(img => img.ImageUrl).ToList()));

            CreateMap<Plant, PlantDetailDTO>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.PlantImages.Select(img => img.ImageUrl).ToList()))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PlantImages))
                .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.PlantReferences));

            CreateMap<Plant, PlantDTO>().ReverseMap();
            CreateMap<PlantCreateDTO, Plant>()
                .ForMember(d => d.CreateAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdateAt, o => o.MapFrom(_ => DateTime.UtcNow));
            CreateMap<PlantUpdateDTO, Plant>().ReverseMap();
            CreateMap<Plant, PlantUpdateDTO>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PlantImages))
                .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.PlantReferences));

            CreateMap<PlantDetailDTO, PlantUpdateDTO>();

            // Disease <-> DiseasesDTO, DiseaseCreateDTO
            CreateMap<Disease, DiseasesDTO>().ReverseMap();
            CreateMap<Disease, DiseaseCreateDTO>().ReverseMap();

            // Use, Category, Species, GrowthCondition, PlantImage, PlantReference
            CreateMap<Use, UseDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Species, SpeciesDTO>().ReverseMap();
            CreateMap<GrowthCondition, GrowthConditionDTO>().ReverseMap();
            CreateMap<PlantImage, PlantImageDTO>().ReverseMap();
            CreateMap<PlantReference, PlantReferenceDTO>().ReverseMap();

            // CreateDTO -> Entity
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<UseCreateDTO, Use>();
            CreateMap<SpeciesCreateDTO, Species>();

            // Review
            CreateMap<PlantReview, ReviewDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));
            CreateMap<ReviewDTO, PlantReview>();
            CreateMap<PlantReview, CreateReviewDTO>().ReverseMap();
            CreateMap<PlantReview, UpdateReviewDTO>().ReverseMap();

            // User
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Disease, DiseaseDTO>()
                        .ForMember(dest => dest.PlantName, opt => opt.MapFrom(src => src.Plant.CommonName));
            CreateMap<DiseaseDTO, Disease>();
        }
    }
}