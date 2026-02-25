namespace TpaSodManagement.Utilities
{
    public interface IExportToPdf
    {
        MemoryStream GeneratePdf<T>(
            string moduleName,
            List<string> columnHeaders,
            List<T> data,
            Func<T, List<object>> rowMapper);
    }
}
