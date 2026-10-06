/*
    CXO Dashboard proof pack
    Portfolio Revenue Mix + Cost & budget variance by project

    Run on W26_Presales.
    Set the two project names, then run the whole script.
    Result set 1 = one row per project per calendar filter (paste into Excel).
    Result set 2 = profitability rows behind the mix revenue.
    Result set 3 = resource rows behind planned cost.
    Result set 4 = timesheet rows behind actual cost.

    Calendar dates come from usp_Whizible2_Sel_AnalyticsDBFilterDateRange.
    Custom is skipped because it needs dates you type on the page.
*/
USE [W26_Presales];
GO

DECLARE @ProjectName1 NVARCHAR(200) = N'CXO_Project_Alpha';
DECLARE @ProjectName2 NVARCHAR(200) = N'';   -- put the second project name here
DECLARE @DashboardID  INT = 21036;

IF OBJECT_ID('tempdb..#Projects') IS NOT NULL DROP TABLE #Projects;
IF OBJECT_ID('tempdb..#Ranges') IS NOT NULL DROP TABLE #Ranges;
IF OBJECT_ID('tempdb..#FilterNames') IS NOT NULL DROP TABLE #FilterNames;
IF OBJECT_ID('tempdb..#OneRange') IS NOT NULL DROP TABLE #OneRange;

SELECT
    P.ProjectID,
    P.ProjectName,
    ISNULL(P.ProjectGroupID, 0) AS PortfolioID,
    CASE
        WHEN ISNULL(P.ProjectGroupID, 0) = 0 THEN N'Unmapped'
        ELSE ISNULL(NULLIF(LTRIM(RTRIM(PG.ProjectGroupName)), ''), N'Unmapped')
    END AS PortfolioName,
    P.ContractType,
    ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency) AS ProjectCurrencyID
INTO #Projects
FROM Tbl_PM_Project P WITH (NOLOCK)
LEFT JOIN Tbl_PM_ProjectGroup PG WITH (NOLOCK)
    ON PG.ProjectGroupID = P.ProjectGroupID
WHERE P.ProjectName IN (@ProjectName1, @ProjectName2);

