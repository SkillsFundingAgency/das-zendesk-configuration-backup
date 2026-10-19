
using ClosedXML.Excel;
using System.Text.Json;
using ZendeskConfigBackup.Helpers;

namespace ZendeskConfigBackup.Services;

/// <summary>
/// Creates Excel worksheets
/// Populates cells with flattened JSON data
/// formatted for readability
/// saves the workbook
/// </summary>
public class ExcelExportService : IDisposable
{
    private readonly XLWorkbook _workbook = new XLWorkbook();

    //Creates and populates a worksheet with flattened JSON data
    public void AddSheet(string sheetName, List<JsonElement> records)
    {
        //Create a new worksheet with the specified name
        var sheet = _workbook.Worksheets.Add(sheetName);

        //Flatten all records using the JsonFlattener helper class
        var flattenedRecords = records
            .Select(JsonFlattener.Flatten)
            .ToList();

        //Identify all unique property names across all flattened records to use as column headers
        var columns = flattenedRecords
            .SelectMany(record => record.Keys)
            .Distinct()
            .ToList();

        //Column headers
        for (int col = 0; col < columns.Count; col++)
        {
            sheet.Cell(1, col + 1).Value = columns[col];
        }

        //Data writing loop: writes each flattened record to the corresponding row in the worksheet
        for (int row = 0; row < flattenedRecords.Count; row++)
        {
            var record = flattenedRecords[row];

            for (int col = 0; col < columns.Count; col++)
            {
                string property = columns[col];
                // Check if the property exists in the record and write its value to the cell
                if (record.TryGetValue(property, out string? value))
                {
                    // Excel has a limit of 32,767 characters per cell. If the value exceeds this limit, throw an exception.
                    if (value.Length > 32767)
                        throw new InvalidOperationException(
                            $"Excel cell limit exceeded in {sheetName}, " +
                            $"row {row + 2}, property {property}");
                    sheet.Cell(row + 2, col + 1).Value = value;
                }
            }
        }

        // Formatting
        // Bold headers, freeze the first row, and wrap text in all columns
        sheet.Row(1).Style.Font.Bold = true;
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().Style.Alignment.WrapText = true;
        // Adjust column widths to fit content, but limit to a maximum width of 50
        if (columns.Count > 0)
        {
            sheet.Columns(1, columns.Count).AdjustToContents();
            // Limit column width to a maximum of 50
            foreach (var column in sheet.Columns(1, columns.Count))
            {
                if (column.Width > 50)
                    column.Width = 50;
            }
        }

        Console.WriteLine(
            $"{sheetName}: {records.Count} records added.");
    }
    // Saves the workbook to the specified filename
    public void Save(string filename)
    {
        _workbook.SaveAs(filename);
        Console.WriteLine($"Excel created: {filename}");
    }

    // Dispose of the workbook when done
    public void Dispose()
    {
        _workbook.Dispose();
    }
}
