using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TpaSodManagement.Database;

#nullable disable

namespace TpaSodManagement.Database.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409152000_SaleFinancialAndMetadataFieldsNullable")]
    public partial class SaleFinancialAndMetadataFieldsNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @fkCurrency nvarchar(200);
DECLARE @fkStatus nvarchar(200);

SELECT TOP 1 @fkCurrency = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Sales' AND c.name = 'CurrencyId';

SELECT TOP 1 @fkStatus = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Sales' AND c.name = 'StatusId';

IF @fkCurrency IS NOT NULL EXEC('ALTER TABLE [Sales] DROP CONSTRAINT [' + @fkCurrency + ']');
IF @fkStatus IS NOT NULL EXEC('ALTER TABLE [Sales] DROP CONSTRAINT [' + @fkStatus + ']');

DECLARE @defaultSql nvarchar(max) = N'';
SELECT @defaultSql = @defaultSql + N'ALTER TABLE [Sales] DROP CONSTRAINT [' + dc.name + N'];'
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Sales'
  AND c.name IN ('SaleNumber', 'PurchaseOrderNumber', 'DueDate', 'SubtotalAmount', 'TaxAmount', 'DiscountAmount', 'TotalAmount', 'CurrencyId', 'PaymentTermsDays', 'StatusId');
IF LEN(@defaultSql) > 0 EXEC sp_executesql @defaultSql;

IF COL_LENGTH('Sales', 'SaleNumber') IS NOT NULL
BEGIN
    DECLARE @saleNumberType nvarchar(200);
    SELECT @saleNumberType = TYPE_NAME(c.user_type_id) +
        CASE
            WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','nchar') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length / 2 AS varchar(10)) END + ')'
            WHEN TYPE_NAME(c.user_type_id) IN ('varchar','char') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length AS varchar(10)) END + ')'
            ELSE ''
        END
    FROM sys.columns c
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Sales' AND c.name = 'SaleNumber';
    EXEC('ALTER TABLE [Sales] ALTER COLUMN [SaleNumber] ' + @saleNumberType + ' NULL');
END;

IF COL_LENGTH('Sales', 'PurchaseOrderNumber') IS NOT NULL
BEGIN
    DECLARE @poType nvarchar(200);
    SELECT @poType = TYPE_NAME(c.user_type_id) +
        CASE
            WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','nchar') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length / 2 AS varchar(10)) END + ')'
            WHEN TYPE_NAME(c.user_type_id) IN ('varchar','char') THEN '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CAST(c.max_length AS varchar(10)) END + ')'
            ELSE ''
        END
    FROM sys.columns c
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'Sales' AND c.name = 'PurchaseOrderNumber';
    EXEC('ALTER TABLE [Sales] ALTER COLUMN [PurchaseOrderNumber] ' + @poType + ' NULL');
END;

IF COL_LENGTH('Sales', 'DueDate') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [DueDate] date NULL;';

IF COL_LENGTH('Sales', 'SubtotalAmount') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [SubtotalAmount] decimal(18,2) NULL;';

IF COL_LENGTH('Sales', 'TaxAmount') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [TaxAmount] decimal(18,2) NULL;';

IF COL_LENGTH('Sales', 'DiscountAmount') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [DiscountAmount] decimal(18,2) NULL;';

IF COL_LENGTH('Sales', 'TotalAmount') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [TotalAmount] decimal(18,2) NULL;';

IF COL_LENGTH('Sales', 'CurrencyId') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [CurrencyId] int NULL;';

IF COL_LENGTH('Sales', 'PaymentTermsDays') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [PaymentTermsDays] int NULL;';

IF COL_LENGTH('Sales', 'StatusId') IS NOT NULL
    EXEC sp_executesql N'ALTER TABLE [Sales] ALTER COLUMN [StatusId] int NULL;';

IF COL_LENGTH('Sales', 'CurrencyId') IS NOT NULL AND OBJECT_ID('[Currencies]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sales_Currencies_CurrencyId')
BEGIN
    ALTER TABLE [Sales] WITH CHECK ADD CONSTRAINT [FK_Sales_Currencies_CurrencyId]
    FOREIGN KEY([CurrencyId]) REFERENCES [Currencies]([CurrencyId]) ON DELETE NO ACTION;
END;

IF COL_LENGTH('Sales', 'StatusId') IS NOT NULL AND OBJECT_ID('[Statuses]', 'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sales_Statuses_StatusId')
BEGIN
    ALTER TABLE [Sales] WITH CHECK ADD CONSTRAINT [FK_Sales_Statuses_StatusId]
    FOREIGN KEY([StatusId]) REFERENCES [Statuses]([StatusId]) ON DELETE NO ACTION;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no down-drop/tightening in transitional phase.
        }
    }
}