IF NOT EXISTS (SELECT 1 FROM #Projects)
BEGIN
    RAISERROR('Neither project name was found in Tbl_PM_Project. Update @ProjectName1 and @ProjectName2.', 16, 1);
    RETURN;
END;

CREATE TABLE #Ranges
(
    FilterName        NVARCHAR(200),
    CurrentStartDate  DATETIME,
    CurrentEndDate    DATETIME
);

CREATE TABLE #OneRange
(
    FilterID            INT,
    FilterName          NVARCHAR(200),
    GetDate             DATETIME,
    CurrentStartDate    DATETIME,
    CurrentEndDate      DATETIME,
    PreviousStartDate   DATETIME,
    PreviousEndDate     DATETIME
);

SELECT FilterName
INTO #FilterNames
FROM tbl_Whizible2_AnalyticsDBFilter WITH (NOLOCK)
WHERE ISNULL(IsComparison, 0) = 0
  AND ISNULL(IsActive, 1) = 1
  AND FilterName <> 'Custom'
ORDER BY DisplayOrder;

DECLARE @FilterName NVARCHAR(200);

DECLARE filter_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT FilterName FROM #FilterNames;

OPEN filter_cursor;
FETCH NEXT FROM filter_cursor INTO @FilterName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DELETE FROM #OneRange;

    INSERT INTO #OneRange
    EXEC dbo.usp_Whizible2_Sel_AnalyticsDBFilterDateRange
        @Flag = @FilterName,
        @StartDate = NULL,
        @EndDate = NULL,
        @DashboardID = @DashboardID;

    INSERT INTO #Ranges (FilterName, CurrentStartDate, CurrentEndDate)
    SELECT @FilterName, CurrentStartDate, CurrentEndDate
    FROM #OneRange;

    FETCH NEXT FROM filter_cursor INTO @FilterName;
END;

CLOSE filter_cursor;
DEALLOCATE filter_cursor;

DECLARE
    @BaseCurrencyID INT,
    @CurrencySymbol NVARCHAR(20),
    @CurrencyCode NVARCHAR(20),
    @IsIRConsiderForRevenue BIT = 0,
    @ProjectProfitabilityReportingFrequency INT = 1;

SELECT TOP 1
    @BaseCurrencyID = ISNULL(BaseCurrencyID, 1),
    @ProjectProfitabilityReportingFrequency = ISNULL(ProjectProfitabilityReportingFrequency, 1)
FROM Tbl_PM_CompanyInformation WITH (NOLOCK);

SELECT
    @CurrencySymbol = ISNULL(CurrencySymbol, N''),
    @CurrencyCode = ISNULL(CurrencyCode, N'')
FROM tbl_PM_CurrencyMaster WITH (NOLOCK)
WHERE CurrencyID = @BaseCurrencyID;

SELECT TOP 1
    @IsIRConsiderForRevenue = ISNULL(IsIRConsiderForRevenue, 0)
FROM dbo.Tbl_Whizible2_AnalyticsDashboardSetting WITH (NOLOCK)
WHERE IsActive = 1
ORDER BY SettingID DESC;

/* ---------- 1. Summary: what the UI shows, one row per filter ---------- */
;WITH RevenueRows AS
(
    SELECT
        R.FilterName,
        R.CurrentStartDate,
        R.CurrentEndDate,
        PR.ProjectID,
        SUM(CAST(
            (
                CASE
                    WHEN @IsIRConsiderForRevenue = 1
                        THEN ISNULL(PP.PeriodicInvoiceBillingTotal, 0)
                    WHEN PR.ContractType = 1
                        THEN ISNULL(PP.PeriodicMilestoneBilledAmount, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                    ELSE ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                END
            )
            * CASE
                WHEN PR.ProjectCurrencyID = @BaseCurrencyID THEN CAST(1.0 AS DECIMAL(18, 8))
                ELSE CAST(ISNULL(
                    NULLIF(CD.ConversionRate, 0),
                    ISNULL(NULLIF(CDFallback.ConversionRate, 0), ISNULL(NULLIF(CDInverse.InvRate, 0), 1.0))
                ) AS DECIMAL(18, 8))
              END
        AS DECIMAL(18, 4))) AS ProjectRevenue
    FROM #Ranges R
    INNER JOIN #Projects PR ON 1 = 1
    INNER JOIN Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
        ON PP.ProjectID = PR.ProjectID
       AND PP.ToDate >= R.CurrentStartDate
       AND PP.ToDate <= EOMONTH(R.CurrentEndDate)
       AND (
              (@ProjectProfitabilityReportingFrequency = 1 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 1 AND 7)
           OR (@ProjectProfitabilityReportingFrequency = 2 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 8 AND 31)
           OR (@ProjectProfitabilityReportingFrequency = 3 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 89 AND 92)
       )
    LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
        ON CD.CurrencyID = PR.ProjectCurrencyID
       AND CD.ConversionCurrencyID = @BaseCurrencyID
       AND PP.ToDate >= CD.FromDate
       AND PP.ToDate <= CD.ToDate
    OUTER APPLY
    (
        SELECT TOP 1 CD2.ConversionRate
        FROM tbl_PM_Currency_Detail CD2 WITH (NOLOCK)
        WHERE CD2.CurrencyID = PR.ProjectCurrencyID
          AND CD2.ConversionCurrencyID = @BaseCurrencyID
          AND ISNULL(CD2.ConversionRate, 0) <> 0
        ORDER BY
            CASE WHEN PP.ToDate >= CD2.FromDate AND PP.ToDate <= CD2.ToDate THEN 0 ELSE 1 END,
            CD2.ToDate DESC
    ) CDFallback
    OUTER APPLY
    (
        SELECT TOP 1
            CASE WHEN ISNULL(CD3.ConversionRate, 0) = 0 THEN NULL
                 ELSE CAST(1.0 AS DECIMAL(18, 8)) / CAST(CD3.ConversionRate AS DECIMAL(18, 8))
            END AS InvRate
        FROM tbl_PM_Currency_Detail CD3 WITH (NOLOCK)
        WHERE CD3.CurrencyID = @BaseCurrencyID
          AND CD3.ConversionCurrencyID = PR.ProjectCurrencyID
          AND ISNULL(CD3.ConversionRate, 0) <> 0
        ORDER BY
            CASE WHEN PP.ToDate >= CD3.FromDate AND PP.ToDate <= CD3.ToDate THEN 0 ELSE 1 END,
            CD3.ToDate DESC
    ) CDInverse
    GROUP BY R.FilterName, R.CurrentStartDate, R.CurrentEndDate, PR.ProjectID
),
PlannedResource AS
(
    SELECT
        R.FilterName,
        PER.ProjectID,
        SUM(CAST(
            ISNULL(dbo.fn_Whizible2_GetWorkingDays_New(
                CASE WHEN PER.ExpectedStartDate > R.CurrentStartDate THEN PER.ExpectedStartDate ELSE R.CurrentStartDate END,
                CASE WHEN PER.ExpectedEndDate < R.CurrentEndDate THEN PER.ExpectedEndDate ELSE R.CurrentEndDate END,
                E.LocationID
            ), 0)
            * ISNULL(L.WorkingHours, 0)
            * ISNULL(PER.ResourcePercentage, 0) / 100.0
            * ISNULL(E.CostPerHour, 0)
        AS DECIMAL(18, 2))) AS PlannedResourceCost
    FROM #Ranges R
    INNER JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK)
        ON PER.ExpectedStartDate <= R.CurrentEndDate
       AND PER.ExpectedEndDate >= R.CurrentStartDate
    INNER JOIN #Projects PR ON PR.ProjectID = PER.ProjectID
    INNER JOIN Tbl_PM_Employee E WITH (NOLOCK) ON E.EmployeeID = PER.EmployeeID
    LEFT JOIN tbl_PM_Location L WITH (NOLOCK) ON L.LocationID = E.LocationID
    WHERE ISNULL(L.Active, 1) = 1
    GROUP BY R.FilterName, PER.ProjectID
),
CostHead AS
(
    SELECT
        PWC.ProjectID,
        SUM(ISNULL(PWC.CostToCompany, 0)) AS CostHeadBudget
    FROM tbl_PM_WorkOrderCosts PWC WITH (NOLOCK)
    INNER JOIN #Projects PR ON PR.ProjectID = PWC.ProjectID
    WHERE ISNULL(PWC.IsActive, 1) = 1
    GROUP BY PWC.ProjectID
),
Actuals AS
(
    SELECT
        R.FilterName,
        DA.ProjectID,
        SUM(CAST(ISNULL(DA.Duration, 0) * ISNULL(E.CostPerHour, 0) AS DECIMAL(18, 2))) AS ActualCost
    FROM #Ranges R
    INNER JOIN tbl_PM_DailyActivity DA WITH (NOLOCK)
        ON DA.EntryDate >= R.CurrentStartDate
       AND DA.EntryDate <= R.CurrentEndDate
    INNER JOIN #Projects PR ON PR.ProjectID = DA.ProjectID
    INNER JOIN Tbl_PM_Employee E WITH (NOLOCK) ON E.EmployeeID = DA.EmployeeID
    GROUP BY R.FilterName, DA.ProjectID
)
SELECT
    R.FilterName AS CalendarFilter,
    CONVERT(VARCHAR(10), R.CurrentStartDate, 23) AS FromDate,
    CONVERT(VARCHAR(10), R.CurrentEndDate, 23) AS ToDate,
    PR.ProjectID,
    PR.ProjectName,
    PR.PortfolioName,
    @CurrencySymbol AS CurrencySymbol,
    @CurrencyCode AS CurrencyCode,
    CAST(ISNULL(RV.ProjectRevenue, 0) AS DECIMAL(18, 2)) AS MixRevenue_BaseCurrency,
    CAST(ISNULL(PL.PlannedResourceCost, 0) AS DECIMAL(18, 2)) AS PlannedResourceCost,
    CAST(ISNULL(CH.CostHeadBudget, 0) AS DECIMAL(18, 2)) AS CostHeadBudget,
    CAST(ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0) AS DECIMAL(18, 2)) AS PlannedBudget,
    CAST(ISNULL(AC.ActualCost, 0) AS DECIMAL(18, 2)) AS ActualCost,
    CAST(
        CASE
            WHEN ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0) = 0 THEN 0
            ELSE ISNULL(AC.ActualCost, 0)
                 / (ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0)) * 100.0
        END
    AS DECIMAL(18, 2)) AS BudgetBurnPct,
    CAST(
        CASE
            WHEN ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0) = 0 THEN 0
            ELSE (ISNULL(AC.ActualCost, 0) - (ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0)))
                 / (ISNULL(PL.PlannedResourceCost, 0) + ISNULL(CH.CostHeadBudget, 0)) * 100.0
        END
    AS DECIMAL(18, 2)) AS BudgetVariancePct
