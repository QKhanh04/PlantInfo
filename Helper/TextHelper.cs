using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantManagement.Helper
{
    public static class TextHelper
    {
        public static string NormalizeKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return "";

            keyword = keyword.ToLower().Trim();

            // Loại bỏ các từ vô nghĩa thường gặp
            string[] stopWords = { "cây", "loài", "thực vật", "loại cây", "loại", "cây thuốc" };
            foreach (var word in stopWords)
            {
                keyword = keyword.Replace(word, " ");
            }

            // Chuẩn hóa: bỏ dấu, gộp khoảng trắng
            // keyword = RemoveDiacritics(keyword);
            keyword = System.Text.RegularExpressions.Regex.Replace(keyword, @"\s+", " ");

            return keyword.Trim();
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalized
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                            != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }

    }
}