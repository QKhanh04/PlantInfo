using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.DTOs
{
    public class PlantUpdateDTO
    {
         public int PlantId { get; set; } // Khóa chính để xác định cây cần cập nhật

        public string? CommonName { get; set; }
        public string? Origin { get; set; }
        public string? Description { get; set; }

        public int? SpeciesId { get; set; }
        public SpeciesDTO? Species { get; set; } // Nếu cho phép sửa hoặc thay đổi loài

        public List<int>? CategoryIds { get; set; } // Nếu cho phép thay đổi danh mục
        public List<CategoryCreateDTO>? NewCategories { get; set; } // Nếu cho phép thêm mới danh mục

        public List<int>? UseIds { get; set; }
        public List<UseCreateDTO>? NewUses { get; set; }

        public List<int>? DiseaseIds { get; set; } // Nếu cho phép thay đổi danh mục
        public List<DiseaseCreateDTO>? NewDiseases { get; set; }

        public GrowthConditionDTO? GrowthCondition { get; set; }



        public List<PlantImageDTO>? Images { get; set; } // Nếu cho phép sửa/ thêm/ xóa ảnh

        public List<PlantReferenceDTO>? References { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }


    }
}