FROM #Ranges R
CROSS JOIN #Projects PR
LEFT JOIN RevenueRows RV
    ON RV.FilterName = R.FilterName AND RV.ProjectID = PR.ProjectID
LEFT JOIN PlannedResource PL
    ON PL.FilterName = R.FilterName AND PL.ProjectID = PR.ProjectID
LEFT JOIN CostHead CH
    ON CH.ProjectID = PR.ProjectID
LEFT JOIN Actuals AC
    ON AC.FilterName = R.FilterName AND AC.ProjectID = PR.ProjectID
ORDER BY R.CurrentStartDate, PR.ProjectName;

/* ---------- 2. Profitability rows used by Portfolio Revenue Mix ---------- */
SELECT
    R.FilterName AS CalendarFilter,
    PR.ProjectName,
    PP.FromDate,
    PP.ToDate,
    CASE
        WHEN @IsIRConsiderForRevenue = 1 THEN 'Invoice billing'
        WHEN PR.ContractType = 1 THEN 'Milestone + billable other cost'
        ELSE 'Accrued resource billing + billable other cost'
    END AS RevenueRule,
    CASE
        WHEN @IsIRConsiderForRevenue = 1 THEN ISNULL(PP.PeriodicInvoiceBillingTotal, 0)
        WHEN PR.ContractType = 1 THEN ISNULL(PP.PeriodicMilestoneBilledAmount, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
        ELSE ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
    END AS RawRevenue
FROM #Ranges R
INNER JOIN #Projects PR ON 1 = 1
INNER JOIN Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
    ON PP.ProjectID = PR.ProjectID
   AND PP.ToDate >= R.CurrentStartDate
   AND PP.ToDate <= EOMONTH(R.CurrentEndDate)
   AND (
          (@ProjectProfitabilityReportingFrequency = 1 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 1 AND 7)
       OR (@ProjectProfitabilityReportingFrequency = 2 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 8 AND 31)
       OR (@ProjectProfitabilityReportingFrequency = 3 AND DATEDIFF(DAY, PP.FromDate, PP.ToDate) + 1 BETWEEN 89 AND 92)
   )
ORDER BY R.CurrentStartDate, PR.ProjectName, PP.ToDate;

/* ---------- 3. Resource rows used by planned cost ---------- */
SELECT
    R.FilterName AS CalendarFilter,
    PR.ProjectName,
    E.EmployeeName,
    PER.ExpectedStartDate,
    PER.ExpectedEndDate,
    CASE WHEN PER.ExpectedStartDate > R.CurrentStartDate THEN PER.ExpectedStartDate ELSE R.CurrentStartDate END AS OverlapStart,
    CASE WHEN PER.ExpectedEndDate < R.CurrentEndDate THEN PER.ExpectedEndDate ELSE R.CurrentEndDate END AS OverlapEnd,
    dbo.fn_Whizible2_GetWorkingDays_New(
        CASE WHEN PER.ExpectedStartDate > R.CurrentStartDate THEN PER.ExpectedStartDate ELSE R.CurrentStartDate END,
        CASE WHEN PER.ExpectedEndDate < R.CurrentEndDate THEN PER.ExpectedEndDate ELSE R.CurrentEndDate END,
        E.LocationID
    ) AS WorkingDays,
    L.WorkingHours,
    PER.ResourcePercentage,
    E.CostPerHour
FROM #Ranges R
INNER JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK)
    ON PER.ExpectedStartDate <= R.CurrentEndDate
   AND PER.ExpectedEndDate >= R.CurrentStartDate
