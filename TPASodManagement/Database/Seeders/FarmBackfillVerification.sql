-- Organization -> Farm transitional backfill verification
-- Rule validated: first active farm per OrganizationId (ORDER BY FarmId ASC)

-- 1) AspNetUsers: pending rows that still need FarmId
SELECT COUNT(*) AS PendingUsers
FROM AspNetUsers u
WHERE u.OrganizationId IS NOT NULL
  AND u.FarmId IS NULL;

-- 2) AspNetUsers: mismatches against deterministic first-farm mapping
WITH FirstFarm AS (
    SELECT f.OrganizationId, MIN(f.FarmId) AS FirstFarmId
    FROM Farms f
    WHERE f.DeletedDate IS NULL
    GROUP BY f.OrganizationId
)
SELECT TOP (100)
    u.Id,
    u.OrganizationId,
    u.FarmId AS CurrentFarmId,
    ff.FirstFarmId AS ExpectedFarmId
FROM AspNetUsers u
INNER JOIN FirstFarm ff ON ff.OrganizationId = u.OrganizationId
WHERE u.OrganizationId IS NOT NULL
  AND u.FarmId IS NOT NULL
  AND u.FarmId <> ff.FirstFarmId
ORDER BY u.Id;

-- 3) Customers: pending rows (works only if FarmId column exists)
IF COL_LENGTH('Customers', 'FarmId') IS NOT NULL
BEGIN
    SELECT COUNT(*) AS PendingCustomers
    FROM Customers c
    WHERE c.OrganizationId IS NOT NULL
      AND c.FarmId IS NULL;
END

-- 4) Customers: mismatches against deterministic first-farm mapping (when FarmId exists)
IF COL_LENGTH('Customers', 'FarmId') IS NOT NULL
BEGIN
    WITH FirstFarm AS (
        SELECT f.OrganizationId, MIN(f.FarmId) AS FirstFarmId
        FROM Farms f
        WHERE f.DeletedDate IS NULL
        GROUP BY f.OrganizationId
    )
    SELECT TOP (100)
        c.CustomerId,
        c.OrganizationId,
        c.FarmId AS CurrentFarmId,
        ff.FirstFarmId AS ExpectedFarmId
    FROM Customers c
    INNER JOIN FirstFarm ff ON ff.OrganizationId = c.OrganizationId
    WHERE c.OrganizationId IS NOT NULL
      AND c.FarmId IS NOT NULL
      AND c.FarmId <> ff.FirstFarmId
    ORDER BY c.CustomerId;
END
