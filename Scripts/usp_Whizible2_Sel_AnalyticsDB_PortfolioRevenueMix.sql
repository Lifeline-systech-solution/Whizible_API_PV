IF EXISTS
(
    SELECT [name] FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix]')
      AND type IN (N'P', N'PC')
)
DROP PROCEDURE [dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix]
/*
Project Name      : W27 Dashboard
Procedure Name    : usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix
Purpose           : Portfolio Revenue Mix drill-down.
Description       : Current-period revenue by Project Group.
                    Unit and scaled Revenue use the same money scale as
                    usp_Whizible2_Sel_AnalyticsDBKPI_Revenue
                    (udf_Whizible2_GetAnalyticsDBMoneyScale on the current total).
i/p param.        :
o/i param.        :
Assumptions       : None.
Dependencies      : udf_Whizible2_GetAnalyticsDBMoneyScale
Author            :
Created           : 23-09-2026
Reviewed          :
Revisions         : 24-09-2026 Aditya J. — return Unit the same way as the Revenue KPI.
*/
    @CurrentFromDate   DATETIME,
    @CurrentToDate     DATETIME,
    @PreviousFromDate  DATETIME = NULL,
    @PreviousToDate    DATETIME = NULL,
    @CustomerIDs       VARCHAR(MAX) = NULL,
    @ProjectManagerIDs VARCHAR(MAX) = NULL,
    @PortfolioIDs      VARCHAR(MAX) = NULL,
    @RegionIDs         VARCHAR(MAX) = NULL,
    @BillingTypeIDs    VARCHAR(MAX) = NULL,
    @HealthIDs         VARCHAR(MAX) = NULL,
    @DashboardID       INT          = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BaseCurrencyID INT, @CurrencySymbol VARCHAR(500), @CurrencyCode NVARCHAR(20);
    SELECT TOP 1 @BaseCurrencyID = ISNULL(BaseCurrencyID, 1) FROM Tbl_PM_CompanyInformation WITH (NOLOCK);
    SELECT @CurrencySymbol = CurrencySymbol, @CurrencyCode = CurrencyCode
    FROM tbl_PM_CurrencyMaster WHERE CurrencyID = @BaseCurrencyID;

    DECLARE @HasPortfolioFilter BIT = 0;
    IF (@PortfolioIDs IS NOT NULL AND LTRIM(RTRIM(@PortfolioIDs)) <> '')
        SET @HasPortfolioFilter = 1;

    ---------------------------------------------------------------------
    -- Current-period base matches usp_Whizible2_Sel_AnalyticsDBKPI_Revenue.
    -- Latest profitability row per project, then grouped by portfolio.
    ---------------------------------------------------------------------
    ;WITH BaseData AS
    (
        SELECT
            P.ProjectID,
            P.ProjectGroupID AS PortfolioID,
            PG.ProjectGroupName AS PortfolioName,
            CASE
                WHEN P.ContractType = 1
                    THEN ISNULL(PP.PeriodicMilestoneBilledAmount, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                ELSE ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
            END AS RawRevenue,
            CASE
                WHEN ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency) = @BaseCurrencyID
                    THEN CAST(1.0 AS DECIMAL(18, 8))
                ELSE CAST(ISNULL(
                    NULLIF(CD.ConversionRate, 0),
                    ISNULL(NULLIF(CDFallback.ConversionRate, 0), ISNULL(NULLIF(CDInverse.InvRate, 0), 1.0))
                ) AS DECIMAL(18, 8))
            END AS ExchangeRate,
            PP.ToDate AS RecordDate
        FROM Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
        INNER JOIN Tbl_PM_Project P WITH (NOLOCK) ON PP.ProjectID = P.ProjectID
        INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H ON H.ProjectID = P.ProjectID
        LEFT JOIN Tbl_PM_ProjectGroup PG WITH (NOLOCK) ON P.ProjectGroupID = PG.ProjectGroupID
        LEFT JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK)
            ON P.ProjectID = PER.ProjectID AND PER.Role = 1
        LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
            ON CD.CurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
           AND CD.ConversionCurrencyID = @BaseCurrencyID
           AND PP.ToDate >= CD.FromDate AND PP.ToDate <= CD.ToDate
        OUTER APPLY (
            SELECT TOP 1 CD2.ConversionRate
            FROM tbl_PM_Currency_Detail CD2 WITH (NOLOCK)
            WHERE CD2.CurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
              AND CD2.ConversionCurrencyID = @BaseCurrencyID
              AND ISNULL(CD2.ConversionRate, 0) <> 0
            ORDER BY
                CASE WHEN PP.ToDate >= CD2.FromDate AND PP.ToDate <= CD2.ToDate THEN 0 ELSE 1 END,
                CD2.ToDate DESC
        ) CDFallback
        OUTER APPLY (
            SELECT TOP 1
                CASE WHEN ISNULL(CD3.ConversionRate, 0) = 0 THEN NULL
                     ELSE CAST(1.0 AS DECIMAL(18, 8)) / CAST(CD3.ConversionRate AS DECIMAL(18, 8))
                END AS InvRate
            FROM tbl_PM_Currency_Detail CD3 WITH (NOLOCK)
            WHERE CD3.CurrencyID = @BaseCurrencyID
              AND CD3.ConversionCurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
              AND ISNULL(CD3.ConversionRate, 0) <> 0
            ORDER BY
                CASE WHEN PP.ToDate >= CD3.FromDate AND PP.ToDate <= CD3.ToDate THEN 0 ELSE 1 END,
                CD3.ToDate DESC
        ) CDInverse
        WHERE
            PP.ToDate >= @CurrentFromDate AND PP.ToDate <= EOMONTH(@CurrentToDate)
        AND (@CustomerIDs IS NULL OR @CustomerIDs = '' OR P.CustomerID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@CustomerIDs, ',')))
        AND (@PortfolioIDs IS NULL OR @PortfolioIDs = '' OR P.ProjectGroupID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PortfolioIDs, ',')))
        AND (@ProjectManagerIDs IS NULL OR @ProjectManagerIDs = '' OR PER.EmployeeID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@ProjectManagerIDs, ',')))
        AND (@BillingTypeIDs IS NULL OR @BillingTypeIDs = '' OR P.ContractType IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@BillingTypeIDs, ',')))
        AND (@RegionIDs IS NULL OR @RegionIDs = '' OR P.LocationID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@RegionIDs, ',')))
    ),
    LatestPeriodData AS
    (
        SELECT *,
            ROW_NUMBER() OVER (
                PARTITION BY ProjectID
                ORDER BY RecordDate DESC
            ) AS RowSeq
        FROM BaseData
    ),
    FilteredBaseData AS
    (
        SELECT * FROM LatestPeriodData WHERE RowSeq = 1
    ),
    RevenueByPortfolio AS
    (
        SELECT
            CASE WHEN PortfolioID IS NULL THEN 0 ELSE PortfolioID END AS PortfolioID,
            CASE
                WHEN PortfolioID IS NULL THEN N'Unmapped'
                ELSE ISNULL(NULLIF(LTRIM(RTRIM(PortfolioName)), ''), N'Unmapped')
            END AS PortfolioName,
            ISNULL(SUM(CAST(RawRevenue AS DECIMAL(18, 4)) * ExchangeRate), 0) AS Revenue,
            /* //Added by Aditya J. on 28-09-2026 Count the projects that make up each mix slice, including Unmapped */
            COUNT(DISTINCT ProjectID) AS ProjectCount
            /* //End of Added by Aditya J. on 28-09-2026 Count the projects that make up each mix slice, including Unmapped */
        FROM FilteredBaseData
        GROUP BY
            CASE WHEN PortfolioID IS NULL THEN 0 ELSE PortfolioID END,
            CASE
                WHEN PortfolioID IS NULL THEN N'Unmapped'
                ELSE ISNULL(NULLIF(LTRIM(RTRIM(PortfolioName)), ''), N'Unmapped')
            END
    ),
    SelectedPortfolios AS
    (
        SELECT
            PG.ProjectGroupID AS PortfolioID,
            PG.ProjectGroupName AS PortfolioName
        FROM Tbl_PM_ProjectGroup PG WITH (NOLOCK)
        WHERE @HasPortfolioFilter = 1
          AND PG.ProjectGroupID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PortfolioIDs, ','))
    ),
    MixRows AS
    (
        SELECT
            SP.PortfolioID,
            SP.PortfolioName,
            CAST(ISNULL(R.Revenue, 0) AS DECIMAL(18, 4)) AS Revenue,
            /* //Added by Aditya J. on 28-09-2026 Keep the slice project count, including a selected group with no projects */
            ISNULL(R.ProjectCount, 0) AS ProjectCount
            /* //End of Added by Aditya J. on 28-09-2026 Keep the slice project count, including a selected group with no projects */
        FROM SelectedPortfolios SP
        LEFT JOIN RevenueByPortfolio R ON R.PortfolioID = SP.PortfolioID

        UNION ALL

        SELECT
            R.PortfolioID,
            R.PortfolioName,
            CAST(R.Revenue AS DECIMAL(18, 4)) AS Revenue,
            R.ProjectCount
        FROM RevenueByPortfolio R
        WHERE @HasPortfolioFilter = 0
    ),
    MixTotal AS
    (
        SELECT ISNULL(SUM(Revenue), 0) AS TotalRevenue
        FROM MixRows
    )
    SELECT
        MixRows.PortfolioID,
        MixRows.PortfolioName,
        CAST(CAST(ROUND(MixRows.Revenue / NULLIF(sc.Divisor, 0), 2) AS DECIMAL(18, 2)) AS VARCHAR(50)) AS Revenue,
        CASE
            WHEN ABS(MixRows.Revenue) < 0.005
                THEN ISNULL(@CurrencySymbol, N'') + '0.00'
            WHEN sc.Unit IS NULL OR LTRIM(RTRIM(sc.Unit)) = '' OR sc.Unit = 'Abs'
                THEN ISNULL(@CurrencySymbol, N'')
                     + CAST(CAST(ROUND(MixRows.Revenue / NULLIF(sc.Divisor, 0), 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
            ELSE ISNULL(@CurrencySymbol, N'')
                 + CAST(CAST(ROUND(MixRows.Revenue / NULLIF(sc.Divisor, 0), 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
                 + ' ' + sc.Unit
        END AS RevenueFormatted,
        CASE
            WHEN ABS(t.TotalRevenue) < 0.005 THEN CAST('' AS VARCHAR(20))
            WHEN sc.Unit IS NULL OR LTRIM(RTRIM(sc.Unit)) = '' OR sc.Unit = 'Abs' THEN CAST('' AS VARCHAR(20))
            ELSE sc.Unit
        END AS Unit,
        ISNULL(@CurrencySymbol, N'') AS BaseCurrencyCode,
        ISNULL(@CurrencyCode, N'') AS CurrencyCode,
        /* //Added by Aditya J. on 28-09-2026 Return the project count for the mix tooltip and drill-down */
        MixRows.ProjectCount
        /* //End of Added by Aditya J. on 28-09-2026 Return the project count for the mix tooltip and drill-down */
    FROM MixRows
    CROSS JOIN MixTotal t
    CROSS APPLY dbo.udf_Whizible2_GetAnalyticsDBMoneyScale(t.TotalRevenue, @CurrencyCode, @CurrencySymbol) sc
    ORDER BY MixRows.PortfolioName;
END
GO