INNER JOIN #Projects PR ON PR.ProjectID = PER.ProjectID
INNER JOIN Tbl_PM_Employee E WITH (NOLOCK) ON E.EmployeeID = PER.EmployeeID
LEFT JOIN tbl_PM_Location L WITH (NOLOCK) ON L.LocationID = E.LocationID
WHERE ISNULL(L.Active, 1) = 1
ORDER BY R.CurrentStartDate, PR.ProjectName, E.EmployeeName;

/* ---------- 4. Timesheet rows used by actual cost ---------- */
SELECT
    R.FilterName AS CalendarFilter,
    PR.ProjectName,
    E.EmployeeName,
    DA.EntryDate,
    DA.Duration,
    E.CostPerHour,
    CAST(ISNULL(DA.Duration, 0) * ISNULL(E.CostPerHour, 0) AS DECIMAL(18, 2)) AS LineCost
FROM #Ranges R
INNER JOIN tbl_PM_DailyActivity DA WITH (NOLOCK)
    ON DA.EntryDate >= R.CurrentStartDate
   AND DA.EntryDate <= R.CurrentEndDate
INNER JOIN #Projects PR ON PR.ProjectID = DA.ProjectID
INNER JOIN Tbl_PM_Employee E WITH (NOLOCK) ON E.EmployeeID = DA.EmployeeID
ORDER BY R.CurrentStartDate, PR.ProjectName, DA.EntryDate, E.EmployeeName;
