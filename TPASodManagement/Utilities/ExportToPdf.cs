using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TpaSodManagement.Utilities;

public class ExportToPdf : IExportToPdf
{
    public MemoryStream GeneratePdf<T>(
        string moduleName,
        List<string> columnHeaders,
        List<T> data,
        Func<T, List<object>> rowMapper,
        byte[]? headerImageBytes = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        bool useCapturedHeader = headerImageBytes != null && headerImageBytes.Length > 0;

        var stream = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(0); // No margin so header can attach to page corners

                page.Header().Column(headerCol =>
                {
                    if (useCapturedHeader)
                        headerCol.Item().ShowOnce().Image(headerImageBytes!).FitWidth();
                    else
                        headerCol.Item().Text(moduleName).Bold().FontSize(14);
                });

                page.Content().Padding(20).PaddingVertical(10).Column(contentCol =>
                {
                    contentCol.Item().PaddingBottom(8).Text(moduleName).Bold().FontSize(14);
                    contentCol.Item().Table(table =>
                {
                    var colCount = columnHeaders.Count;
                    table.ColumnsDefinition(columns =>
                    {
                        for (int i = 0; i < colCount; i++)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var h in columnHeaders)
                            header.Cell().BorderBottom(1).Background(Colors.Grey.Lighten3).Padding(6).Text(h).Bold().FontSize(9);
                    });

                    foreach (var item in data)
                    {
                        var values = rowMapper(item);
                        foreach (var v in values)
                        {
                            var text = v?.ToString() ?? "";
                            if (text.Length > 80) text = text.Substring(0, 77) + "...";
                            table.Cell().BorderBottom(0.5f).Padding(5).Text(text).FontSize(8);
                        }
                    }
                });
                });
            });
        }).GeneratePdf(stream);

        stream.Position = 0;
        return stream;
    }
}
