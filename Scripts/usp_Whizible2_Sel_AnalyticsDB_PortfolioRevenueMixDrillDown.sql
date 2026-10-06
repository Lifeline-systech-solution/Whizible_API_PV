IF EXISTS
(
    SELECT [name] FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown]')
      AND type IN (N'P', N'PC')
)
DROP PROCEDURE [dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown]
/*
Project Name     	:	W27 Dashboard
Procedure Name		:	usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown
Purpose          	:	Portfolio Revenue Mix drill-down.
Description      	:	Portfolio Revenue Mix drill-down.
i/p param.			:	
o/i param.			:	
Assumptions      	:	None.
Dependencies     	:	None.
Author           	:	
Created          	:	23-09-2026
Reviewed         	:	
Revisions        	:	28-09-2026 Aditya J. Unmapped drill-down keeps level-wise rows for ProjectGroupID NULL or 0.
*/
-- Added by Aditya J. on 15-09-2026
-- Portfolio Revenue Mix drill-down.
-- Levels 2–6 for a mapped portfolio reuse usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown
-- after forcing @PortfolioIDs to the clicked ProjectGroupID.
-- Level 1 is local so a selected group with no projects still returns Revenue = 0.
-- PortfolioID = 0 means Unmapped (Tbl_PM_Project.ProjectGroupID IS NULL or 0) at every level.
    @Level             INT = 1,
    @CurrentFromDate   DATETIME,
    @CurrentToDate     DATETIME,
    @PortfolioID       INT = NULL,
    @ProjectID         INT = NULL,
    @InvoiceID         VARCHAR(50) = NULL,
    @InvoiceItemIDs    VARCHAR(50) = NULL,
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

    DECLARE @BaseCurrencyID INT, @CurrencySymbol NVARCHAR(10);
    SELECT TOP 1 @BaseCurrencyID = ISNULL(BaseCurrencyID, 1) FROM Tbl_PM_CompanyInformation WITH (NOLOCK);
    SELECT @CurrencySymbol = ISNULL(CurrencySymbol, '') FROM tbl_PM_CurrencyMaster WHERE CurrencyID = @BaseCurrencyID;
    /* //Added by Aditya J. on 29-09-2026 Show $ for USD the same way as the Revenue card */
    IF UPPER(LTRIM(RTRIM(ISNULL(@CurrencySymbol, N'')))) = N'USD'
        SET @CurrencySymbol = N'$';
    /* //End of Added by Aditya J. on 29-09-2026 Show $ for USD the same way as the Revenue card */

    /* //Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines (level 4) */
    IF @Level > 4
        RETURN;
    /* //End of Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines (level 4) */

    DECLARE @IsUnmapped BIT = CASE WHEN @PortfolioID IS NOT NULL AND @PortfolioID = 0 THEN 1 ELSE 0 END;

    -- Clicked slice wins over the dashboard multi-select list.
    IF (@PortfolioID IS NOT NULL AND @PortfolioID > 0)
        SET @PortfolioIDs = CONVERT(VARCHAR(20), @PortfolioID);

    ---------------------------------------------------------------------
    -- LEVEL 1: single clicked portfolio (or Unmapped), including empty groups
    ---------------------------------------------------------------------
    IF @Level = 1
    BEGIN
        IF @IsUnmapped = 1
        BEGIN
            ;WITH RawUnmapped AS
            (
                SELECT
                    P.ProjectID,
                    CASE
                        WHEN P.ContractType = 1 THEN ISNULL(PP.PeriodicMilestoneBilledAmount, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                        ELSE ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                    END * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0) AS ProjectRevenue,
                    (ISNULL(PP.PeriodicAccruedResourceCostTotal, 0) + ISNULL(PP.PeriodicOtherCostTotal, 0))
                        * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0) AS ProjectCost
                FROM Tbl_PM_Project P WITH (NOLOCK)
                INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H ON H.ProjectID = P.ProjectID
                /* //Added by Aditya J. on 28-09-2026 Count only Unmapped projects that have revenue in the mix period */
                INNER JOIN Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
                    ON P.ProjectID = PP.ProjectID
                   AND PP.ToDate >= @CurrentFromDate AND PP.ToDate <= EOMONTH(@CurrentToDate)
                /* //End of Added by Aditya J. on 28-09-2026 Count only Unmapped projects that have revenue in the mix period */
                LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
                    ON CD.CurrencyID = P.BaseCurrency
                   AND CD.ConversionCurrencyID = @BaseCurrencyID
                   AND PP.ToDate >= CD.FromDate AND PP.ToDate <= CD.ToDate
                /* //Added by Aditya J. on 28-09-2026 Unmapped level 1 includes ProjectGroupID NULL or 0 so the summary matches the mix chart */
                WHERE ISNULL(P.ProjectGroupID, 0) = 0
                /* //End of Added by Aditya J. on 28-09-2026 Unmapped level 1 includes ProjectGroupID NULL or 0 so the summary matches the mix chart */
                AND (@CustomerIDs IS NULL OR @CustomerIDs = '' OR P.CustomerID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@CustomerIDs, ',')))
                AND (@BillingTypeIDs IS NULL OR @BillingTypeIDs = '' OR P.ContractType IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@BillingTypeIDs, ',')))
                AND (@RegionIDs IS NULL OR @RegionIDs = '' OR P.LocationID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@RegionIDs, ',')))
            ),
            UnmappedAgg AS
            (
                SELECT
                    COUNT(DISTINCT ProjectID) AS Projects,
                    ISNULL(SUM(ProjectRevenue), 0) AS TotalRevenue,
                    ISNULL(SUM(ProjectCost), 0) AS TotalCost
                FROM RawUnmapped
            )
            SELECT
                0 AS PortfolioID,
                N'Unmapped' AS Portfolio,
                Projects,
                TotalRevenue,
                TotalCost,
                CASE
                    WHEN ABS(TotalRevenue) >= 10000000
                        THEN @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' Cr'
                    WHEN ABS(TotalRevenue) >= 100000
                        THEN @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue / 100000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' L'
                    ELSE @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue, 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
                END AS RevenueFormatted,
                CASE
                    WHEN ABS(TotalCost) >= 10000000
                        THEN @CurrencySymbol + CAST(CAST(ROUND(TotalCost / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' Cr'
                    WHEN ABS(TotalCost) >= 100000
                        THEN @CurrencySymbol + CAST(CAST(ROUND(TotalCost / 100000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' L'
                    ELSE @CurrencySymbol + CAST(CAST(ROUND(TotalCost, 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
                END AS CostFormatted,
                CASE WHEN TotalRevenue <> 0 THEN ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 2) ELSE 0.00 END AS GrossMarginPct,
                CASE
                    WHEN TotalRevenue <> 0 THEN CAST(CAST(ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 1) AS DECIMAL(18, 1)) AS VARCHAR(20)) + '%'
                    ELSE '—'
                END AS GrossMarginFormatted
            FROM UnmappedAgg;
        END
        ELSE
        BEGIN
            /*
                LEVEL 1 - MAPPED PORTFOLIO
                Keep this calculation identical to
                usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown Level 1.

                Important:
                1. Use only the latest profitability row per project.
                2. Use EOMONTH(@CurrentToDate), exactly like RevenueDrillDown.
                3. Use the same currency fallback/inverse-rate logic.
                4. Apply the same Project Manager / Customer / Billing / Region / Health filters.
                5. Aggregate the project-level latest values after RowSeq = 1.
            */
            ;WITH RawProfitabilityData AS
            (
                SELECT
                    PG.ProjectGroupID AS PortfolioID,
                    ISNULL(NULLIF(LTRIM(RTRIM(PG.ProjectGroupName)), ''), N'Unassigned') AS Portfolio,
                    P.ProjectID,
                    PP.ToDate,

                    (
                        CASE
                            WHEN P.ContractType = 1 THEN
                                ISNULL(PP.PeriodicMilestoneBilledAmount, 0)
                                + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                            ELSE
                                ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0)
                                + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                        END
                    )
                    * CASE
                        WHEN ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency) = @BaseCurrencyID
                            THEN CAST(1.0 AS DECIMAL(18, 8))
                        ELSE CAST(ISNULL(
                            NULLIF(CD.ConversionRate, 0),
                            ISNULL(
                                NULLIF(CDFallback.ConversionRate, 0),
                                ISNULL(NULLIF(CDInverse.InvRate, 0), 1.0)
                            )
                        ) AS DECIMAL(18, 8))
                      END AS RawRevenue,

                    (
                        ISNULL(PP.PeriodicAccruedResourceCostTotal, 0)
                        + ISNULL(PP.PeriodicOtherCostTotal, 0)
                    )
                    * CASE
                        WHEN ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency) = @BaseCurrencyID
                            THEN CAST(1.0 AS DECIMAL(18, 8))
                        ELSE CAST(ISNULL(
                            NULLIF(CD.ConversionRate, 0),
                            ISNULL(
                                NULLIF(CDFallback.ConversionRate, 0),
                                ISNULL(NULLIF(CDInverse.InvRate, 0), 1.0)
                            )
                        ) AS DECIMAL(18, 8))
                      END AS RawCost,

                    ROW_NUMBER() OVER
                    (
                        PARTITION BY P.ProjectID
                        ORDER BY PP.ToDate DESC
                    ) AS RowSeq

                FROM Tbl_PM_ProjectGroup PG WITH (NOLOCK)

                INNER JOIN Tbl_PM_Project P WITH (NOLOCK)
                    ON P.ProjectGroupID = PG.ProjectGroupID

                INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H
                    ON H.ProjectID = P.ProjectID

                INNER JOIN Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
                    ON P.ProjectID = PP.ProjectID
                   AND PP.ToDate >= @CurrentFromDate
                   AND PP.ToDate <= EOMONTH(@CurrentToDate)

                LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
                    ON CD.CurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
                   AND CD.ConversionCurrencyID = @BaseCurrencyID
                   AND PP.ToDate >= CD.FromDate
                   AND PP.ToDate <= CD.ToDate

                OUTER APPLY
                (
                    SELECT TOP 1
                        CD2.ConversionRate
                    FROM tbl_PM_Currency_Detail CD2 WITH (NOLOCK)
                    WHERE CD2.CurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
                      AND CD2.ConversionCurrencyID = @BaseCurrencyID
                      AND ISNULL(CD2.ConversionRate, 0) <> 0
                    ORDER BY
                        CASE
                            WHEN PP.ToDate >= CD2.FromDate
                             AND PP.ToDate <= CD2.ToDate
                                THEN 0
                            ELSE 1
                        END,
                        CD2.ToDate DESC
                ) CDFallback

                OUTER APPLY
                (
                    SELECT TOP 1
                        CASE
                            WHEN ISNULL(CD3.ConversionRate, 0) = 0 THEN NULL
                            ELSE CAST(1.0 AS DECIMAL(18, 8))
                                 / CAST(CD3.ConversionRate AS DECIMAL(18, 8))
                        END AS InvRate
                    FROM tbl_PM_Currency_Detail CD3 WITH (NOLOCK)
                    WHERE CD3.CurrencyID = @BaseCurrencyID
                      AND CD3.ConversionCurrencyID = ISNULL(NULLIF(P.BaseCurrency, 0), P.BaseCurrency)
                      AND ISNULL(CD3.ConversionRate, 0) <> 0
                    ORDER BY
                        CASE
                            WHEN PP.ToDate >= CD3.FromDate
                             AND PP.ToDate <= CD3.ToDate
                                THEN 0
                            ELSE 1
                        END,
                        CD3.ToDate DESC
                ) CDInverse

                WHERE
                    /* //Added by Aditya J. on 29-09-2026 Main drill button lists every portfolio when no slice is selected */
                    (@PortfolioID IS NULL OR PG.ProjectGroupID = @PortfolioID)
                    /* //End of Added by Aditya J. on 29-09-2026 Main drill button lists every portfolio when no slice is selected */

                AND (
                    @PortfolioIDs IS NULL
                    OR LTRIM(RTRIM(@PortfolioIDs)) = ''
                    OR PG.ProjectGroupID IN
                    (
                        SELECT CAST(value AS INT)
                        FROM STRING_SPLIT(@PortfolioIDs, ',')
                        WHERE LTRIM(RTRIM(value)) <> ''
                    )
                )

                AND (
                    @CustomerIDs IS NULL
                    OR LTRIM(RTRIM(@CustomerIDs)) = ''
                    OR P.CustomerID IN
                    (
                        SELECT CAST(value AS INT)
                        FROM STRING_SPLIT(@CustomerIDs, ',')
                        WHERE LTRIM(RTRIM(value)) <> ''
                    )
                )

                AND (
                    @BillingTypeIDs IS NULL
                    OR LTRIM(RTRIM(@BillingTypeIDs)) = ''
                    OR P.ContractType IN
                    (
                        SELECT CAST(value AS INT)
                        FROM STRING_SPLIT(@BillingTypeIDs, ',')
                        WHERE LTRIM(RTRIM(value)) <> ''
                    )
                )

                AND (
                    @RegionIDs IS NULL
                    OR LTRIM(RTRIM(@RegionIDs)) = ''
                    OR P.LocationID IN
                    (
                        SELECT CAST(value AS INT)
                        FROM STRING_SPLIT(@RegionIDs, ',')
                        WHERE LTRIM(RTRIM(value)) <> ''
                    )
                )

                AND (
                    @ProjectManagerIDs IS NULL
                    OR LTRIM(RTRIM(@ProjectManagerIDs)) = ''
                    OR P.ProjectManager IN
                    (
                        SELECT CAST(value AS INT)
                        FROM STRING_SPLIT(@ProjectManagerIDs, ',')
                        WHERE LTRIM(RTRIM(value)) <> ''
                    )
                    OR EXISTS
                    (
                        SELECT 1
                        FROM tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK)
                        WHERE PER.ProjectID = P.ProjectID
                          AND ISNULL(PER.Role, 0) = 1
                          AND PER.EmployeeID IN
                          (
                              SELECT CAST(value AS INT)
                              FROM STRING_SPLIT(@ProjectManagerIDs, ',')
                              WHERE LTRIM(RTRIM(value)) <> ''
                          )
                    )
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.tbl_Whizible2_Project_AdditionalInformation PAI WITH (NOLOCK)
                        WHERE PAI.ProjectID = P.ProjectID
                          AND ISNULL(PAI.ProjectManagerID, 0) <> 0
                          AND PAI.ProjectManagerID IN
                          (
                              SELECT CAST(value AS INT)
                              FROM STRING_SPLIT(@ProjectManagerIDs, ',')
                              WHERE LTRIM(RTRIM(value)) <> ''
                          )
                    )
                )
            ),
            ProjectPeriodData AS
            (
                SELECT
                    PortfolioID,
                    Portfolio,
                    ProjectID,
                    RawRevenue AS ProjectRevenue,
                    RawCost AS ProjectCost
                FROM RawProfitabilityData
                WHERE RowSeq = 1
            ),
            PortfolioAggregated AS
            (
                SELECT
                    PortfolioID,
                    Portfolio,
                    COUNT(DISTINCT ProjectID) AS Projects,
                    ISNULL(SUM(ProjectRevenue), 0) AS TotalRevenue,
                    ISNULL(SUM(ProjectCost), 0) AS TotalCost
                FROM ProjectPeriodData
                GROUP BY PortfolioID, Portfolio
            )
            SELECT
                PortfolioID,
                Portfolio,
                Projects,
                TotalRevenue,
                TotalCost,
                @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) AS RevenueFormatted,
                @CurrencySymbol + CAST(CAST(ROUND(TotalCost, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) AS CostFormatted,
                @CurrencySymbol AS CorporateCurrencySymbol,
                CASE
                    WHEN TotalRevenue <> 0
                        THEN ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 2)
                    ELSE 0.00
                END AS GrossMarginPct,
                CASE
                    WHEN TotalRevenue <> 0
                        THEN CAST(CAST(
                            ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 1)
                            AS DECIMAL(18, 1)
                        ) AS VARCHAR(20)) + '%'
                    ELSE '—'
                END AS GrossMarginFormatted
            FROM PortfolioAggregated
            WHERE Projects > 0
            ORDER BY Portfolio;
        END
        RETURN;
    END

    ---------------------------------------------------------------------
    -- Mapped portfolio, levels 2–6: reuse existing Revenue drill-down SP.
    ---------------------------------------------------------------------
    IF @IsUnmapped = 0 and  @Level <> 5
    BEGIN
        EXEC [dbo].[usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown]
            @Level             = @Level,
            @CurrentFromDate   = @CurrentFromDate,
            @CurrentToDate     = @CurrentToDate,
            @PortfolioID       = @PortfolioID,
            @ProjectID         = @ProjectID,
            @InvoiceID         = @InvoiceID,
            @InvoiceItemIDs    = @InvoiceItemIDs,
            @CustomerIDs       = @CustomerIDs,
            @ProjectManagerIDs = @ProjectManagerIDs,
            @PortfolioIDs      = @PortfolioIDs,
            @RegionIDs         = @RegionIDs,
            @BillingTypeIDs    = @BillingTypeIDs,
            @HealthIDs         = @HealthIDs,
            @DashboardID       = @DashboardID;

        RETURN;
    END

    ---------------------------------------------------------------------
    -- Unmapped (ProjectGroupID IS NULL or 0), levels 2–5
    -- Copied from usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown with NULL group filter.
    ---------------------------------------------------------------------
    IF @Level = 2
    BEGIN
        ;WITH RawProjectData AS
        (
            SELECT
                P.ProjectID,
                P.ProjectName AS Project,
                P.ProjectCode,
                C.CustomerName AS Customer,
                EMP.EmployeeName AS PM,
                /* //Added by Aditya J. on 28-09-2026 Do not drop an unmapped project when its contract type row is missing */
                ISNULL(CT.NodeLabel, N'—') AS Billing,
                /* //End of Added by Aditya J. on 28-09-2026 Do not drop an unmapped project when its contract type row is missing */
                ISNULL(CUR.CurrencyCode, N'—') AS SourceCurrency,
                ISNULL(
                    CASE
                        WHEN P.ContractType = 1 THEN ISNULL(PP.PeriodicMilestoneBilledAmount, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                        ELSE ISNULL(PP.PeriodicAccruedResourceBillingTotal, 0) + ISNULL(PP.PeriodicBillableOtherCostTotal, 0)
                    END * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)
                , 0) AS Revenue,
                ISNULL(
                    (ISNULL(PP.PeriodicAccruedResourceCostTotal, 0) + ISNULL(PP.PeriodicOtherCostTotal, 0))
                    * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)
                , 0) AS Cost,
                H.OverallHealth AS Health
            FROM Tbl_PM_Project P WITH (NOLOCK)
            INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H ON H.ProjectID = P.ProjectID
            /* //Added by Aditya J. on 28-09-2026 Do not drop an unmapped project when its contract type row is missing */
            LEFT JOIN tbl_CNF_ContractType CT WITH (NOLOCK) ON CT.ContractTypeID = P.ContractType
            /* //End of Added by Aditya J. on 28-09-2026 Do not drop an unmapped project when its contract type row is missing */
            LEFT JOIN Tbl_PM_Customer C WITH (NOLOCK) ON P.CustomerID = C.Customer
            LEFT JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK) ON P.ProjectID = PER.ProjectID AND PER.Role = 1
            LEFT JOIN Tbl_PM_Employee EMP WITH (NOLOCK) ON PER.EmployeeID = EMP.EmployeeID
            LEFT JOIN Tbl_PM_ProjectProfitability PP WITH (NOLOCK)
                ON P.ProjectID = PP.ProjectID
               AND PP.ToDate >= @CurrentFromDate AND PP.ToDate <= @CurrentToDate
            LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
                ON CD.CurrencyID = P.BaseCurrency
               AND CD.ConversionCurrencyID = @BaseCurrencyID
               AND PP.ToDate >= CD.FromDate AND PP.ToDate <= CD.ToDate
            /* //Added by Aditya J. on 28-09-2026 Return source currency on the unmapped project grid */
            LEFT JOIN tbl_PM_CurrencyMaster CUR WITH (NOLOCK) ON CUR.CurrencyID = P.BaseCurrency
            /* //End of Added by Aditya J. on 28-09-2026 Return source currency on the unmapped project grid */
            /* //Added by Aditya J. on 28-09-2026 Unmapped level 2 includes ProjectGroupID NULL or 0 so project rows are returned */
            WHERE ISNULL(P.ProjectGroupID, 0) = 0
            /* //End of Added by Aditya J. on 28-09-2026 Unmapped level 2 includes ProjectGroupID NULL or 0 so project rows are returned */
            AND (@CustomerIDs IS NULL OR @CustomerIDs = '' OR P.CustomerID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@CustomerIDs, ',')))
            AND (@ProjectManagerIDs IS NULL OR @ProjectManagerIDs = '' OR PER.EmployeeID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@ProjectManagerIDs, ',')))
            AND (@BillingTypeIDs IS NULL OR @BillingTypeIDs = '' OR P.ContractType IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@BillingTypeIDs, ',')))
            AND (@RegionIDs IS NULL OR @RegionIDs = '' OR P.LocationID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@RegionIDs, ',')))
        ),
        ProjectAggregated AS
        (
            SELECT
                ProjectID, Project, ProjectCode, Customer, ISNULL(PM, '—') AS PM, Billing, SourceCurrency, Health,
                SUM(Revenue) AS TotalRevenue,
                SUM(Cost) AS TotalCost
            FROM RawProjectData
            GROUP BY ProjectID, Project, ProjectCode, Customer, PM, Billing, SourceCurrency, Health
        )
        SELECT
            ProjectID, Project, ProjectCode, Customer, PM, Billing, SourceCurrency,
            TotalRevenue AS Revenue,
            CASE
                WHEN ABS(TotalRevenue) >= 10000000
                    THEN @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' Cr'
                WHEN ABS(TotalRevenue) >= 100000
                    THEN @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' L'
                ELSE @CurrencySymbol + CAST(CAST(ROUND(TotalRevenue, 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
            END AS RevenueFormatted,
            /* //Added by Aditya J. on 28-09-2026 Return cost on the unmapped project grid */
            TotalCost AS Cost,
            CASE
                WHEN ABS(TotalCost) >= 10000000
                    THEN @CurrencySymbol + CAST(CAST(ROUND(TotalCost / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' Cr'
                WHEN ABS(TotalCost) >= 100000
                    THEN @CurrencySymbol + CAST(CAST(ROUND(TotalCost / 100000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' L'
                ELSE @CurrencySymbol + CAST(CAST(ROUND(TotalCost, 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
            END AS CostFormatted,
            /* //End of Added by Aditya J. on 28-09-2026 Return cost on the unmapped project grid */
            CASE WHEN TotalRevenue <> 0 THEN ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 1) ELSE 0.0 END AS Margin,
            CASE
                WHEN TotalRevenue <> 0 THEN CAST(CAST(ROUND(((TotalRevenue - TotalCost) / TotalRevenue) * 100.0, 1) AS DECIMAL(18, 1)) AS VARCHAR(20)) + '%'
                ELSE '0.0%'
            END AS MarginFormatted,
            Health
        FROM ProjectAggregated
        ORDER BY Project;
    END
    ELSE IF @Level = 3
    BEGIN
        SELECT
            INV.InvoiceID,
            LTRIM(RTRIM(INV.InvoiceNumber)) AS [Invoice Number],
            ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0) AS RawInvoiceAmount,
            CASE
                WHEN ABS(ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)) >= 10000000
                    THEN @CurrencySymbol + CAST(CAST(ROUND((ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)) / 10000000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' Cr'
                WHEN ABS(ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)) >= 100000
                    THEN @CurrencySymbol + CAST(CAST(ROUND((ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)) / 100000.0, 2) AS DECIMAL(18, 2)) AS VARCHAR(30)) + ' L'
                ELSE @CurrencySymbol + CAST(CAST(ROUND((ISNULL(INV.BaseCurrencyAmount, 0) * ISNULL(CASE WHEN P.BaseCurrency = @BaseCurrencyID THEN 1.0 ELSE CD.ConversionRate END, 1.0)), 2) AS DECIMAL(18, 2)) AS VARCHAR(30))
            END AS Amount,
            'NA' AS Status,
            CONVERT(VARCHAR(11), INV.InvoiceDate, 106) AS [Invoice Date],
            INV.InvoiceDate AS RawInvoiceDate,
            ISNULL(NULLIF(LTRIM(RTRIM(INV.CustomFieldText2)), ''), '—') AS [PO Ref]
        FROM Tbl_PM_RFIInvoices INV WITH (NOLOCK)
        INNER JOIN Tbl_PM_Project P WITH (NOLOCK) ON INV.ProjectID = P.ProjectID
        INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H ON H.ProjectID = P.ProjectID
        LEFT JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK) ON P.ProjectID = PER.ProjectID AND PER.Role = 1
        LEFT JOIN tbl_PM_Currency_Detail CD WITH (NOLOCK)
            ON CD.CurrencyID = P.BaseCurrency
           AND CD.ConversionCurrencyID = @BaseCurrencyID
           AND INV.InvoiceDate >= CD.FromDate
           AND INV.InvoiceDate <= CD.ToDate
        /* //Added by Aditya J. on 28-09-2026 Unmapped invoices include ProjectGroupID NULL or 0 */
        WHERE ISNULL(P.ProjectGroupID, 0) = 0
        /* //End of Added by Aditya J. on 28-09-2026 Unmapped invoices include ProjectGroupID NULL or 0 */
        AND (@ProjectID IS NULL OR INV.ProjectID = @ProjectID)
        AND (@CustomerIDs IS NULL OR @CustomerIDs = '' OR P.CustomerID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@CustomerIDs, ',')))
        AND (@ProjectManagerIDs IS NULL OR @ProjectManagerIDs = '' OR PER.EmployeeID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@ProjectManagerIDs, ',')))
        AND (@BillingTypeIDs IS NULL OR @BillingTypeIDs = '' OR P.ContractType IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@BillingTypeIDs, ',')))
        AND (@RegionIDs IS NULL OR @RegionIDs = '' OR P.LocationID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@RegionIDs, ',')))
        ORDER BY INV.InvoiceDate DESC, INV.InvoiceNumber;
    END
    ELSE IF @Level = 4
    BEGIN
        SELECT
            ISNULL(IL.ProjectTimesheetID, 0) AS ProjectTimesheetID,
            ISNULL(IL.ExpensesEntryID, 0) AS ExpensesEntryID,
            ISNULL(IL.EmployeeID, 0) AS EmployeeID,
            ISNULL(IL.DeliverableID, 0) AS DeliverableID,
            ISNULL(IL.MilestoneID, 0) AS MilestoneID,
            ISNULL(IL.RFIItemID, 0) AS Line,
            IL.ItemDescription,
            IL.Quantity AS [Qty (h)],
            FORMAT(ROUND(IL.Rate, 3), 'N3', 'en-US') AS Rate,
            FORMAT(ROUND(IL.Amount, 3), 'N3', 'en-US') AS Amount,
            FORMAT(ROUND(IL.BaseCurrencyAmount, 3), 'N3', 'en-US') AS BaseCurrencyAmount,
            @CurrencySymbol AS CurrencySymbol
        FROM tbl_PM_RFI_Items IL WITH (NOLOCK)
        INNER JOIN Tbl_PM_RFIInvoices INV WITH (NOLOCK) ON IL.InvoiceID = INV.InvoiceID
        INNER JOIN Tbl_PM_Project P WITH (NOLOCK) ON INV.ProjectID = P.ProjectID
        INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H ON H.ProjectID = P.ProjectID
        LEFT JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK) ON P.ProjectID = PER.ProjectID AND PER.Role = 1
        /* //Added by Aditya J. on 28-09-2026 Unmapped invoice lines include ProjectGroupID NULL or 0 */
        WHERE ISNULL(P.ProjectGroupID, 0) = 0
        /* //End of Added by Aditya J. on 28-09-2026 Unmapped invoice lines include ProjectGroupID NULL or 0 */
        AND (@InvoiceID IS NULL OR IL.InvoiceID = @InvoiceID)
        AND (@ProjectID IS NULL OR INV.ProjectID = @ProjectID)
        AND (@CustomerIDs IS NULL OR @CustomerIDs = '' OR P.CustomerID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@CustomerIDs, ',')))
        AND (@ProjectManagerIDs IS NULL OR @ProjectManagerIDs = '' OR PER.EmployeeID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@ProjectManagerIDs, ',')))
        AND (@BillingTypeIDs IS NULL OR @BillingTypeIDs = '' OR P.ContractType IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@BillingTypeIDs, ',')))
        AND (@RegionIDs IS NULL OR @RegionIDs = '' OR P.LocationID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@RegionIDs, ',')));
    END
	ELSE IF @Level = 5
