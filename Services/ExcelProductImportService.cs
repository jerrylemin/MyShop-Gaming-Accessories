using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using ProjectTest.DataAccess;
using ProjectTest.DataAccess.Seeding;
using ProjectTest.Models;

namespace ProjectTest.Services;

public class ExcelProductImportService
{
    private readonly MyShopDbContextFactory _dbContextFactory;

    public ExcelProductImportService(MyShopDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<OperationResult<ProductImportSummary>> ImportAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return OperationResult<ProductImportSummary>.Fail("Excel file not found.");
        }

        var rows = ReadRows(filePath);
        if (rows.Count == 0)
        {
            return OperationResult<ProductImportSummary>.Fail("The selected workbook does not contain any product rows.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var summary = new ProductImportSummary();
        var categories = await dbContext.Categories
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (!categories.TryGetValue(row.CategoryName, out var category))
            {
                category = new Category
                {
                    Name = row.CategoryName,
                    Description = $"{row.CategoryName} products imported from Excel."
                };

                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
                categories[category.Name] = category;
                summary.CategoryCount += 1;
            }

            var existing = await dbContext.Products.FirstOrDefaultAsync(x => x.SKU == row.SKU);
            if (existing is null)
            {
                existing = new Product();
                dbContext.Products.Add(existing);
                summary.CreatedCount += 1;
            }
            else
            {
                summary.UpdatedCount += 1;
            }

            existing.SKU = row.SKU;
            existing.Name = row.Name;
            existing.Manufacturer = row.Manufacturer;
            existing.CPU = row.CPU;
            existing.RAM = row.RAM;
            existing.Storage = row.Storage;
            existing.GPU = row.GPU;
            existing.Screen = row.Screen;
            existing.ImportPrice = row.ImportPrice;
            existing.SalePrice = row.SalePrice;
            existing.Stock = row.Stock;
            existing.CategoryId = category.Id;
            existing.Description = row.Description;

            await dbContext.SaveChangesAsync();

            existing.Image1 = string.IsNullOrWhiteSpace(row.Image1) ? GamingAccessorySeedGenerator.BuildImagePath(existing.Id, 1) : row.Image1;
            existing.Image2 = string.IsNullOrWhiteSpace(row.Image2) ? GamingAccessorySeedGenerator.BuildImagePath(existing.Id, 2) : row.Image2;
            existing.Image3 = string.IsNullOrWhiteSpace(row.Image3) ? GamingAccessorySeedGenerator.BuildImagePath(existing.Id, 3) : row.Image3;
        }

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return OperationResult<ProductImportSummary>.Ok(summary, $"Imported {summary.TotalCount} product rows from Excel.");
    }

    private static List<ImportedProductRow> ReadRows(string filePath)
    {
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Workbook part is missing.");
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
        var firstWorksheet = workbookPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook does not contain a worksheet.");

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstWorksheet.Id!);
        var rows = worksheetPart.Worksheet.Descendants<Row>().ToList();
        if (rows.Count <= 1)
        {
            return [];
        }

        var headerMap = BuildHeaderMap(rows[0], sharedStrings);
        var importedRows = new List<ImportedProductRow>();

        foreach (var row in rows.Skip(1))
        {
            var values = ReadRowValues(row, sharedStrings);
            if (!TryGetValue(headerMap, values, "SKU", out var sku) || string.IsNullOrWhiteSpace(sku))
            {
                continue;
            }

            importedRows.Add(new ImportedProductRow
            {
                SKU = sku.Trim(),
                Name = GetValue(headerMap, values, "Name"),
                CategoryName = GetValue(headerMap, values, "Category", "Gaming Keyboard"),
                Manufacturer = GetValue(headerMap, values, "Manufacturer"),
                CPU = GetValue(headerMap, values, "CPU"),
                RAM = GetValue(headerMap, values, "RAM"),
                Storage = GetValue(headerMap, values, "Storage"),
                GPU = GetValue(headerMap, values, "GPU"),
                Screen = GetValue(headerMap, values, "Screen"),
                ImportPrice = GetDecimalValue(headerMap, values, "ImportPrice"),
                SalePrice = GetDecimalValue(headerMap, values, "SalePrice"),
                Stock = GetIntValue(headerMap, values, "Stock"),
                Description = GetValue(headerMap, values, "Description"),
                Image1 = GetValue(headerMap, values, "Image1"),
                Image2 = GetValue(headerMap, values, "Image2"),
                Image3 = GetValue(headerMap, values, "Image3")
            });
        }

        return importedRows;
    }

    private static Dictionary<string, int> BuildHeaderMap(Row headerRow, SharedStringTable? sharedStrings)
    {
        var headers = ReadRowValues(headerRow, sharedStrings);
        return headers
            .Select((value, index) => new { value, index })
            .Where(x => !string.IsNullOrWhiteSpace(x.value))
            .ToDictionary(x => x.value.Trim(), x => x.index, StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> ReadRowValues(Row row, SharedStringTable? sharedStrings)
    {
        var values = new List<string>();
        var currentIndex = 0;

        foreach (var cell in row.Elements<Cell>())
        {
            var cellIndex = GetCellIndex(cell.CellReference?.Value);
            while (currentIndex < cellIndex)
            {
                values.Add(string.Empty);
                currentIndex += 1;
            }

            values.Add(GetCellValue(cell, sharedStrings));
            currentIndex += 1;
        }

        return values;
    }

    private static int GetCellIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return 0;
        }

        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var index = 0;
        foreach (var letter in letters)
        {
            index = (index * 26) + (char.ToUpperInvariant(letter) - 'A' + 1);
        }

        return Math.Max(0, index - 1);
    }

    private static string GetCellValue(Cell cell, SharedStringTable? sharedStrings)
    {
        var rawValue = cell.CellValue?.InnerText ?? cell.InnerText ?? string.Empty;
        if (cell.DataType?.Value == CellValues.SharedString && int.TryParse(rawValue, out var sharedStringIndex))
        {
            return sharedStrings?.ElementAt(sharedStringIndex).InnerText ?? string.Empty;
        }

        return rawValue;
    }

    private static bool TryGetValue(Dictionary<string, int> headerMap, IReadOnlyList<string> values, string header, out string value)
    {
        value = GetValue(headerMap, values, header);
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string GetValue(Dictionary<string, int> headerMap, IReadOnlyList<string> values, string header, string fallback = "")
    {
        if (headerMap.TryGetValue(header, out var index) && index < values.Count)
        {
            return values[index].Trim();
        }

        return fallback;
    }

    private static decimal GetDecimalValue(Dictionary<string, int> headerMap, IReadOnlyList<string> values, string header)
    {
        return decimal.TryParse(GetValue(headerMap, values, header), out var parsed) ? parsed : 0m;
    }

    private static int GetIntValue(Dictionary<string, int> headerMap, IReadOnlyList<string> values, string header)
    {
        return int.TryParse(GetValue(headerMap, values, header), out var parsed) ? parsed : 0;
    }

    private sealed class ImportedProductRow
    {
        public string SKU { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;

        public string CPU { get; set; } = string.Empty;

        public string RAM { get; set; } = string.Empty;

        public string Storage { get; set; } = string.Empty;

        public string GPU { get; set; } = string.Empty;

        public string Screen { get; set; } = string.Empty;

        public decimal ImportPrice { get; set; }

        public decimal SalePrice { get; set; }

        public int Stock { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Image1 { get; set; } = string.Empty;

        public string Image2 { get; set; } = string.Empty;

        public string Image3 { get; set; } = string.Empty;
    }
}
