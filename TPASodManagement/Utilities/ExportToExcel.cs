using OfficeOpenXml;
using TpaSodManagement.Utilities;

namespace TpaSodManagement.Utilities
{
    public class ExportToExcel : IExportToExcel
    {
        public MemoryStream GenerateExcel<T>(
            string moduleName,
            string worksheetName,
            List<string> columnHeaders,
            List<T> data,
            Func<T, List<object>> rowMapper)
        {
            // Set EPPlus license context (non-commercial use)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Generate Excel file using EPPlus
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Set header row
            for (int col = 0; col < columnHeaders.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = columnHeaders[col];
            }

            // Style header row
            using (var range = worksheet.Cells[1, 1, 1, columnHeaders.Count])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // Add data rows
            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var item = data[i];
                var rowValues = rowMapper(item);

                for (int col = 0; col < rowValues.Count; col++)
                {
                    worksheet.Cells[row, col + 1].Value = rowValues[col] ?? "";
                }
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Add borders to data cells
            if (data.Count > 0)
            {
                using (var range = worksheet.Cells[1, 1, data.Count + 1, columnHeaders.Count])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            
            // Dispose package after saving to stream
            package.Dispose();

            return stream;
        }
    }
}

