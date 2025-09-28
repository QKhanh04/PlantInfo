using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantManagement.Common.Results;
using PlantManagement.Models;
using PlantManagement.Repositories.Interfaces;
using PlantManagement.Services.Interfaces;

namespace PlantManagement.Services.Implementations
{
    public class PlantImageService : IPlantImageService
    {
        private readonly IPlantImageRepository _repo;
    public PlantImageService(IPlantImageRepository repo)
    {
        _repo = repo;
    }
    public async Task<ServiceResult<PlantImage>> CreateAsync(PlantImage image)
    {
        try
        {
            await _repo.AddAsync(image);
            await _repo.SaveChangesAsync();
            return ServiceResult<PlantImage>.Ok(image, "Image created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PlantImage>.Fail($"Error creating image: {ex.Message}");
        }
    }
    }
}