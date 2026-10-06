using System;
using System.Collections.Generic;

namespace Whizible26.Domain.Entity.DashboardEntities
{
    #region Filter masters

    public class ForecastingBusinessGroupModel
    {
        public int BusinessGroupID { get; set; }
        public string BusinessGroup { get; set; } = string.Empty;
    }

    public class ForecastingOriginationUnitModel
    {
        public int LocationID { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class ForecastingDepartmentModel
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; } = string.Empty;
    }

    public class ForecastingRoleModel
    {
        public int RoleID { get; set; }
        public string RoleDescription { get; set; } = string.Empty;
    }

    public class ForecastingFilterMastersModel
    {
        public List<ForecastingBusinessGroupModel> BusinessGroups { get; set; } = new();
        public List<ForecastingOriginationUnitModel> OriginationUnits { get; set; } = new();
        public List<ForecastingDepartmentModel> Departments { get; set; } = new();
        public List<ForecastingRoleModel> Roles { get; set; } = new();
    }

    #endregion

    #region Request

    public class ForecastingFilterRequest
    {
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public int? DepartmentID { get; set; }
        public int? RoleID { get; set; }
        public string? SearchText { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ForecastingExportRequest : ForecastingFilterRequest
    {
        public string? CompanyLogo { get; set; }
    }

    #endregion

    #region KPI summary

    public class ForecastingKpiSummaryModel
    {
        public int TotalResources { get; set; }
        public int TotalResourcesCompanyWide { get; set; }
        public double TotalPlannedHours { get; set; }
        public double TotalActualHours { get; set; }
        public double RemainingHours => TotalPlannedHours - TotalActualHours < 0 ? 0 : TotalPlannedHours - TotalActualHours;
        public double UtilizationPercent { get; set; }
        public string UtilizationLabel { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
    }

    public class ForecastingKpiRawModel
    {
        public int TotalResources { get; set; }
        public int TotalResourcesCompanyWide { get; set; }
        public double TotalPlannedHours { get; set; }
        public double TotalActualHours { get; set; }
        public int TotalRecords { get; set; }
    }

    #endregion

    #region Monthly grid

    public class ForecastingMonthColumnModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ForecastingMonthRowModel
    {
        public string ResourceName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public int? DepartmentID { get; set; }
        public int? RoleID { get; set; }
        public string RoleDescription { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public double January { get; set; }
        public double February { get; set; }
        public double March { get; set; }
        public double April { get; set; }
        public double May { get; set; }
        public double June { get; set; }
        public double July { get; set; }
        public double August { get; set; }
        public double September { get; set; }
        public double October { get; set; }
        public double November { get; set; }
        public double December { get; set; }

        public double January_Ph { get; set; }
        public double February_Ph { get; set; }
        public double March_Ph { get; set; }
        public double April_Ph { get; set; }
        public double May_Ph { get; set; }
        public double June_Ph { get; set; }
        public double July_Ph { get; set; }
        public double August_Ph { get; set; }
        public double September_Ph { get; set; }
        public double October_Ph { get; set; }
        public double November_Ph { get; set; }
        public double December_Ph { get; set; }

        public string ReportPeriod { get; set; } = string.Empty;
        public string RequestedRoleDescription { get; set; } = string.Empty;
    }

    #endregion

    #region Weekly grid

    public class ForecastingWeekColumnModel
    {
        public int WKNo { get; set; }
        public DateTime WkStartDate { get; set; }
        public int SequenceNo { get; set; }
    }

    public class ForecastingWeekRowModel
    {
        public string ResourceName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public int? BusinessGroupID { get; set; }
        public int? LocationID { get; set; }
        public int? DepartmentID { get; set; }
        public int? RoleID { get; set; }
        public string RoleDescription { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public double W1 { get; set; }
        public double W1_Ah { get; set; }
        public double W2 { get; set; }
        public double W2_Ah { get; set; }
        public double W3 { get; set; }
        public double W3_Ah { get; set; }
        public double W4 { get; set; }
        public double W4_Ah { get; set; }
        public double W5 { get; set; }
        public double W5_Ah { get; set; }
        public double W6 { get; set; }
        public double W6_Ah { get; set; }
        public double W7 { get; set; }
        public double W7_Ah { get; set; }
        public double W8 { get; set; }
        public double W8_Ah { get; set; }
        public double W9 { get; set; }
        public double W9_Ah { get; set; }
        public double W10 { get; set; }
        public double W10_Ah { get; set; }
        public double W11 { get; set; }
        public double W11_Ah { get; set; }
        public double W12 { get; set; }
        public double W12_Ah { get; set; }
        public double W13 { get; set; }
        public double W13_Ah { get; set; }
        public double W14 { get; set; }
        public double W14_Ah { get; set; }
        public double W15 { get; set; }
        public double W15_Ah { get; set; }
        public double W16 { get; set; }
        public double W16_Ah { get; set; }
        public double W17 { get; set; }
        public double W17_Ah { get; set; }
        public double W18 { get; set; }
        public double W18_Ah { get; set; }
        public double W19 { get; set; }
        public double W19_Ah { get; set; }
        public double W20 { get; set; }
        public double W20_Ah { get; set; }
        public double W21 { get; set; }
        public double W21_Ah { get; set; }
        public double W22 { get; set; }
        public double W22_Ah { get; set; }
        public double W23 { get; set; }
        public double W23_Ah { get; set; }
        public double W24 { get; set; }
        public double W24_Ah { get; set; }
        public double W25 { get; set; }
        public double W25_Ah { get; set; }
        public double W26 { get; set; }
        public double W26_Ah { get; set; }
        public double W27 { get; set; }
        public double W27_Ah { get; set; }
        public double W28 { get; set; }
        public double W28_Ah { get; set; }
        public double W29 { get; set; }
        public double W29_Ah { get; set; }
        public double W30 { get; set; }
        public double W30_Ah { get; set; }
        public double W31 { get; set; }
        public double W31_Ah { get; set; }
        public double W32 { get; set; }
        public double W32_Ah { get; set; }
        public double W33 { get; set; }
        public double W33_Ah { get; set; }
        public double W34 { get; set; }
        public double W34_Ah { get; set; }
        public double W35 { get; set; }
        public double W35_Ah { get; set; }
        public double W36 { get; set; }
        public double W36_Ah { get; set; }
        public double W37 { get; set; }
        public double W37_Ah { get; set; }
        public double W38 { get; set; }
        public double W38_Ah { get; set; }
        public double W39 { get; set; }
        public double W39_Ah { get; set; }
        public double W40 { get; set; }
        public double W40_Ah { get; set; }
        public double W41 { get; set; }
        public double W41_Ah { get; set; }
        public double W42 { get; set; }
        public double W42_Ah { get; set; }
        public double W43 { get; set; }
        public double W43_Ah { get; set; }
        public double W44 { get; set; }
        public double W44_Ah { get; set; }
        public double W45 { get; set; }
        public double W45_Ah { get; set; }
        public double W46 { get; set; }
        public double W46_Ah { get; set; }
        public double W47 { get; set; }
        public double W47_Ah { get; set; }
        public double W48 { get; set; }
        public double W48_Ah { get; set; }
        public double W49 { get; set; }
        public double W49_Ah { get; set; }
        public double W50 { get; set; }
        public double W50_Ah { get; set; }
        public double W51 { get; set; }
        public double W51_Ah { get; set; }
        public double W52 { get; set; }
        public double W52_Ah { get; set; }
        public double W53 { get; set; }
        public double W53_Ah { get; set; }

        public string ReportPeriod { get; set; } = string.Empty;
    }

    #endregion

    #region Composite response / paging envelope

    public class PagedResult<T>
    {
        public List<T> Rows { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
    }

    public class ForecastingMonthlyResponseModel
    {
        public ForecastingKpiSummaryModel Kpi { get; set; } = new();
        public List<ForecastingMonthColumnModel> MonthColumns { get; set; } = new();
        public PagedResult<ForecastingMonthRowModel> Grid { get; set; } = new();
    }

    public class ForecastingWeeklyResponseModel
    {
        public ForecastingKpiSummaryModel Kpi { get; set; } = new();
        public List<ForecastingWeekColumnModel> WeekColumns { get; set; } = new();
        public PagedResult<ForecastingWeekRowModel> Grid { get; set; } = new();
    }

    #endregion
}