BEGIN
    /*
        LEVEL 5
        -------
        Invoice -> Source details

        @InvoiceItemIDs is not required.
        Source records are derived directly from tbl_PM_RFI_Items
        using @InvoiceID and @ProjectID.

        Supports mapped and unmapped projects.
    */

    SELECT DISTINCT
        CASE
            WHEN ISNULL(IL.ProjectTimesheetID, 0) > 0
                THEN N'Timesheet'
            WHEN ISNULL(IL.MilestoneID, 0) > 0
                THEN N'Milestone'
            WHEN ISNULL(IL.DeliverableID, 0) > 0
                THEN N'Deliverable'
            WHEN ISNULL(IL.ExpensesEntryID, 0) > 0
                THEN N'Expense'
        END AS SourceType,

        /* Common identifiers */
        ISNULL(IL.ProjectTimesheetID, 0) AS ProjectTimesheetID,
        ISNULL(IL.MilestoneID, 0) AS MilestoneID,
        ISNULL(IL.DeliverableID, 0) AS DeliverableID,
        ISNULL(IL.ExpensesEntryID, 0) AS ExpensesEntryID,

        /* Timesheet information */
        CONVERT(VARCHAR(200), TS.EntryDate, 106) AS TimesheetDate,
        E.EmployeeName,
        TS.Task,
        dbo.fn_Whizible2_ConvertDecimalToHourViceVersa(
            TS.Duration,
            1
        ) AS Duration,
        ISNULL(PT.BillableYN, 0) AS Billable,

        /* Milestone information */
        M.MileStone,
        FORMAT(
            ROUND(M.BillAmount, 3),
            'N3',
            'en-US'
        ) AS BillAmount,
        CONVERT(
            VARCHAR(200),
            M.ActualCompletionDate,
            106
        ) AS ActualCompletionDate,
        M.CompletionPercentage,
        CONVERT(
            VARCHAR(200),
            M.RevenueStatusChangeDate,
            106
        ) AS RevenueStatusChangeDate,

        /* Deliverable information */
        OS.Title,
        OS.ScheduleID,
        FORMAT(
            ROUND(OS.BillableAmount, 3),
            'N3',
            'en-US'
        ) AS BillableAmount,
        OS.PercentageComplete,

        /* Project information */
        P.ProjectID,
        P.ProjectName,
        P.ProjectGroupID AS PortfolioID

    FROM tbl_PM_RFI_Items IL WITH (NOLOCK)

    INNER JOIN Tbl_PM_RFIInvoices INV WITH (NOLOCK)
        ON IL.InvoiceID = INV.InvoiceID

    INNER JOIN Tbl_PM_Project P WITH (NOLOCK)
        ON INV.ProjectID = P.ProjectID

    INNER JOIN dbo.fn_Whizible2_Get_ProjectOverallHealth(@HealthIDs) H
        ON H.ProjectID = P.ProjectID

    LEFT JOIN tbl_PM_ProjectEmployeeRole PER WITH (NOLOCK)
        ON P.ProjectID = PER.ProjectID
       AND PER.Role = 1

    /* Timesheet */
    LEFT JOIN tbl_PM_Timesheet TS WITH (NOLOCK)
        ON IL.ProjectTimesheetID = TS.TimeSheetNo

    LEFT JOIN tbl_PM_ProjectTasks PT WITH (NOLOCK)
        ON PT.TaskID = TS.TaskID

    LEFT JOIN Tbl_PM_Employee E WITH (NOLOCK)
        ON TS.EmployeeID = E.EmployeeID

    /* Milestone */
    LEFT JOIN tbl_PM_Milestones M WITH (NOLOCK)
        ON IL.MilestoneID = M.MilestoneID

    /* Deliverable */
    LEFT JOIN tbl_PM_OtherSchedules OS WITH (NOLOCK)
        ON IL.DeliverableID = OS.ScheduleID

    WHERE
        (@InvoiceID IS NULL OR IL.InvoiceID = @InvoiceID)

        AND
        (
            @ProjectID IS NULL
            OR P.ProjectID = @ProjectID
        )

        AND
        (
            ISNULL(IL.ProjectTimesheetID, 0) > 0
            OR ISNULL(IL.MilestoneID, 0) > 0
            OR ISNULL(IL.DeliverableID, 0) > 0
            OR ISNULL(IL.ExpensesEntryID, 0) > 0
        )

        /* //Added by Aditya J. on 28-09-2026 Unmapped level 5 stays on ProjectGroupID NULL or 0; mapped level 5 is unchanged */
        AND
        (
            @IsUnmapped = 0
            OR ISNULL(P.ProjectGroupID, 0) = 0
        )
        /* //End of Added by Aditya J. on 28-09-2026 Unmapped level 5 stays on ProjectGroupID NULL or 0; mapped level 5 is unchanged */

        AND
        (
            @CustomerIDs IS NULL
            OR @CustomerIDs = ''
            OR P.CustomerID IN
            (
                SELECT CAST(value AS INT)
                FROM STRING_SPLIT(@CustomerIDs, ',')
            )
        )

        AND
        (
            @ProjectManagerIDs IS NULL
            OR @ProjectManagerIDs = ''
            OR PER.EmployeeID IN
            (
                SELECT CAST(value AS INT)
                FROM STRING_SPLIT(@ProjectManagerIDs, ',')
            )
        )

        AND
        (
            @BillingTypeIDs IS NULL
            OR @BillingTypeIDs = ''
            OR P.ContractType IN
            (
                SELECT CAST(value AS INT)
                FROM STRING_SPLIT(@BillingTypeIDs, ',')
            )
        )

        AND
        (
            @RegionIDs IS NULL
            OR @RegionIDs = ''
            OR P.LocationID IN
            (
                SELECT CAST(value AS INT)
                FROM STRING_SPLIT(@RegionIDs, ',')
            )
        )

    ORDER BY
        SourceType,
        TimesheetDate,
        EmployeeName,
        Task,
        MileStone,
        Title;

END
END
GO
