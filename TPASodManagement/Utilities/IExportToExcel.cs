namespace TpaSodManagement.Utilities
{
    public interface IExportToExcel
    {
        MemoryStream GenerateExcel<T>(
            string moduleName,
            string worksheetName,
            List<string> columnHeaders,
            List<T> data,
            Func<T, List<object>> rowMapper);
    }
}

