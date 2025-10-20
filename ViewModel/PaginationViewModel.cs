using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantManagement.ViewModel
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Hàm tạo URL cho 1 trang (ví dụ: p => Url.Page(..., new { CurrentPage = p, ... }))
        // Partial sẽ gọi Model.PageUrl(pageNumber)
        public Func<int, string> PageUrl { get; set; }
    }
}