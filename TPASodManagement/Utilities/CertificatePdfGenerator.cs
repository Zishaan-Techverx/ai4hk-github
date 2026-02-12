using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using TpaSodManagement.ViewModels.Sale;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace TpaSodManagement.Utilities;

public static class CertificatePdfGenerator
{
    /// <summary>
    /// Generates PDF bytes for the certificate: image as background with text overlaid at same positions as preview.
    /// </summary>
    public static byte[] Generate(SaleCertificateViewModel vm, byte[] certificateImageBytes)
    {
        using var image = ImageSharpImage.Load(certificateImageBytes);
        int w = image.Width;
        int h = image.Height;
        // 96 DPI: 1 inch = 96 px = 72 pt, so 1 px = 0.75 pt
        float pageWidthPt = w * (72f / 96f);
        float pageHeightPt = h * (72f / 96f);

        // Same positions as in Views/Sale/Certificate.cshtml (percentages)
        float LeftPt(float leftPercent) => pageWidthPt * (leftPercent / 100f);
        float TopPt(float topPercent) => pageHeightPt * (topPercent / 100f);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(pageWidthPt, pageHeightPt));
                page.Margin(0);
                page.Content().Layers(layers =>
                {
                    // Background: full-page certificate image
                    layers.PrimaryLayer()
                        .Width(pageWidthPt)
                        .Height(pageHeightPt)
                        .Image(certificateImageBytes).FitArea();

                    // Overlays: text at same % positions as preview; regular (non-bold), small font to match certificate image
                    const float fontSize = 40f;

                    // 1. LICENSED GROWER: left 18%, top 33.5%, centered
                    layers.Layer()
                        .PaddingLeft(LeftPt(18f))
                        .PaddingTop(TopPt(33.5f))
                        .Width((float)(pageWidthPt - LeftPt(18f) - LeftPt(5f)))
                        .AlignCenter()
                        .Text(vm.LicensedGrower)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 1b. FARM ADDRESS: left 18%, top 38%, centered (below Licensed Grower)
                    layers.Layer()
                        .PaddingLeft(LeftPt(18f))
                        .PaddingTop(TopPt(38f))
                        .Width((float)(pageWidthPt - LeftPt(18f) - LeftPt(5f)))
                        .AlignCenter()
                        .Text(vm.FarmAddress)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 2. DATE CERTIFICATE ISSUED: left 50%, top 47.8%
                    layers.Layer()
                        .PaddingLeft(LeftPt(50f))
                        .PaddingTop(TopPt(47.8f))
                        .Text(vm.DateCertificateIssued)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 3. AREA SOLD: left 50%, top 52%
                    layers.Layer()
                        .PaddingLeft(LeftPt(50f))
                        .PaddingTop(TopPt(52f))
                        .Text(vm.AreaSold)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 4. INVOICE NUMBER(S): left 50%, top 55.8%
                    layers.Layer()
                        .PaddingLeft(LeftPt(50f))
                        .PaddingTop(TopPt(55.8f))
                        .Text(vm.InvoiceNumbers)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 5. CUSTOMER: left 48%, top 64%
                    layers.Layer()
                        .PaddingLeft(LeftPt(48f))
                        .PaddingTop(TopPt(64f))
                        .Text(vm.Customer)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    // 5b. CUSTOMER ADDRESS: left 48%, top 68% (below Customer name)
                    layers.Layer()
                        .PaddingLeft(LeftPt(42f))
                        .PaddingTop(TopPt(68f))
                        .Text(vm.CustomerAddress)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
