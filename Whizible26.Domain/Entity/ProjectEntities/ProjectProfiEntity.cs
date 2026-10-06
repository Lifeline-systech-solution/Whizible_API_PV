using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whizible26.Domain.Entity.ProjectEntities.ProjectProfiEntity
{
    //// Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability Entities

    // Added by Vyankat Bhure on 22-12-2025
    // Response model for usp_Whizible2_Sel_ProjectProfitability_PeridGraphData
    public class ProjectProfitabilityPeriodGraphDataModel
    {
        public string AsOn { get; set; } = string.Empty;

        public double? PeriodicAccruedResourceBillingTotal { get; set; }
        public double? PeriodicBillableOtherCostTotal { get; set; }
        public double? PeriodicTotalAccruedRevenue { get; set; }

        public double? PeriodicPeopleCosts { get; set; }
        public double? PeriodicDirectCost { get; set; }
        public double? PeriodicTotalAccruedCost { get; set; }

        public double? PeriodicGPM_Project { get; set; }

        public double? PeriodicInvoiceBillingTotal { get; set; }
        public double? PeriodicActualHoursTotal { get; set; }
    }

    // Added by Vyankat Bhure on 22-12-2025
    // Response model for usp_Whizible2_Sel_ProjectProfitability_FinacialData
    // Note: SP returns aggregated summary (single row) for all periods based on reporting frequency
    // SP returns: PlannedRevenue, PlannedCost, PlannedProfit, ActualRevenue (summed), ActualCost (summed), ActualProfit (summed), Variance (summed)
    // AsOn is kept for backward compatibility but will be null (SP no longer returns date)
    public class ProjectProfitabilityFinancialDataModel
    {
        public Decimal? PlannedRevenue { get; set; }
        public Decimal? PlannedCost { get; set; }
        public Decimal? PlannedProfit { get; set; }
        public Decimal? ActualRevenue { get; set; }
        public Decimal? ActualCost { get; set; }
        public Decimal? ActualProfit { get; set; }
        public Decimal? Variance { get; set; }
    }

    public class ProjectDetailRequest
    {
        public int? ProjectID { get; set; }
    }

    public class ProfitabilityRequest
    {
        public int ProjectID { get; set; }
        public string? ProjectName { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        //public int intReportFlag { get; set; }


    }

    public class ProjectDetailModel
    {       
        // text description of cost method (e.g. "Standard Resource Cost")
        public int CostMethod { get; set; }
        public int ReportFrequency { get; set; }


        // flags: sample shows 0/1 -> mapped to bool
        public bool HaveSubTaskTypes { get; set; }

        public bool ApplyEffortDistribution { get; set; }

        public int LocationID { get; set; }

        public DateTime? ExpectedStartDate { get; set; }

        public DateTime? ExpectedEndDate { get; set; }

        public bool Over { get; set; }

        public string? ProjectStatus { get; set; }

        // numeric validation flag or enum value
        public bool? ResourceValidation { get; set; }

        // whether project is billable
        public bool Billable { get; set; }

        // can be null in sample -> nullable int
        public int? RevisionNo { get; set; }

        public bool ResourceLevelTaskCompletion { get; set; }

        public bool EnforceConstraints { get; set; }

        // free text
        [MaxLength(500)]
        public string ProjectName { get; set; } = string.Empty;

        // actual start/end may be null
        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        // reference ids
        public int ContractType { get; set; }

        public int BaseCurrency { get; set; }

        // currency symbol including any punctuation (e.g. "Rs.")
        public string CurrencySymbol { get; set; } = string.Empty;

        // contract type label / node label e.g. "T&M by Resource"
        public string NodeLabel { get; set; } = string.Empty;

        // money values -> double is preferred for currency
        [DataType(DataType.Currency)]
        public double ContractValue { get; set; }

        // efforts often fractional; double is OK, or double if you need precision
        public double EstimatedEfforts { get; set; }

        // working hours per day (can be fractional)
        public double WorkingHours { get; set; }
    }

    public class ProjectProfitabilityPeriodModel
    {
        /// <summary>
        /// Formatted date string (dd-MMM-yyyy) used as ID in legacy UI
        /// </summary>
        public string ID { get; set; } = string.Empty;

        /// <summary>
        /// Formatted period string (dd-MMM-yyyy)
        /// </summary>
        public string Period { get; set; } = string.Empty;
    }

    public class ProjectProfitabilityPeriodsRequest
    {
        public int ProjectID { get; set; }
        public string WhichDate { get; set; } = "F"; // 'F' or other -> ToDate
    }

    // Added by Vyankat Bhure on 24-12-2025
    // Response model for usp_Whizible2_Sel_ProjectProfitability_Periods
    public class ProjectProfitabilityPeriodsResponse
    {
        public string ID { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
    }
    // End of Added by Vyankat Bhure on 24-12-2025

    public class ProjectProfitabilityTrendGraph1Model
    {
        public string AsOn { get; set; } = string.Empty;
        public double? ResourceCost { get; set; }
        public double? OtherCost { get; set; }
        public double? TotalCost { get; set; }
    }

    public class ProjectProfitabilityTrendGraph2Model
    {
        public string AsOn { get; set; } = string.Empty;
        public double? ResourceRevenue { get; set; }
        public double? AccruedBillableExpenses { get; set; }
        public double? InvoiceRevenue { get; set; }
    }

    public class ProjectProfitabilityTrendGraph3Model
    {
        public string AsOn { get; set; } = string.Empty;
        public double? TotalCost { get; set; }
        public double? Revenue { get; set; }
        public double? GPM { get; set; }
    }

    public class TrendsRequest
    {
        public int ProjectID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? GraphId { get; set; }
    }
    public class ProfitabilityReportData
    {
        // --- Header Information (Repeated in every row by the SP) ---
        public string ProjectName { get; set; }
        public double ProjectValue { get; set; }
        public string CurrencySymbol { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }

        // --- Grid Data Columns ---
        public DateTime AsOn { get; set; } // Maps to 'As On' column
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double PeriodicAccruedActualHoursTotal { get; set; }
        public double CumulativeAccruedActualHoursTotal { get; set; }

        // People Cost
        public double PeriodicAccruedResourceCostTotal { get; set; }
        public double CumulativeAccruedResourceCostTotal { get; set; }

        // Accrued Billed Hours
        public double PeriodicAccruedBilledHoursTotal { get; set; }
        public double CumulativeAccruedBilledHoursTotal { get; set; }

        // Expenses
        public double PeriodicOtherCostTotal { get; set; }
        public double CumulativeOtherCostTotal { get; set; }

        // Accrued People Revenue
        public double PeriodicAccruedResourceBillingTotal { get; set; }
        public double CumulativeAccruedResourceBillingTotal { get; set; }

        // Accrued Billable Expenses
        public double PeriodicBillableOtherCostTotal { get; set; }
        public double CumulativeBillableOtherCostTotal { get; set; }

        // GPM
        public double PeriodicGPM_Project { get; set; }
        public double CumulativeGPM_Project { get; set; }

        // Invoice Revenue
        public double PeriodicInvoiceBillingTotal { get; set; }
        public double CumulativeInvoiceBillingTotal { get; set; }

        // Base Currency - People Cost
        public double BasePeriodicAccruedResourceCostTotal { get; set; }
        public double BaseCumulativeAccruedResourceCostTotal { get; set; }

        // Base Currency - Accrued People Revenue
        public double BasePeriodicAccruedResourceBillingTotal { get; set; }
        public double BaseCumulativeAccruedResourceBillingTotal { get; set; }

    }


    public class CompanyDataEntity
    {
        public string? CompanyName { get; set; }      
    }

    // Added by Vyankat Bhure on 18-Feb-2026 - Response model for Company Logo
    public class CompanyLogoModel
    {
        public string? OriginalFileName { get; set; }
        public string? SystemFileName { get; set; }
    }


    // Added by Vyankat Bhure on 15-12-2025 to receive cost trend request data for Project Profitability
    public class CostTrendRequest
    {
        public int ProjectID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    //End of Added by Vyankat Bhure on 15-12-2025 to receive cost trend request data for Project Profitability

    // Added by Vyankat Bhure on 15-12-2025  to represent Project Profitability cost trend response data
    public class CostTrendResponse
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public DateTime AsOn { get; set; }
        public Double ResourceCost { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Double TotalCost { get; set; }
        public Double? TravelAllowance { get; set; }
        

    }
    // End of Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability cost trend response data

    // Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability revenue trend response data
    public class RevenueTrendResponse
    {
        public DateTime AsOn { get; set; }
        public Double AccruedRevenue { get; set; }
        public Double InvoicedRevenue { get; set; }
        public Double UnbilledRevenue { get; set; }
    }
    //End of Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability revenue trend response data

    // Added by Vyankat Bhure on 15-12-2025 to represent latest GPM (Gross Profit Margin) data for a Project Profitability
    public class GPMTrendResponse
    {
        public int ProjectProfitabilityID { get; set; }
        public int ProjectID { get; set; }
        public double PeopleRevenue { get; set; }
        public double AccruedBillableExpenses { get; set; }
        public double Cost { get; set; }
        public double Total { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double PeopleCost { get; set; }
        public double TotalCost { get; set; }
        public double BasePeopleRevenue { get; set; }
        public double BaseAccruedBillableExpenses { get; set; }
        public double BaseCost { get; set; }
        public double BaseTotal { get; set; }
        public double BasePeopleCost { get; set; }
        public double BaseTotalCost { get; set; }
        public string Revenue { get; set; }
    }
    // End ofAdded by Vyankat Bhure on 15-12-2025 to represent latest GPM (Gross Profit Margin) data for a Project Profitability

    // Added by Vyankat Bhure on 15-12-2025 to receive GPM request data for Project Profitability
    public class GPMRequest
    {
        public int ProjectID { get; set; }
    }
    // End of Added by Vyankat Bhure on 15-12-2025 to receive GPM request data for Project Profitability


    // Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability snapshot response data
    public class SnapshotTrendResponse
    {
        public int ProjectProfitabilityID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Status { get; set; }
        public DateTime LastGeneratedOn { get; set; }
        public DateTime? GeneratedDate { get; set; }
    }
    // End of Added by Vyankat Bhure on 15-12-2025 to represent Project Profitability snapshot response data

    // Added by Vyankat Bhure on 15-12-2025 - Request model for Project Profitability report
    public class ProjectProfitabilityRequest
    {
        public int ProjectID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int ReportFlag { get; set; }  // 1=Periodic, 2=Detailed, 3=Cumulative Summary
    }
    // End of ProjectProfitabilityRequest model


    // Added by Vyankat Bhure on 15-12-2025 
    // Response model for Project Profitability – Periodic Summary View (Flag = 1)
    public class ProjectProfitabilityPeriodicResponse
    {
        public string FromDate { get; set; }
        public string AsOn { get; set; }

        public double PeriodicAccruedResourceBillingTotal { get; set; }
        public double PeriodicBillableOtherCostTotal { get; set; }
        public double PeriodicTotalAccruedRevenue { get; set; }

        public double PeriodicPeopleCosts { get; set; }
        public double PeriodicDirectCost { get; set; }
        public double PeriodicTotalAccruedCost { get; set; }

        public double PeriodicGPM_Project { get; set; }

        public double PeriodicInvoiceBillingTotal { get; set; }
        public double PeriodicActualHoursTotal { get; set; }
    }
    // End of ProjectProfitabilityPeriodicResponse model

    // Added by Vyankat Bhure on 15-12-2025 
    // Response model for Project Profitability – Detailed View (Flag = 2)
    public class ProjectProfitabilityDetailedResponse
    {
        public string FromDate { get; set; }

        public string ReportingDate { get; set; }

        public double PeriodicActualHoursTotal { get; set; }
        public double CumulativeActualHoursTotal { get; set; }

        public double PeriodicPeopleCosts { get; set; }
        public double CumulativePeopleCosts { get; set; }

        public double PeriodicDirectCost { get; set; }
        public double CumulativeDirectCost { get; set; }

        public double PeriodicAccruedResourceBillingTotal { get; set; }
        public double CumulativeAccruedResourceBillingTotal { get; set; }

        public double PeriodicBillableOtherCostTotal { get; set; }
        public double CumulativeBillableOtherCostTotal { get; set; }

        public double PeriodicAccruedGPM { get; set; }
        public double CumulativeAccruedGPM { get; set; }

        public double PeriodicInvoicedRevenue { get; set; }
        public double CumulativeInvoicedRevenue { get; set; }
    }
    // End of ProjectProfitabilityDetailedResponse model


    // Added by Vyankat Bhure on 15-12-2025 
    // Response model for Project Profitability – Cumulative Revenue/Cost/GPM View (Flag = 3)
    public class ProjectProfitabilityCumulativeResponse
    {
        public string FromDate { get; set; }

        public string AsOn { get; set; }

        public double CumulativeAccruedRevenue { get; set; }
        public double CumulativeBillableOtherCostTotal { get; set; }
        public double CumulativeTotalAccruedRevenue { get; set; }

        public double CumulativePeopleCosts { get; set; }
        public double CumulativeDirectCost { get; set; }
        public double CumulativeTotalAccruedCost { get; set; }

        public double CumulativeGPM_Project { get; set; }

        public double CumulativeInvoiceBillingTotal { get; set; }
        public double CumulativeActualHoursTotal { get; set; }
    }
    // End of ProjectProfitabilityCumulativeResponse model

    // Added by Vyankat Bhure on 15-12-2025 - Request model for fetching Resource Profitability based on Cost Type
    public class ResourceProfitabilityRequest
    {
        public int ProjectID { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string CostType { get; set; } = string.Empty;
    }
    // End of Added by Vyankat Bhure on 15-12-2025 - Request model for fetching Resource Profitability based on Cost Type


    // Added by Vyankat Bhure on 15-12-2025 - Response model for Resource Profitability details
    public class ResourceProfResponse
    {
        public string? EmployeeName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Double? ResourceCost { get; set; }

    }
    // End of Added by Vyankat Bhure on 15-12-2025 - Response model for Resource Profitability details

    // Added by Vyankat Bhure on 15-12-2025 - Request model for Project Profitability Generate/ReGenerate sp
    public class ProjectProfiRequest
    {
        public int ProjectID { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        public int DtFlag { get; set; } 

    }
    // End of Added by Vyankat Bhure on 15-12-2025 - Request model for Project Profitability Generate/ReGenerate sp

    // Added by Vyankat Bhure on 22-12-2025 - Request model for Project Financial Data
    public class ProjProfFinancialRequest
    {
        public int ProjectID { get; set; }
       
    }
    // End of Added by Vyankat Bhure on 22-12-2025 - Request model for Project Financial Data

    // Added by Vyankat Bhure on 22-12-2025 - Response model for Project Expense Trend Data
    public class ProjectExpenseTrendModel
    {
        public string? CostHeads { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public double? Expense { get; set; }
    }
    // End of Added by Vyankat Bhure on 22-12-2025 - Response model for Project Expense Trend Data

    // Added by Vyankat Bhure on 22-12-2025 - Response model for Project Milestone Cost Data
    public class ProjectMilestoneCostResponse
    {
        public string? MileStone { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public double? MilestoneCost { get; set; }
    }
    // End of Added by Vyankat Bhure on 22-12-2025 - Response model for Project Milestone Cost Data

    public class ProjectContractTypeCurrencyModel
    {
        public int? ContractType { get; set; }
        public int? BillingCurrencyID { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrencyCode { get; set; }
        public string? MajorCurrencyUnit { get; set; }
        public string? MinorCurrencyUnit { get; set; }


    }


    // Added by Vyankat Bhure on 26-12-2025 - Request model for Update Company Information
    public class UpdateCompanyInformationRequest
    {
        public int CostMethod { get; set; }
    }
    // End of Added by Vyankat Bhure on 26-12-2025 - Request model for Update Company Information

    // Added by Vyankat Bhure on 26-12-2025 - Request model for Update Company Information Reporting Frequency
    public class UpdateCompanyInformationReportingFrequencyRequest
    {
        public int ReportingFrequency { get; set; }
    }
    // End of Added by Vyankat Bhure on 26-12-2025 - Request model for Update Company Information Reporting Frequency

    // Added by Vyankat Bhure on 12-12-2025 - Request model for Get Project Profitability Date Range
    public class ProjectDateRangeRequest
    {
        public int ProjectID { get; set; }
    }
    // End of Added by Vyankat Bhure on 12-12-2025 - Request model for Get Project Profitability Date Range

    // Added by Vyankat Bhure on 12-12-2025 - Response model for Get Project Profitability Date Range
    public class ProjectDateRangeResponse
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    // End of Added by Vyankat Bhure on 12-12-2025 - Response model for Get Project Profitability Date Range

    

    // Added by Vyankat Bhure on 13-01-2025 - Request model for Get Project Profitability ToDate
    public class ProjectProfitabilityToDateRequest
    {
        public int ProjectID { get; set; }
        public DateTime? FromDate { get; set; }
    }
    // End of Added by Vyankat Bhure on 13-01-2025 - Request model for Get Project Profitability ToDate

    // Added by Vyankat Bhure on 13-01-2025 - Response model for Get Project Profitability ToDate
    public class ProjectProfitabilityToDateModel
    {
        public DateTime? ToDate { get; set; }
    }
    // End of Added by Vyankat Bhure on 13-01-2025 - Response model for Get Project Profitability ToDate



}

