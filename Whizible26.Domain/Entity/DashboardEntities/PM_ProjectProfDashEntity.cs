using System;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    // Added by Vyankat B. on 07-08-2026 - Project Profitability Dashboard entities / requests / responses

    #region Requests

    public class ProfDashBusinessGroupRequest
    {
        public int? UserID { get; set; }
        public string? LoginType { get; set; } = "E";
    }

    public class ProfDashAccessibleProjectsRequest
    {
        public int? ProjectTypeId { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? OrganizationUnitID { get; set; }
        public int? ProjectGroupId { get; set; }
        public int? EmployeeID { get; set; }
        public string? ProjectIDs { get; set; }
        public string? LoginType { get; set; }
    }

    public class ProfDashLocationsRequest
    {
        public int? BusinessGroupID { get; set; }
        public int? UserID { get; set; }
        public string? LoginType { get; set; } = "E";
    }

    // Added by Vyankat B. on 17-08-2026 - CSV multi-select for BG / Location / Project / ProjectGroup
    public class ProfDashProjectProfitByProjectGroupRequest
    {
        /// <summary>CSV of BusinessGroupID. null / "" / "0" = all. Example: "1,4"</summary>
        public string? BusinessGroupID { get; set; }
        /// <summary>CSV of LocationID. null / "" / "0" = all. Example: "1,3,9"</summary>
        public string? LocationID { get; set; }
        /// <summary>CSV of ProjectID. null / "" / "0" = all.</summary>
        public string? ProjectIDs { get; set; }
        /// <summary>Optional single ProjectID filter.</summary>
        public int? ProjectID { get; set; }
        public int GPMTrend { get; set; } = 0;
        /// <summary>CSV of ProjectGroupID. null / "" / "0" = all.</summary>
        public string? ProjectGroupID { get; set; }
        public int? CurrencyID { get; set; }
    }

    #endregion

    #region Response models (dropdown SPs)

    public class ProfDashBusinessGroupModel
    {
        public int BusinessGroupID { get; set; }
        public string? BusinessGroup { get; set; }
        public int Ordinaery { get; set; }
    }

    public class ProfDashProjectGroupModel
    {
        public int ProjectGroupID { get; set; }
        public string? ProjectGroupName { get; set; }
        public int Ordinaery { get; set; }
    }

    public class ProfDashCurrencyModel
    {
        public int CurrencyID { get; set; }
        public string? CurrencyCode { get; set; }
        // Added By Vyankat B. on 26th Aug 2026
        public string? CurrencySymbol { get; set; }
        // End of Added By Vyankat B. on 26th Aug 2026
        public int Ordinaery { get; set; }
    }

    public class ProfDashLocationModel
    {
        public int LocationID { get; set; }
        public string? Location { get; set; }
        public int Ordinaery { get; set; }
    }

    public class ProfDashAccessibleProjectModel
    {
        public int ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public int Ordinaery { get; set; }
    }

    public class ProfDashProjectProfitByProjectGroupModel
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ProjectCurrencyID { get; set; }
        public int? BaseCurrencyID { get; set; }
        public double AccruedRevenue { get; set; }
        public double InvoiceRevenue { get; set; }
        public double AccruedCost { get; set; }
        public double AccruedGPM { get; set; }
        public double AccruedGPMPercent { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public string? Location { get; set; }
        public string? BusinessGroup { get; set; }
        public string? CustomerName { get; set; }
        public string? CurrencySymbol { get; set; }
        public int? DisplayCurrencyID { get; set; }
        public bool IsGrandTotal { get; set; }

    }

    // Added by Vyankat B. on 14-08-2026 - usp_Whizible2_CRW_seL_tbl_pm_ProjectProfitability_ProjectProfit_forProjectGroup_Total
    public class ProfDashProjectProfitGroupTotalModel
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ProjectCurrencyID { get; set; }
        public int? BaseCurrencyID { get; set; }
        public decimal AccruedRevenue { get; set; }
        public decimal InvoiceRevenue { get; set; }
        public decimal AccruedCost { get; set; }
        public decimal AccruedGPM { get; set; }
        public decimal AccruedGPMPercent { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public string? Location { get; set; }
        public string? BusinessGroup { get; set; }
        public string? CustomerName { get; set; }
        public string? CurrencySymbol { get; set; }
        public decimal AccruedRevenueTotal { get; set; }
        public decimal InvoiceRevenueTotal { get; set; }
        public decimal AccruedCostTotal { get; set; }
        public decimal AccruedGPMTotal { get; set; }
        public decimal AccruedGPMPercentTotal { get; set; }
        public decimal AccruedRevenueGrandTotal { get; set; }
        public decimal InvoiceRevenueGrandTotal { get; set; }
        public decimal AccruedCostGrandTotal { get; set; }
        public decimal AccruedGPMGrandTotal { get; set; }
        public decimal AccruedGPMPercentGrandTotal { get; set; }
    }

    #endregion

    // End of Added by Vyankat B. on 07-08-2026
}
