using BusinessLogic.UnitOfWork;
using ClosedXML.Excel;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using ViewModels;
using BusinessLogic.CommonService;

namespace BusinessLogic.MedicinesService
{
    public class MedicinesService : IMedicinesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        public MedicinesService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }

        public async Task<ResponseObj<string>> CreateMedicines(MedicinesRequestModel request)
        {
            var response = new ResponseObj<string>();
            if (!string.IsNullOrEmpty(request.ExcelBase64))
            {
                byte[] bytes;
                
                    bytes = Convert.FromBase64String(request.ExcelBase64);
               
                List<Medicines> list;
                
                    list = IsExcelFile(bytes)
                        ? ParseMedicinesFromExcel(bytes)
                        : ParseMedicinesFromCsv(bytes);
                
                if (list.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "No valid rows found in uploaded file. Expected columns: Name, Dosage, Frequency, MealTiming, Duration.";
                    return response;
                }

                await _unitOfWork.MedicinesRepository.AddRange(list);
                await _unitOfWork.Complete();
                response.IsSuccess = true;
                response.Message = $"{list.Count} medicines created successfully.";
                return response;
            }
            if (request.Medicine != null)
            {
                var entity = new Medicines
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Medicine.Name,
                    Dosage = request.Medicine.Dosage,
                    Frequency = request.Medicine.Frequency,
                    MealTiming = request.Medicine.MealTiming,
                    Duration = request.Medicine.Duration,
                    CreatedAt = _commonService.GetCurrentIndianTime()
                };
                await _unitOfWork.MedicinesRepository.Add(entity);
                await _unitOfWork.Complete();
                response.IsSuccess = true;
                response.Message = "Medicine created successfully.";
                return response;
            }

            response.IsSuccess = false;
            response.Message = "No data provided.";
            return response;

        }

        public async Task<ResponseObj<string>> UpdateMedicine(MedicinesRequestModel request, string id)
        {
            var response = new ResponseObj<string>();
            var existing = await _unitOfWork.MedicinesRepository.GetWithTracking().Where(m => m.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Medicine not found.";
                return response;
            }

            if (!string.IsNullOrEmpty(request.ExcelBase64))
            {
                var bytes = Convert.FromBase64String(request.ExcelBase64);
                var list = IsExcelFile(bytes)
                    ? ParseMedicinesFromExcel(bytes)
                    : ParseMedicinesFromCsv(bytes);

                if (list.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "No valid rows found in uploaded file. Expected columns: Name, Dosage, Frequency, MealTiming, Duration.";
                    return response;
                }

                var fromFile = list[0];
                existing.Name = fromFile.Name;
                existing.Dosage = fromFile.Dosage;
                existing.Frequency = fromFile.Frequency;
                existing.MealTiming = fromFile.MealTiming;
                existing.Duration = fromFile.Duration;
            }
            else if (request.Medicine != null)
            {
                existing.Name = request.Medicine.Name;
                existing.Dosage = request.Medicine.Dosage;
                existing.Frequency = request.Medicine.Frequency;
                existing.MealTiming = request.Medicine.MealTiming;
                existing.Duration = request.Medicine.Duration;
            }

            await _unitOfWork.MedicinesRepository.Update(existing);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "Medicine updated successfully.";
            return response;
        }

        public async Task<ResponseObj<string>> DeleteMedicine(string id)
        {
            var response = new ResponseObj<string>();
            var existing = await _unitOfWork.MedicinesRepository.GetWithTracking().Where(m => m.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Medicine not found.";
                return response;
            }
            await _unitOfWork.MedicinesRepository.Remove(existing);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "Medicine deleted successfully.";
            return response;
        }
        public async Task<ResponseObj<MedicinesDto>> GetMedicineById(string id)
        {
            var response = new ResponseObj<MedicinesDto>();
            var existing = await _unitOfWork.MedicinesRepository.GetWithoutTracking().Where(m => m.Id == id)
                .Select(m => new MedicinesDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    MealTiming = m.MealTiming,
                    Duration = m.Duration,
                    CreatedAt = m.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (existing == null)
            {
                response.IsSuccess = false;
                response.Message = "Medicine not found.";
                return response;
            }
            response.IsSuccess = true;
            response.Data = existing;
            return response;
        }
        public async Task<ResponseObj<List<MedicinesDto>>> GetAllMedicines()
        {
            var response = new ResponseObj<List<MedicinesDto>>();
            var list = await _unitOfWork.MedicinesRepository.GetWithoutTracking()
                .Select(m => new MedicinesDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    MealTiming = m.MealTiming,
                    Duration = m.Duration,
                    CreatedAt = m.CreatedAt
                })
                .OrderByDescending(m => m.CreatedAt).ToListAsync();
            response.IsSuccess = true;
            response.Data = list;
            return response;
        }
        public async Task<ResponseObj<List<MedicinesDto>>> GetSearchedMedicines(string? searchTerm)
        {
            var response = new ResponseObj<List<MedicinesDto>>();
            var list = await _unitOfWork.MedicinesRepository.GetWithoutTracking()
                .Where(m => m.Name.StartsWith(searchTerm))
                .Select(m => new MedicinesDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Dosage = m.Dosage,
                    Frequency = m.Frequency,
                    MealTiming = m.MealTiming,
                    Duration = m.Duration,
                    CreatedAt = m.CreatedAt
                })
                .OrderByDescending(m => m.CreatedAt).ToListAsync();
            response.IsSuccess = true;
            response.Data = list;
            return response;
        }

        private static bool IsExcelFile(byte[] bytes)
            => bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B;

        private static List<Medicines> ParseMedicinesFromExcel(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Workbook contains no worksheets.");

            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
                return [];

            var rows = usedRange.RowsUsed().ToList();
            if (rows.Count < 2)
                return [];

            var columnMap = BuildColumnMap(rows[0]);
            if (!columnMap.ContainsKey("Name"))
                throw new InvalidOperationException("Missing required column: Name.");

            var list = new List<Medicines>();
            foreach (var row in rows.Skip(1))
            {
                var name = GetCellText(row, columnMap, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                list.Add(new Medicines
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Dosage = GetCellText(row, columnMap, "Dosage"),
                    Frequency = GetCellInt(row, columnMap, "Frequency"),
                    MealTiming = GetCellText(row, columnMap, "MealTiming"),
                    Duration = GetCellText(row, columnMap, "Duration"),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return list;
        }

        private static Dictionary<string, int> BuildColumnMap(IXLRangeRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var header = cell.GetString().Trim();
                if (!string.IsNullOrEmpty(header))
                    map[header] = cell.Address.ColumnNumber;
            }
            return map;
        }

        private static string? GetCellText(IXLRangeRow row, Dictionary<string, int> columnMap, string columnName)
        {
            if (!columnMap.TryGetValue(columnName, out var col))
                return null;

            var cell = row.Cell(col);
            if (cell.IsEmpty())
                return null;

            return cell.GetFormattedString().Trim();
        }

        private static int GetCellInt(IXLRangeRow row, Dictionary<string, int> columnMap, string columnName)
        {
            if (!columnMap.TryGetValue(columnName, out var col))
                return 0;

            var cell = row.Cell(col);
            if (cell.IsEmpty())
                return 0;

            if (cell.TryGetValue(out int value))
                return value;
            if (cell.TryGetValue(out double number))
                return (int)number;

            return int.TryParse(cell.GetFormattedString().Trim(), out var parsed) ? parsed : 0;
        }

        private static List<Medicines> ParseMedicinesFromCsv(byte[] bytes)
        {
            var content = Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
            var list = new List<Medicines>();
            using var reader = new StringReader(content);
            string? line;
            var isFirstRow = true;
            Dictionary<string, int>? columnMap = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',').Select(p => p.Trim()).ToArray();
                if (isFirstRow)
                {
                    isFirstRow = false;
                    if (parts.Length > 0 && parts[0].Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        columnMap = parts
                            .Select((header, index) => new { header, index })
                            .Where(x => !string.IsNullOrWhiteSpace(x.header))
                            .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
                        continue;
                    }
                }

                string? GetPart(string columnName, int fallbackIndex)
                {
                    if (columnMap != null && columnMap.TryGetValue(columnName, out var index) && index < parts.Length)
                        return parts[index];
                    return fallbackIndex < parts.Length ? parts[fallbackIndex] : null;
                }

                var name = GetPart("Name", 0);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var frequencyText = GetPart("Frequency", 2);
                _ = int.TryParse(frequencyText, out var frequency);

                list.Add(new Medicines
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Dosage = GetPart("Dosage", 1),
                    Frequency = frequency,
                    MealTiming = GetPart("MealTiming", 3),
                    Duration = GetPart("Duration", 4),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return list;
        }
    }
}
