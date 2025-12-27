using System.Globalization;
using CovenantPromptKey.Constants;
using CovenantPromptKey.Models;
using CovenantPromptKey.Models.Results;
using CovenantPromptKey.Services.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace CovenantPromptKey.Services.Implementations;

/// <summary>
/// CSV 匯入/匯出服務實作
/// 使用 CsvHelper 進行 CSV 處理
/// </summary>
public class CsvService : ICsvService
{
    private readonly IDebugLogService _logService;

    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        TrimOptions = TrimOptions.Trim,
        MissingFieldFound = null,
        HeaderValidated = null
    };

    public CsvService(IDebugLogService logService)
    {
        _logService = logService;
    }

    public string ExportToCsv(IReadOnlyList<KeywordMapping> mappings)
    {
        try
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CsvConfig);

            // Write header
            csv.WriteField("SensitiveKey");
            csv.WriteField("SafeKey");
            csv.WriteField("HighlightColor");
            csv.NextRecord();

            // Write data
            foreach (var mapping in mappings)
            {
                csv.WriteField(mapping.SensitiveKey);
                csv.WriteField(mapping.SafeKey);
                csv.WriteField(mapping.HighlightColor);
                csv.NextRecord();
            }

            var result = writer.ToString();
            _logService.Log($"匯出 {mappings.Count} 筆關鍵字至 CSV", Models.LogLevel.Info);
            return result;
        }
        catch (Exception ex)
        {
            _logService.Log($"CSV 匯出失敗: {ex.Message}", Models.LogLevel.Error);
            throw;
        }
    }

    public CsvImportResult ImportFromCsv(string csvContent)
    {
        var importedMappings = new List<KeywordMapping>();
        var errors = new List<CsvError>();
        var warnings = new List<string>();
        var seenSafeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skippedCount = 0;

        try
        {
            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, CsvConfig);

            // Read header
            if (!csv.Read() || !csv.ReadHeader())
            {
                errors.Add(new CsvError { LineNumber = 1, Message = "無法讀取 CSV 標頭" });
                return new CsvImportResult
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Validate required headers
            var headers = csv.HeaderRecord ?? [];
            var hasSensitiveKey = headers.Any(h => h.Equals("SensitiveKey", StringComparison.OrdinalIgnoreCase));
            var hasSafeKey = headers.Any(h => h.Equals("SafeKey", StringComparison.OrdinalIgnoreCase));

            if (!hasSensitiveKey || !hasSafeKey)
            {
                errors.Add(new CsvError
                {
                    LineNumber = 1,
                    Message = "CSV 必須包含 SensitiveKey 和 SafeKey 欄位"
                });
                return new CsvImportResult
                {
                    Success = false,
                    Errors = errors
                };
            }

            var lineNumber = 1; // Header is line 1
            var colorIndex = 0;

            while (csv.Read())
            {
                lineNumber++;

                try
                {
                    var sensitiveKey = csv.GetField("SensitiveKey")?.Trim() ?? string.Empty;
                    var safeKey = csv.GetField("SafeKey")?.Trim() ?? string.Empty;
                    var highlightColor = csv.GetField("HighlightColor")?.Trim();

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(sensitiveKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = "SensitiveKey 不可為空"
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(safeKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = "SafeKey 不可為空"
                        });
                        continue;
                    }

                    // Check for duplicate SafeKey
                    if (seenSafeKeys.Contains(safeKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = $"SafeKey '{safeKey}' 重複"
                        });
                        skippedCount++;
                        continue;
                    }

                    // Check for reserved keywords
                    if (ReservedKeywords.IsReserved(safeKey))
                    {
                        warnings.Add($"第 {lineNumber} 行: SafeKey '{safeKey}' 是程式語言保留字");
                    }

                    // Assign default color if not provided
                    if (string.IsNullOrWhiteSpace(highlightColor) || !highlightColor.StartsWith('#'))
                    {
                        highlightColor = AppConstants.DefaultColors[colorIndex % AppConstants.DefaultColors.Length];
                        colorIndex++;
                    }

                    var mapping = new KeywordMapping
                    {
                        SensitiveKey = sensitiveKey,
                        SafeKey = safeKey,
                        HighlightColor = highlightColor
                    };

                    importedMappings.Add(mapping);
                    seenSafeKeys.Add(safeKey);
                }
                catch (Exception ex)
                {
                    errors.Add(new CsvError
                    {
                        LineNumber = lineNumber,
                        Message = $"解析錯誤: {ex.Message}"
                    });
                }
            }

            var success = errors.Count == 0 || importedMappings.Count > 0;

            _logService.Log(
                $"CSV 匯入完成: {importedMappings.Count} 成功, {skippedCount} 跳過, {errors.Count} 錯誤",
                success ? Models.LogLevel.Info : Models.LogLevel.Warning);

            return new CsvImportResult
            {
                Success = success,
                ImportedCount = importedMappings.Count,
                SkippedCount = skippedCount,
                ImportedMappings = importedMappings,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logService.Log($"CSV 匯入失敗: {ex.Message}", Models.LogLevel.Error);
            errors.Add(new CsvError { LineNumber = 0, Message = $"CSV 解析失敗: {ex.Message}" });
            return new CsvImportResult
            {
                Success = false,
                Errors = errors
            };
        }
    }

    public CsvValidationResult ValidateCsv(string csvContent)
    {
        var errors = new List<CsvError>();

        try
        {
            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, CsvConfig);

            // Read and validate header
            if (!csv.Read() || !csv.ReadHeader())
            {
                errors.Add(new CsvError { LineNumber = 1, Message = "無法讀取 CSV 標頭" });
                return new CsvValidationResult { IsValid = false, Errors = errors };
            }

            var headers = csv.HeaderRecord ?? [];
            var hasSensitiveKey = headers.Any(h => h.Equals("SensitiveKey", StringComparison.OrdinalIgnoreCase));
            var hasSafeKey = headers.Any(h => h.Equals("SafeKey", StringComparison.OrdinalIgnoreCase));

            if (!hasSensitiveKey || !hasSafeKey)
            {
                errors.Add(new CsvError
                {
                    LineNumber = 1,
                    Message = "CSV 必須包含 SensitiveKey 和 SafeKey 欄位"
                });
                return new CsvValidationResult { IsValid = false, Errors = errors };
            }

            var seenSafeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lineNumber = 1;

            while (csv.Read())
            {
                lineNumber++;

                try
                {
                    var sensitiveKey = csv.GetField("SensitiveKey")?.Trim();
                    var safeKey = csv.GetField("SafeKey")?.Trim();

                    if (string.IsNullOrWhiteSpace(sensitiveKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = "SensitiveKey 不可為空"
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(safeKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = "SafeKey 不可為空"
                        });
                        continue;
                    }

                    if (seenSafeKeys.Contains(safeKey))
                    {
                        errors.Add(new CsvError
                        {
                            LineNumber = lineNumber,
                            Message = $"SafeKey '{safeKey}' 重複"
                        });
                    }
                    else
                    {
                        seenSafeKeys.Add(safeKey);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new CsvError
                    {
                        LineNumber = lineNumber,
                        Message = $"解析錯誤: {ex.Message}"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add(new CsvError { LineNumber = 0, Message = $"CSV 驗證失敗: {ex.Message}" });
        }

        return new CsvValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
