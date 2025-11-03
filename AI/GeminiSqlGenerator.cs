using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantManagement.Data;
using PlantManagement.Services.Implementations;

namespace PlantManagement.AI
{
    public class GeminiSqlGenerator
    {
        private readonly PlantDbContext _dbContext;
        private readonly GeminiService _geminiService;
        private readonly string _schemaDescription;
        private readonly int _defaultRowLimit = 50;

        private static readonly string[] AllowedTables = new[]
        {
            "plants", "species", "growth_conditions", "diseases", "uses", "plant_use", "plant_images", "plant_references",
            "categories", "plant_category"
        };

        private static readonly string[] ForbiddenTokens = new[]
        {
            "drop", "delete", "insert", "update", "truncate", "alter",
            "create", "grant", "revoke", "exec", "execute", ";--", "--"
        };

        public GeminiSqlGenerator(PlantDbContext dbContext, GeminiService geminiService)
        {
            _dbContext = dbContext;
            _geminiService = geminiService;

            var schemaPath = Path.Combine(AppContext.BaseDirectory, "AI", "schemaDescription.txt");
            _schemaDescription = File.Exists(schemaPath)
                ? File.ReadAllText(schemaPath)
                : "No schema description found.";

            Console.WriteLine("==== LOADED SCHEMA DESCRIPTION ====");
            Console.WriteLine(_schemaDescription);
        }

        public async Task<SqlGenerationResult> RunQueryFlowAsync(string userQuestion)
        {
            var prompt = BuildSqlGenerationPrompt(userQuestion);
            string rawAiResponse = await _geminiService.AskGeminiAsync(prompt);
            string generatedSql = ExtractSql(rawAiResponse);

            if (string.IsNullOrWhiteSpace(generatedSql))
                return SqlGenerationResult.Fail("Không sinh được câu SQL từ AI.");

            string normalizedSql = NormalizeSql(generatedSql);

            // Loại bỏ prefix schema nếu có
            normalizedSql = RemoveSchemaPrefix(normalizedSql);

            if (!IsSafeSql(normalizedSql))
                return SqlGenerationResult.Fail("Câu SQL không an toàn hoặc chứa từ khóa bị cấm.");

            if (!IsSqlWithinSchema(normalizedSql))
                return SqlGenerationResult.Fail("Câu SQL chứa bảng không tồn tại trong schema public.");

            normalizedSql = EnsureSelectAndLimit(normalizedSql, _defaultRowLimit);

            try
            {
                var rows = await ExecuteSelectAsync(normalizedSql);
                return SqlGenerationResult.Success(normalizedSql, rows, rawAiResponse);
            }
            catch (Exception ex)
            {
                return SqlGenerationResult.Fail($"Lỗi khi chạy SQL: {ex.Message}");
            }
        }

        private string BuildSqlGenerationPrompt(string userQuestion)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là trợ lý SQL chỉ được phép đọc dữ liệu (SELECT).");
            sb.AppendLine("Database schema đầy đủ như sau:");
            sb.AppendLine(_schemaDescription);
            sb.AppendLine();
            sb.AppendLine("Hãy sinh ra đúng MỘT câu lệnh SQL duy nhất để trả lời câu hỏi sau:");
            sb.AppendLine($"Câu hỏi: \"{userQuestion}\"");
            sb.AppendLine();
            sb.AppendLine("QUY TẮC:");
            sb.AppendLine("- Chỉ dùng SELECT, không được phép dùng các câu lệnh khác.");
            sb.AppendLine("- Nếu truy vấn nhiều bản ghi, hãy thêm LIMIT 50.");
            sb.AppendLine("- Hãy sinh câu SQL, nhưng **giữ nguyên từ khóa tiếng Việt trong điều kiện WHERE**, không dịch sang tiếng Anh.");
            sb.AppendLine("- Nếu tìm theo tên, dùng ILIKE '%keyword%'.");
            sb.AppendLine("- Chỉ lấy những cột liên quan đến cây trồng và thông tin cần thiết để trả lời câu hỏi. ");
            sb.AppendLine("- Không lấy các thuộc tính của hệ thông như id, created_at, updated_at trừ khi thực sự cần thiết.");
            sb.AppendLine("- KHÔNG giải thích, KHÔNG in thêm gì ngoài SQL.");
            return sb.ToString();
        }

        private string ExtractSql(string aiText)
        {
            if (string.IsNullOrWhiteSpace(aiText)) return null;

            aiText = Regex.Replace(aiText, @"```(sql)?", "", RegexOptions.IgnoreCase).Trim();

            var match = Regex.Match(aiText, @"(?is)(select\b.*?;)|(?is)(select\b.*$)");
            if (match.Success)
            {
                var sql = match.Value.Trim('`', ';', ' ');
                return sql;
            }

            return aiText.Trim();
        }

        private string NormalizeSql(string sql)
        {
            return Regex.Replace(sql ?? "", @"\s+", " ").Trim();
        }

        private string RemoveSchemaPrefix(string sql)
        {
            // Loại bỏ tất cả "plant_schema." nếu AI vô tình sinh ra
            return Regex.Replace(sql ?? "", @"\bplant_schema\.", "", RegexOptions.IgnoreCase);
        }

        private bool IsSafeSql(string sql)
        {
            var lower = sql.ToLowerInvariant();
            if (!lower.StartsWith("select")) return false;
            return !ForbiddenTokens.Any(t => lower.Contains(t));
        }

        private bool IsSqlWithinSchema(string sql)
        {
            var lower = sql.ToLowerInvariant();

            var matches = Regex.Matches(lower, @"\bfrom\s+([a-zA-Z0-9_\.]+)|\bjoin\s+([a-zA-Z0-9_\.]+)");
            foreach (Match m in matches)
            {
                var table = m.Value.Split(' ').Last().Split('.').Last();
                if (!AllowedTables.Contains(table))
                    return false;
            }
            return true;
        }

        private string EnsureSelectAndLimit(string sql, int limit)
        {
            if (!Regex.IsMatch(sql, @"\blimit\s+\d+", RegexOptions.IgnoreCase))
                sql += $" LIMIT {limit}";
            return sql;
        }

        private async Task<List<Dictionary<string, object>>> ExecuteSelectAsync(string sql)
        {
            var rows = new List<Dictionary<string, object>>();
            var conn = _dbContext.Database.GetDbConnection();
            await _dbContext.Database.OpenConnectionAsync();


            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.IsDBNullAsync(i)
                            ? null
                            : reader.GetValue(i);
                    }
                    rows.Add(row);
                }
            }
            finally
            {
                await _dbContext.Database.CloseConnectionAsync();
            }

            return rows;
        }
    }

    public record SqlGenerationResult
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = "";
        public string GeneratedSql { get; init; } = "";
        public List<Dictionary<string, object>> Rows { get; init; } = new();
        public string RawAiResponse { get; init; } = "";

        public static SqlGenerationResult Success(string sql, List<Dictionary<string, object>> rows, string rawAi)
            => new() { IsSuccess = true, GeneratedSql = sql, Rows = rows, RawAiResponse = rawAi };

        public static SqlGenerationResult Fail(string message)
            => new() { IsSuccess = false, Message = message };
    }
}
