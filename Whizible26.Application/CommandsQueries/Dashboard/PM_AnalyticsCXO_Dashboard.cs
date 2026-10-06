using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using InitiativeNextGen.Infra.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.Repository.DashboardRepo;
using Whizible26.Domain.Entity.DashboardEntities;
using WhizibleTeams.Domain.Entity;

namespace Whizible26.Application.CommandsQueries.Dashboard
{
    public class PM_AnalyticsCXO_Dashboard
    {
        private readonly IConfiguration _configuration;
        private readonly PM_AnalyticsCXO_DashboardRepo _repository;
        private readonly string _connectionString;

        /* Chip multi-select masters (omit Flag loads these). */
        private static readonly HashSet<string> ChipFlags = new(StringComparer.OrdinalIgnoreCase)
        {
            "Portfolio",
            "Customer",
            "Region",
            "BillingType",
            "Health",
            "ProjectManager"
        };

        /* All FlagWise values including HighLevelRole for execRoleBtn. */
        private static readonly HashSet<string> AllowedFlags = new(StringComparer.OrdinalIgnoreCase)
        {
            "Portfolio",
            "Customer",
            "Region",
            "BillingType",
            "Health",
            "ProjectManager",
            "HighLevelRole"
        };

        // =====================================================================
        // KPI cards — simple Flag → SP map (same chip + calendar inputs for all)
        // DipalI v On 10th sep 2026
        // Flag          | SP name
        // Revenue       | usp_Whizible2_Sel_AnalyticsDBKPI_Revenue
        // GrossProfit   | usp_Whizible2_Sel_AnalyticsDBKPI_GrossProfit
        // GrossMargin   | usp_Whizible2_Sel_AnalyticsDBKPI_GrossMargin
        // EBIT          | usp_Whizible2_Sel_AnalyticsDBKPI_EBIT
        // NetProfit     | usp_Whizible2_Sel_AnalyticsDBKPI_NetProfit
        // Utilization   | usp_Whizible2_Sel_AnalyticsDBKPI_Utilization
        // Bench         | usp_Whizible2_Sel_AnalyticsDBKPI_Bench
        // ActiveProjects| usp_Whizible2_Sel_AnalyticsDBKPI_ActiveProjects
        // DelayedProjects| usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjects
        // BudgetVariance| usp_Whizible2_Sel_AnalyticsDBKPI_BudgetVariance
        // CSAT          | usp_Whizible2_Sel_AnalyticsDBKPI_CSAT
        // PortfolioHealth| usp_Whizible2_Sel_AnalyticsDBKPI_PortfolioHealth
        // Added by Vikas T on 16-09-2026 - Util/Bench/Active bind live SPs at DrillLevel 0 (KPI card).
        // =====================================================================
        private sealed class KpiFlagDef
        {
            public string Flag { get; set; } = "";
            public string Label { get; set; } = "";
            public string SpName { get; set; } = "";
            public string Unit { get; set; } = "";
            public bool GoodUp { get; set; } = true;
        }

        private static readonly KpiFlagDef[] KpiCardRegistry =
        {
            new KpiFlagDef { Flag = "Revenue",          Label = "Revenue",               SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_Revenue",          Unit = "Cr",    GoodUp = true  },
            new KpiFlagDef { Flag = "GrossProfit",      Label = "Gross Profit",          SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_GrossProfit",      Unit = "Cr",    GoodUp = true  },
            new KpiFlagDef { Flag = "GrossMargin",      Label = "Gross Margin",          SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_GrossMargin",      Unit = "Pct",   GoodUp = true  },
            new KpiFlagDef { Flag = "EBIT",             Label = "EBIT",                  SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_EBIT",             Unit = "Cr",    GoodUp = true  },
            new KpiFlagDef { Flag = "NetProfit",        Label = "Net Profit",            SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_NetProfit",        Unit = "Cr",    GoodUp = true  },
            // Added by Vikas T on 16-09-2026 - Resource Utilization card → usp_Whizible2_Sel_AnalyticsDBKPI_Utilization (@DrillLevel=0)
            new KpiFlagDef { Flag = "Utilization",      Label = "Resource Utilization",  SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_Utilization",      Unit = "Pct",   GoodUp = true  },
            // Added by Vikas T on 16-09-2026 - Bench card → usp_Whizible2_Sel_AnalyticsDBKPI_Bench (@DrillLevel=0); decrease = favourable
            new KpiFlagDef { Flag = "Bench",            Label = "Bench",                 SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_Bench",            Unit = "FTE",   GoodUp = false },
            // Added by Vikas T on 16-09-2026 - Active Projects card → usp_Whizible2_Sel_AnalyticsDBKPI_ActiveProjects (@DrillLevel=0)
            new KpiFlagDef { Flag = "ActiveProjects",   Label = "Active Projects",       SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_ActiveProjects",   Unit = "Count", GoodUp = true  },
            new KpiFlagDef { Flag = "DelayedProjects",  Label = "Delayed Projects",      SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjects",  Unit = "Count", GoodUp = false },
            new KpiFlagDef { Flag = "BudgetVariance",   Label = "Budget Variance",       SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_BudgetVariance",   Unit = "Pct",   GoodUp = false },
            new KpiFlagDef { Flag = "CSAT",             Label = "Customer CSAT",         SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_CSAT",             Unit = "Score", GoodUp = true  },
            new KpiFlagDef { Flag = "PortfolioHealth",  Label = "AI Portfolio Health",   SpName = "usp_Whizible2_Sel_AnalyticsDBKPI_PortfolioHealth",  Unit = "Score", GoodUp = true  }
        };

        // Added by Vikas T on 16-09-2026 - These three SPs default DrillLevel=1 (drill); cards must force 0.
        private static readonly HashSet<string> KpiFlagsRequiringDrillLevelZero =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Utilization",
                "Bench",
                "ActiveProjects"
            };

        // Added by Dipali V. on 08-09-2026 - Default constructor required by the repository pattern
        public PM_AnalyticsCXO_Dashboard() { }
        // End of default constructor

        // Added by Dipali V. on 08-09-2026 - Initialise the dashboard service with configuration
        public PM_AnalyticsCXO_Dashboard(IConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _connectionString = CommonFunctions.General.BuildConnectionString(
                    _configuration.GetSection("ConnectionStrings:WhizibleDbConnection").Value);
                _repository = new PM_AnalyticsCXO_DashboardRepo(_connectionString);
            }
        }
        // End of Added by Dipali V. on 08-09-2026

        #region Helpers

        private ResponseEntity RepositoryNotConfigured()
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message = "Database connection is not configured." }
            };
        }

        private static string? NullIfBlank(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;
            return trimmed;
        }

        private static string NormalizeFlag(string flag)
        {
            return flag.Replace(" ", "", StringComparison.Ordinal);
        }

        private static bool IsCustomFlag(string flag)
        {
            if (flag.Equals("Custom", StringComparison.OrdinalIgnoreCase)
                || flag.Equals("0", StringComparison.OrdinalIgnoreCase))
                return true;

            return int.TryParse(flag, out var filterId) && filterId == 0;
        }

        private static DateTime? ParseDateOrNull(string? value)
        {
            var trimmed = NullIfBlank(value);
            if (trimmed == null) return null;
            if (DateTime.TryParse(trimmed, out var dt)) return dt.Date;
            return null;
        }

        private static bool TryParsePositiveInt(string? value, out int id)
        {
            id = 0;
            var trimmed = NullIfBlank(value);
            if (trimmed == null) return false;
            return int.TryParse(trimmed, out id) && id > 0;
        }

        private static bool IsValidDashboardId(int dashboardId) => dashboardId > 0;

        private static T? FirstRowOf<T>(object? result) where T : class
        {
            /* Purpose: Avoid FirstOrDefault on dynamic — runtime binder cannot resolve LINQ extensions. */
            var list = ExtractList<T>(result);
            return list.Count > 0 ? list[0] : null;
        }

        // DipalI v On 9th sep 2026 — GetAsyncSP returns ExpandoObject; System.Text.Json serializes that as {}.
        // Unwrap to List so the page receives a real JSON array.
        private static List<T> ExtractList<T>(object? result)
        {
            if (result == null) return new List<T>();
            if (result is List<T> list) return list;
            if (result is IEnumerable<T> seq) return seq.ToList();
            if (result is IDictionary<string, object> bag)
            {
                foreach (var kv in bag)
                {
                    if (kv.Value is List<T> typed) return typed;
                    if (kv.Value is IEnumerable<T> typedSeq) return typedSeq.ToList();
                }
            }
            return new List<T>();
        }

        private static ResponseEntity Success(string message, object data)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.SUCCESS,
                Data = new { message, data }
            };
        }

        private static ResponseEntity Failure(Exception ex)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new
                {
                    message = ex.Message
                        + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "")
                }
            };
        }

        private static ResponseEntity Failure(string message)
        {
            return new ResponseEntity
            {
                Status = ResponseStatus.FAILURE,
                Data = new { message }
            };
        }

        #endregion

        // Added by Dipali V. on 08-09-2026 - Flag-wise filter master (ID, Name)
        // Revised 10-09-2026: Flag blank = all chips in one response for multi-select UI bind.
        public async Task<ResponseEntity> GetAnalyticsDBFilterFlagWise(AnalyticsDBFilterFlagWiseRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBFilterFlagWiseRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var flags = ResolveFlagList(request.Flag);
                if (flags.Count == 0)
                    return Failure("Flag must be Portfolio, Customer, Region, BillingType, Health, ProjectManager or HighLevelRole (or omit Flag to load chip masters).");

                // Purpose: one SP call per flag; bag keyed by Flag so UI binds every chip without N separate endpoints.
                var bag = new Dictionary<string, List<AnalyticsDBFilterFlagWiseModel>>(StringComparer.OrdinalIgnoreCase);
                foreach (var flag in flags)
                {
                    bag[flag] = await LoadFlagWiseRows(flag, request.DashboardID);
                }

                return Success("Analytics DB filter list", bag);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBFilterFlagWise

        // Added by Dipali - Portfolio-dependent chip masters
        // SP: usp_Whizible2_Sel_AnalyticsDBFilterFlagWise_Dependent
        public async Task<ResponseEntity> GetAnalyticsDBFilterFlagWiseDependent(
            AnalyticsDBFilterFlagWiseDependentRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBFilterFlagWiseDependentRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var flags = ResolveDependentFlagList(request.Flag);
                if (flags.Count == 0)
                    return Failure("Flag must be Customer, Region, BillingType, Health, ProjectManager (or omit Flag to load dependent chips).");

                var portfolioIds = NullIfBlank(request.PortfolioIDs);
                var bag = new Dictionary<string, List<AnalyticsDBFilterFlagWiseModel>>(StringComparer.OrdinalIgnoreCase);
                foreach (var flag in flags)
                {
                    bag[flag] = await LoadFlagWiseDependentRows(flag, portfolioIds, request.DashboardID);
                }

                return Success("Analytics DB dependent filter list", bag);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBFilterFlagWiseDependent — Added by Dipali

        /* Purpose: Blank Flag → dependent chips only (not Portfolio / HighLevelRole). Added by Dipali */
        private static List<string> ResolveDependentFlagList(string? flag)
        {
            var trimmed = NullIfBlank(flag);
            if (trimmed == null)
            {
                return new List<string>
                {
                    "Customer",
                    "ProjectManager",
                    "Health",
                    "Region",
                    "BillingType"
                };
            }

            var normalized = NormalizeFlag(trimmed);
            if (normalized.Equals("Portfolio", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("HighLevelRole", StringComparison.OrdinalIgnoreCase))
                return new List<string>();

            if (!ChipFlags.Contains(normalized)) return new List<string>();
            return new List<string> { normalized };
        }

        /* Purpose: Call usp_Whizible2_Sel_AnalyticsDBFilterFlagWise_Dependent for one chip. Added by Dipali */
        private async Task<List<AnalyticsDBFilterFlagWiseModel>> LoadFlagWiseDependentRows(
            string flag, string? portfolioIds, int dashboardId)
        {
            if (_repository == null) return new List<AnalyticsDBFilterFlagWiseModel>();
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@Flag", flag),
                new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                {
                    Value = (object?)portfolioIds ?? DBNull.Value
                },
                new SqlParameter("@DashboardID", dashboardId)
            };
            var result = await _repository.GetAsyncSP<AnalyticsDBFilterFlagWiseModel>(
                "usp_Whizible2_Sel_AnalyticsDBFilterFlagWise_Dependent", sqlParams);
            return ExtractList<AnalyticsDBFilterFlagWiseModel>(result);
        }

        /* Purpose: Blank Flag → chip masters + HighLevelRole (one round-trip for CXO boot).
           Added by Dipali — omit Flag also returns HighLevelRole for the role pill. */
        private static List<string> ResolveFlagList(string? flag)
        {
            var trimmed = NullIfBlank(flag);
            if (trimmed == null)
                return AllowedFlags.OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();

            var normalized = NormalizeFlag(trimmed);
            if (normalized.Equals("HighLevelrole", StringComparison.OrdinalIgnoreCase))
                normalized = "HighLevelRole";
            if (!AllowedFlags.Contains(normalized)) return new List<string>();
            return new List<string> { normalized };
        }

        // Added by Dipali V. on 10-09-2026 - Top 1 employee name for selected HighLevelRole
        public async Task<ResponseEntity> GetAnalyticsDBRoleGreeting(AnalyticsDBRoleGreetingRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBRoleGreetingRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");
                if (request.RoleID <= 0)
                    return Failure("RoleID is required.");

                int? userId = null;
                if (TryParsePositiveInt(request.UserID, out var parsedUserId))
                    userId = parsedUserId;

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@RoleID", request.RoleID),
                    new SqlParameter("@UserID", (object?)userId ?? DBNull.Value),
                    new SqlParameter("@DashboardID", request.DashboardID)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBRoleGreetingModel>(
                    "usp_Whizible2_Sel_AnalyticsDBRoleGreeting", sqlParams);

                return Success("Analytics DB role greeting", FirstRowOf<AnalyticsDBRoleGreetingModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBRoleGreeting

        /* Purpose: Call usp_Whizible2_Sel_AnalyticsDBFilterFlagWise for one chip. */
        private async Task<List<AnalyticsDBFilterFlagWiseModel>> LoadFlagWiseRows(string flag, int dashboardId)
        {
            if (_repository == null) return new List<AnalyticsDBFilterFlagWiseModel>();
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@Flag", flag),
                new SqlParameter("@DashboardID", dashboardId)
            };
            var result = await _repository.GetAsyncSP<AnalyticsDBFilterFlagWiseModel>(
                "usp_Whizible2_Sel_AnalyticsDBFilterFlagWise", sqlParams);
            return ExtractList<AnalyticsDBFilterFlagWiseModel>(result);
        }

        // Added by Dipali V. on 08-09-2026 - Date filter options from tbl_Whizible2_AnalyticsDBFilter
        public async Task<ResponseEntity> GetPageGenericfilters(AnalyticsDBPageGenericFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBPageGenericFilterRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@IsComparison",
                        request.IsComparison.HasValue
                            ? (object)request.IsComparison.Value
                            : DBNull.Value)
                };

                var result = await _repository.GetAsyncSP<PageGenericFilterModel>(
                    "usp_Whizible2_Sel_PageGenericFilters", sqlParams);

                return Success("Analytics DB date filters", ExtractList<PageGenericFilterModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetPageGenericfilters

        // Added by Dipali V. on 08-09-2026 - Date picker. Original UDF output unchanged.
        public async Task<ResponseEntity> GetAnalyticsDBFilterDateRange(AnalyticsDBFilterDateRangeRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBFilterDateRangeRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var flag = NullIfBlank(request.Flag);
                if (flag == null)
                    return Failure("Flag is required.");

                var startDate = ParseDateOrNull(request.StartDate);
                var endDate = ParseDateOrNull(request.EndDate);
                var isCustom = IsCustomFlag(flag);

                if (isCustom)
                {
                    if (startDate == null || endDate == null)
                        return Failure("StartDate and EndDate are required when Flag is Custom.");
                    if (startDate > endDate)
                        return Failure("StartDate cannot be after EndDate.");
                }
                else
                {
                    startDate = null;
                    endDate = null;
                }

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@Flag", flag),
                    new SqlParameter("@StartDate", (object?)startDate ?? DBNull.Value),
                    new SqlParameter("@EndDate", (object?)endDate ?? DBNull.Value),
                    new SqlParameter("@DashboardID", request.DashboardID)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBFilterDateRangeModel>(
                    "usp_Whizible2_Sel_AnalyticsDBFilterDateRange", sqlParams);

                return Success("Analytics DB filter date range", ExtractList<AnalyticsDBFilterDateRangeModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBFilterDateRange

        // Added by Dipali V. on 09-09-2026 - Comparison only (IsComparison = 1)
        public async Task<ResponseEntity> GetAnalyticsDBFilterDateRangeUpdated(
            AnalyticsDBFilterDateRangeUpdatedRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBFilterDateRangeUpdatedRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (!request.IsComparison)
                    return Failure("IsComparison must be true. Use GetAnalyticsDBFilterDateRange when comparison is off.");

                if (request.FilterID < 0)
                    return Failure("FilterID from the date-range SP is required.");

                if (request.ComparisonID <= 0)
                    return Failure("ComparisonID is required.");

                var customCompStart = ParseDateOrNull(request.CustomCompStartDate);
                var customCompEnd = ParseDateOrNull(request.CustomCompEndDate);
                if (customCompStart == null || customCompEnd == null)
                    return Failure("CustomCompStartDate and CustomCompEndDate are required (CurrentStartDate / CurrentEndDate from the date-range SP).");
                if (customCompStart > customCompEnd)
                    return Failure("CustomCompStartDate cannot be after CustomCompEndDate.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@FilterID", request.FilterID),
                    new SqlParameter("@IsComparison", request.IsComparison),
                    new SqlParameter("@ComparisonID", request.ComparisonID),
                    new SqlParameter("@CustomCompStartDate", customCompStart),
                    new SqlParameter("@CustomCompEndDate", customCompEnd),
                    new SqlParameter("@DashboardID", request.DashboardID)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBFilterDateRangeModel>(
                    "usp_Whizible2_Sel_AnalyticsDBFilterDateRange_Updated", sqlParams);

                return Success("Analytics DB filter date range comparison", ExtractList<AnalyticsDBFilterDateRangeModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBFilterDateRangeUpdated

        // Added by Dipali V. on 10-09-2026 - One endpoint, Flag-wise KPI SPs, shared filters.
        // Added by Vikas T on 16-09-2026 - Utilization / Bench / ActiveProjects cards load at DrillLevel 0
        // with L0 columns mapped (UtilizationPct, BenchFTE, ActiveProjectsCount → CurrentValue).
        public async Task<ResponseEntity> GetAnalyticsDBKPICards(AnalyticsDBKPICardsRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBKPICardsRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                // Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
                // var previousFrom = ParseDateOrNull(request.PreviousFromDate) ?? currentFrom.Value;
                // var previousTo = ParseDateOrNull(request.PreviousToDate) ?? currentTo.Value;
                // if (previousFrom > previousTo)
                //     return Failure("PreviousFromDate cannot be after PreviousToDate.");
                var previousFrom = ParseDateOrNull(request.PreviousFromDate);
                var previousTo = ParseDateOrNull(request.PreviousToDate);
                if (!previousFrom.HasValue || !previousTo.HasValue)
                {
                    previousFrom = null;
                    previousTo = null;
                }
                else if (previousFrom > previousTo)
                    return Failure("PreviousFromDate cannot be after PreviousToDate.");
                // End of Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off

                var flags = ResolveKpiFlagList(request.Flag);
                if (flags.Count == 0)
                    return Failure("Flag must be a known KPI card (or omit Flag to load all).");

                var cards = new List<AnalyticsDBKPICardModel>();
                foreach (var flag in flags)
                {
                    cards.Add(await LoadKpiCard(flag, request, currentFrom.Value, currentTo.Value, previousFrom, previousTo));
                }

                return Success("Analytics DB KPI cards", cards);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBKPICards

        // Added by Dipali V. on 15-09-2026 - Greeting project count (Portfolio + Customer combo)
        // SP: usp_Whizible2_Sel_AnalyticsDB_ConsolidatedProjectCount
        public async Task<ResponseEntity> GetAnalyticsDBConsolidatedProjectCount(
            AnalyticsDBConsolidatedProjectCountRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBConsolidatedProjectCountRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime)
                    {
                        Value = currentFrom.HasValue ? currentFrom.Value : (object)DBNull.Value
                    },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime)
                    {
                        Value = currentTo.HasValue ? currentTo.Value : (object)DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    }
                };

                var rows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_ConsolidatedProjectCount", sqlParams);
                var row0 = rows.Count > 0 ? rows[0] : null;

                static int ParseCount(Dictionary<string, string>? row, string key)
                {
                    if (row == null || !row.TryGetValue(key, out var raw)) return 0;
                    _ = int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var n);
                    return Math.Max(0, n);
                }

                return Success("Consolidated project count", new AnalyticsDBConsolidatedProjectCountModel
                {
                    ProjectCount = ParseCount(row0, "ProjectCount"),
                    PortfolioCount = ParseCount(row0, "PortfolioCount"),
                    CustomerCount = ParseCount(row0, "CustomerCount")
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBConsolidatedProjectCount

        // Added by Dipali V. on 11-09-2026 - Revenue KPI drill-through
        // SP: usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown
        // Levels in SP: 1 Portfolios, 2 Projects, 4 Invoice lines (+ filters)
        public async Task<ResponseEntity> GetAnalyticsDBRevenueDrillDown(AnalyticsDBRevenueDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBRevenueDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var level = request.Level < 1 ? 1 : request.Level;
                if (level > 6) return Failure("Level must be between 1 and 6.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                // L2 needs PortfolioID (0 = unmapped / NA project group — allowed).
                if (level == 2 && !request.PortfolioID.HasValue)
                    return Failure("PortfolioID is required for Level 2.");
                if (level == 3 && (request.ProjectID == null || request.ProjectID <= 0))
                    return Failure("ProjectID is required for Level 3.");
                if (level >= 4 && string.IsNullOrWhiteSpace(request.InvoiceID))
                    return Failure("InvoiceID is required for Level 4+.");
                if (level >= 5 && string.IsNullOrWhiteSpace(request.InvoiceItemIDs))
                    return Failure("InvoiceItemIDs is required for Level 5+ (ProjectTimesheetID / ExpensesEntryID / DeliverableID / MilestoneID / EmployeeID from the selected line).");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@Level", SqlDbType.Int) { Value = level },
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PortfolioID", SqlDbType.Int)
                    {
                        /* 0 = Unmapped / NA — pass through to SP (do not convert to NULL). */
                        Value = request.PortfolioID.HasValue
                            ? request.PortfolioID.Value
                            : (object)DBNull.Value
                    },
                    new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID.HasValue && request.ProjectID.Value > 0
                            ? request.ProjectID.Value
                            : (object)DBNull.Value
                    },
                    new SqlParameter("@InvoiceID", SqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(request.InvoiceID)
                            ? (object)DBNull.Value
                            : request.InvoiceID.Trim()
                    },
                    new SqlParameter("@InvoiceItemIDs", SqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(request.InvoiceItemIDs)
                            ? (object)DBNull.Value
                            : request.InvoiceItemIDs.Trim()
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    }
                };

                var rows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_RevenueDrillDown", sqlParams);

                return Success("Revenue drill-down", new AnalyticsDBRevenueDrillDownModel
                {
                    Level = level,
                    LevelLabel = RevenueDrillLevelLabel(level),
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBRevenueDrillDown

        // Added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through
        public async Task<ResponseEntity> GetAnalyticsDBDelayedProjectsDrillDown(
            AnalyticsDBDelayedProjectsDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBDelayedProjectsDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var level = request.Level < 1 ? 1 : request.Level;
                if (level > 4) return Failure("Level must be between 1 and 4.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                if (level >= 2 && (request.ProjectID == null || request.ProjectID <= 0))
                    return Failure("ProjectID is required for Level 2+.");
                if (level == 3 && (request.ItemID == null || request.ItemID <= 0))
                    return Failure("ItemID is required for Level 3.");
                if (level == 4 && (request.TaskID == null || request.TaskID <= 0))
                    return Failure("TaskID is required for Level 4.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var prevFrom = ParseDateOrNull(request.PreviousFromDate);
                var prevTo = ParseDateOrNull(request.PreviousToDate);
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PreviousFromDate", SqlDbType.DateTime)
                    {
                        Value = (object?)prevFrom ?? DBNull.Value
                    },
                    new SqlParameter("@PreviousToDate", SqlDbType.DateTime)
                    {
                        Value = (object?)prevTo ?? DBNull.Value
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    },
                    new SqlParameter("@EmployeeID", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.EmployeeID) ?? DBNull.Value
                    }
                };

                string spName;
                if (level == 1)
                {
                    spName = "usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjectsList";
                }
                else if (level == 2)
                {
                    spName = "usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjectsMilestones";
                    sqlParams.Insert(0, new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID!.Value
                    });
                }
                else if (level == 3)
                {
                    spName = "usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjectsTasks";
                    sqlParams.Insert(0, new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID!.Value
                    });
                    sqlParams.Insert(1, new SqlParameter("@ItemID", SqlDbType.Int)
                    {
                        Value = request.ItemID!.Value
                    });
                    sqlParams.Insert(2, new SqlParameter("@ItemType", SqlDbType.VarChar, 20)
                    {
                        Value = string.IsNullOrWhiteSpace(request.ItemType)
                            ? "Milestone"
                            : request.ItemType.Trim()
                    });
                }
                else
                {
                    spName = "usp_Whizible2_Sel_AnalyticsDBKPI_DelayedProjectsActivity";
                    sqlParams.Insert(0, new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID!.Value
                    });
                    sqlParams.Insert(1, new SqlParameter("@TaskID", SqlDbType.Int)
                    {
                        Value = request.TaskID!.Value
                    });
                }

                var rows = await ReadSpRowsAsStringDictAsync(spName, sqlParams);

                return Success("Delayed projects drill-down", new AnalyticsDBRevenueDrillDownModel
                {
                    Level = level,
                    LevelLabel = DelayedProjectsDrillLevelLabel(level),
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }

        private static string DelayedProjectsDrillLevelLabel(int level)
        {
            return level switch
            {
                1 => "Delayed projects",
                2 => "Milestones",
                3 => "Tasks",
                4 => "Activity",
                _ => "Delayed projects"
            };
        }
        // End of added By Vyankat B. on the 16th sep 2026 for the Delayed Projects KPI drill-through

        // Added By Vyankat B. on 21th Sep 2026 for the Budget Variance KPI drill-through
        public async Task<ResponseEntity> GetAnalyticsDBBudgetVarianceDrillDown(
            AnalyticsDBDelayedProjectsDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBDelayedProjectsDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var level = request.Level < 1 ? 1 : request.Level;
                if (level > 4) return Failure("Level must be between 1 and 4.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                if (level >= 2 && (request.ProjectID == null || request.ProjectID <= 0))
                    return Failure("ProjectID is required for Level 2+.");
                if (level == 3 && (request.ItemID == null || request.ItemID <= 0))
                    return Failure("ItemID is required for Level 3.");
                if (level == 4 && (request.TaskID == null || request.TaskID <= 0))
                    return Failure("TaskID is required for Level 4.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var prevFrom = ParseDateOrNull(request.PreviousFromDate);
                var prevTo = ParseDateOrNull(request.PreviousToDate);
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PreviousFromDate", SqlDbType.DateTime)
                    {
                        Value = (object?)prevFrom ?? DBNull.Value
                    },
                    new SqlParameter("@PreviousToDate", SqlDbType.DateTime)
                    {
                        Value = (object?)prevTo ?? DBNull.Value
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    },
                    new SqlParameter("@EmployeeID", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.EmployeeID) ?? DBNull.Value
                    }
                };

                string spName = "usp_Whizible2_Sel_AnalyticsDBKPI_BudgetVarianceList";
                if (level >= 2)
                {
                    sqlParams.Add(new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID!.Value
                    });
                }
                sqlParams.Add(new SqlParameter("@Level", SqlDbType.Int) { Value = level });
                sqlParams.Add(new SqlParameter("@ItemID", SqlDbType.Int)
                {
                    Value = request.ItemID is > 0 ? request.ItemID.Value : DBNull.Value
                });
                sqlParams.Add(new SqlParameter("@ItemType", SqlDbType.VarChar, 20)
                {
                    Value = string.IsNullOrWhiteSpace(request.ItemType)
                        ? "Milestone"
                        : request.ItemType.Trim()
                });
                sqlParams.Add(new SqlParameter("@TaskID", SqlDbType.Int)
                {
                    Value = request.TaskID is > 0 ? request.TaskID.Value : DBNull.Value
                });

                var rows = await ReadSpRowsAsStringDictAsync(spName, sqlParams);

                return Success("Budget variance drill-down", new AnalyticsDBRevenueDrillDownModel
                {
                    Level = level,
                    LevelLabel = level switch
                    {
                        1 => "Projects",
                        2 => "Milestones",
                        3 => "Work items",
                        4 => "Activity log",
                        _ => "Projects"
                    },
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Added By Vyankat B. on 21th Sep 2026 for the Budget Variance KPI drill-through

        // Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through
        public async Task<ResponseEntity> GetAnalyticsDBCSATDrillDown(
            AnalyticsDBCSATDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBCSATDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var level = request.Level < 1 ? 1 : request.Level;
                if (level > 2) return Failure("Level must be 1 or 2.");

                // Commented and Added By Vyankat b. on 28th Sep 2026 for Helpdesk SLA Status drill
                if (level == 2)
                {
                    if (request.QueryID == null || request.QueryID <= 0)
                        return Failure("QueryID is required for SLA Details.");

                    var slaParams = new List<SqlParameter>
                    {
                        new SqlParameter("@intParamQueryID", SqlDbType.Int)
                        {
                            Value = request.QueryID.Value
                        }
                    };
                    var slaRows = await ReadSpRowsAsStringDictAsync(
                        "usp_PM_CalculateSLAForHelpDesk_Query_Details", slaParams);

                    return Success("Customer CSAT drill-down", new AnalyticsDBRevenueDrillDownModel
                    {
                        Level = 2,
                        LevelLabel = "SLA Status",
                        Rows = slaRows
                    });
                }
                // End of Commented and Added By Vyankat b. on 28th Sep 2026 for Helpdesk SLA Status drill

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                var prevFrom = ParseDateOrNull(request.PreviousFromDate);
                var prevTo = ParseDateOrNull(request.PreviousToDate);
                var kpiFilter = new AnalyticsDBKPICardsRequest
                {
                    DashboardID = request.DashboardID,
                    CustomerIDs = request.CustomerIDs,
                    ProjectManagerIDs = request.ProjectManagerIDs,
                    PortfolioIDs = request.PortfolioIDs,
                    RegionIDs = request.RegionIDs,
                    BillingTypeIDs = request.BillingTypeIDs,
                    HealthIDs = request.HealthIDs
                };
                var sqlParams = BuildKpiFilterParameters(
                    kpiFilter, currentFrom.Value, currentTo.Value, prevFrom, prevTo);

                var rows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDBKPI_CSATList", sqlParams);

                return Success("Customer CSAT drill-down", new AnalyticsDBRevenueDrillDownModel
                {
                    Level = 1,
                    LevelLabel = "Tickets",
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the Customer CSAT KPI drill-through

        // Added by Aditya J. on 15-09-2026 - Portfolio-wise current revenue (mix chart)
        // SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix
        public async Task<ResponseEntity> GetAnalyticsDBPortfolioRevenueMix(AnalyticsDBPortfolioRevenueMixRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBPortfolioRevenueMixRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                var previousFrom = ParseDateOrNull(request.PreviousFromDate) ?? currentFrom.Value;
                var previousTo = ParseDateOrNull(request.PreviousToDate) ?? currentTo.Value;
                if (previousFrom > previousTo)
                    return Failure("PreviousFromDate cannot be after PreviousToDate.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var kpiFilter = new AnalyticsDBKPICardsRequest
                {
                    DashboardID = request.DashboardID,
                    CurrentFromDate = request.CurrentFromDate,
                    CurrentToDate = request.CurrentToDate,
                    PreviousFromDate = request.PreviousFromDate,
                    PreviousToDate = request.PreviousToDate,
                    PortfolioIDs = request.PortfolioIDs,
                    CustomerIDs = request.CustomerIDs,
                    ProjectManagerIDs = request.ProjectManagerIDs,
                    RegionIDs = request.RegionIDs,
                    BillingTypeIDs = request.BillingTypeIDs,
                    HealthIDs = request.HealthIDs
                };

                var sqlParams = BuildKpiFilterParameters(
                    kpiFilter, currentFrom.Value, currentTo.Value, previousFrom, previousTo);

                var rawRows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix", sqlParams);

                var items = new List<AnalyticsDBPortfolioRevenueMixItemModel>();
                foreach (var row in rawRows)
                {
                    items.Add(new AnalyticsDBPortfolioRevenueMixItemModel
                    {
                        PortfolioID = (int)ParseSpDecimal(GetRowValue(row, "PortfolioID")),
                        PortfolioName = NullIfBlank(GetRowValue(row, "PortfolioName")) ?? string.Empty,
                        Revenue = ParseSpDecimal(GetRowValue(row, "Revenue")),
                        RevenueFormatted = NullIfBlank(GetRowValue(row, "RevenueFormatted")),
                        BaseCurrencyCode = NullIfBlank(GetRowValue(row, "BaseCurrencyCode")) ?? "",
                        //Added by Aditya J. on 28-09-2026 Pass the mix slice project count through to the chart
                        ProjectCount = (int)ParseSpDecimal(GetRowValue(row, "ProjectCount"))
                        //End of Added by Aditya J. on 28-09-2026 Pass the mix slice project count through to the chart
                    });
                }

                return Success("Portfolio revenue mix", items);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBPortfolioRevenueMix

        // Added by Vikas T on 21-09-2026 - Revenue vs Cost chart drill-through
        // SP: usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown
        // Same Level + date + chip filters as RevenueDrillDown
        public async Task<ResponseEntity> GetAnalyticsDBPortfolioRevenueMixDrillDown(
            AnalyticsDBPortfolioRevenueMixDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBPortfolioRevenueMixDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var level = request.Level < 1 ? 1 : request.Level;
                //Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines
                if (level > 4) return Failure("Level must be between 1 and 4.");
                //End of Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                // Added by Aditya J. on 15-09-2026 - Mix SP: @PortfolioID = 0 is Unmapped.
                if (level >= 2 && (request.PortfolioID == null || request.PortfolioID < 0))
                    return Failure("PortfolioID is required for Level 2+ (use 0 for Unmapped).");
                if (level == 3 && (request.ProjectID == null || request.ProjectID <= 0))
                    return Failure("ProjectID is required for Level 3.");
                if (level >= 4 && string.IsNullOrWhiteSpace(request.InvoiceID))
                    return Failure("InvoiceID is required for Level 4.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@Level", SqlDbType.Int) { Value = level },
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PortfolioID", SqlDbType.Int)
                    {
                        Value = request.PortfolioID.HasValue
                            ? request.PortfolioID.Value
                            : (object)DBNull.Value
                    },
                    new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID.HasValue && request.ProjectID.Value > 0
                            ? request.ProjectID.Value
                            : (object)DBNull.Value
                    },
                    new SqlParameter("@InvoiceID", SqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(request.InvoiceID)
                            ? (object)DBNull.Value
                            : request.InvoiceID.Trim()
                    },
                    new SqlParameter("@InvoiceItemIDs", SqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(request.InvoiceItemIDs)
                            ? (object)DBNull.Value
                            : request.InvoiceItemIDs.Trim()
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    }
                };

                // Added by Aditya J. on 15-09-2026 - mix slice drill always uses the mix SP
                // (PortfolioID 0 = Unmapped). Do not fall back to the revenue drill SP.
                var rows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown", sqlParams);

                return Success("Portfolio revenue mix drill-down", new AnalyticsDBRevenueDrillDownModel
                {
                    Level = level,
                    LevelLabel = RevenueDrillLevelLabel(level),
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBPortfolioRevenueMixDrillDown

        //Added by Aditya J. on 21-09-2026 Cost & budget variance by project
        // SP: usp_Whizible2_Sel_AnalyticsDB_CostBudgetVarianceByProject
        public async Task<ResponseEntity> GetAnalyticsDBCostBudgetVarianceByProject(
            AnalyticsDBCostBudgetVarianceByProjectRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBCostBudgetVarianceByProjectRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    }
                };

                var rawRows = await ReadSpRowsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_CostBudgetVarianceByProject", sqlParams);

                var items = new List<AnalyticsDBCostBudgetVarianceByProjectItemModel>();
                foreach (var row in rawRows)
                {
                    items.Add(new AnalyticsDBCostBudgetVarianceByProjectItemModel
                    {
                        ProjectID = (int)ParseSpDecimal(GetRowValue(row, "ProjectID")),
                        ProjectName = NullIfBlank(GetRowValue(row, "ProjectName")) ?? string.Empty,
                        PlannedBudget = ParseSpDecimal(GetRowValue(row, "PlannedBudget")),
                        ActualCost = ParseSpDecimal(GetRowValue(row, "ActualCost")),
                        VarianceAmount = ParseSpDecimal(GetRowValue(row, "VarianceAmount")),
                        BudgetBurnPercentage = ParseSpDecimal(GetRowValue(row, "BudgetBurnPercentage")),
                        BudgetVariancePercentage = ParseSpDecimal(GetRowValue(row, "BudgetVariancePercentage")),
                        IsOverBudget = ParseSpBool(GetRowValue(row, "IsOverBudget"))
                    });
                }

                return Success("Cost and budget variance by project", items);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        //End of Added by Aditya J. on 21-09-2026 Cost & budget variance by project

        // Added by Aditya J. on 17-09-2026 - Portfolio Revenue Mix Excel export.
        // Excel-compatible .xls HTML avoids any new package/reference requirement.
        public async Task<byte[]> ExportAnalyticsDBPortfolioRevenueMixExcel(AnalyticsDBPortfolioRevenueMixRequest request)
        {
            if (request == null) request = new AnalyticsDBPortfolioRevenueMixRequest();
            if (!IsValidDashboardId(request.DashboardID)) throw new ArgumentException("DashboardID is required.");
            var currentFrom = ParseDateOrNull(request.CurrentFromDate);
            var currentTo = ParseDateOrNull(request.CurrentToDate);
            if (currentFrom == null || currentTo == null) throw new ArgumentException("CurrentFromDate and CurrentToDate are required.");
            if (currentFrom > currentTo) throw new ArgumentException("CurrentFromDate cannot be after CurrentToDate.");
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new InvalidOperationException("Database connection is not configured.");
            var previousFrom = ParseDateOrNull(request.PreviousFromDate) ?? currentFrom.Value;
            var previousTo = ParseDateOrNull(request.PreviousToDate) ?? currentTo.Value;
            var kpiFilter = new AnalyticsDBKPICardsRequest
            {
                DashboardID = request.DashboardID,
                CurrentFromDate = request.CurrentFromDate,
                CurrentToDate = request.CurrentToDate,
                PreviousFromDate = request.PreviousFromDate,
                PreviousToDate = request.PreviousToDate,
                PortfolioIDs = request.PortfolioIDs,
                CustomerIDs = request.CustomerIDs,
                ProjectManagerIDs = request.ProjectManagerIDs,
                RegionIDs = request.RegionIDs,
                BillingTypeIDs = request.BillingTypeIDs,
                HealthIDs = request.HealthIDs
            };
            var sqlParams = BuildKpiFilterParameters(kpiFilter, currentFrom.Value, currentTo.Value, previousFrom, previousTo);
            var rows = await ReadSpRowsAsStringDictAsync("usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMix", sqlParams);

            // Do not generate an Excel file when the selected period/filter combination
            // has no revenue records. The UI displays the same message through Alertify.
            decimal totalRevenue = 0m;
            foreach (var row in rows)
            {
                totalRevenue += ParseSpDecimal(GetRowValue(row, "Revenue"));
            }

            if (totalRevenue <= 0m)
                throw new InvalidOperationException("There are no records to show.");

            // Export only user-facing columns; internal ID columns are not shown in Excel.
            var mixColumns = new[] { "PortfolioName", "Revenue", "RevenueFormatted", "BaseCurrencyCode" };
            return BuildExcelHtmlFile("Portfolio Revenue Mix", mixColumns, rows);
        }

        // Added by Aditya J. on 17-09-2026 - Portfolio Revenue Mix drill-down Excel export.
        public async Task<byte[]> ExportAnalyticsDBPortfolioRevenueMixDrillDownExcel(AnalyticsDBPortfolioRevenueMixDrillDownRequest request)
        {
            if (request == null) request = new AnalyticsDBPortfolioRevenueMixDrillDownRequest();
            if (!IsValidDashboardId(request.DashboardID)) throw new ArgumentException("DashboardID is required.");
            var level = request.Level < 1 ? 1 : request.Level;
            //Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines
            if (level > 4) throw new ArgumentException("Level must be between 1 and 4.");
            //End of Added by Aditya J. on 29-09-2026 Portfolio revenue mix drill-down stops at invoice lines
            var currentFrom = ParseDateOrNull(request.CurrentFromDate);
            var currentTo = ParseDateOrNull(request.CurrentToDate);
            if (currentFrom == null || currentTo == null) throw new ArgumentException("CurrentFromDate and CurrentToDate are required.");
            if (currentFrom > currentTo) throw new ArgumentException("CurrentFromDate cannot be after CurrentToDate.");
            if (level >= 2 && (request.PortfolioID == null || request.PortfolioID < 0)) throw new ArgumentException("PortfolioID is required for Level 2+ (use 0 for Unmapped).");
            if (level == 3 && (request.ProjectID == null || request.ProjectID <= 0)) throw new ArgumentException("ProjectID is required for Level 3.");
            if (level >= 4 && string.IsNullOrWhiteSpace(request.InvoiceID)) throw new ArgumentException("InvoiceID is required for Level 4.");
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new InvalidOperationException("Database connection is not configured.");
            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@Level", SqlDbType.Int) { Value = level },
                new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                new SqlParameter("@PortfolioID", SqlDbType.Int) { Value = request.PortfolioID.HasValue ? request.PortfolioID.Value : (object)DBNull.Value },
                new SqlParameter("@ProjectID", SqlDbType.Int) { Value = request.ProjectID.HasValue && request.ProjectID.Value > 0 ? request.ProjectID.Value : (object)DBNull.Value },
                new SqlParameter("@InvoiceID", SqlDbType.VarChar, 50) { Value = string.IsNullOrWhiteSpace(request.InvoiceID) ? (object)DBNull.Value : request.InvoiceID.Trim() },
                new SqlParameter("@InvoiceItemIDs", SqlDbType.VarChar, 50) { Value = (object?)NullIfBlank(request.InvoiceItemIDs) ?? DBNull.Value },
                new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value },
                new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value },
                new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value },
                new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value },
                new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value },
                new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1) { Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value },
                new SqlParameter("@DashboardID", SqlDbType.Int) { Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value }
            };
            var rows = await ReadSpRowsAsStringDictAsync("usp_Whizible2_Sel_AnalyticsDB_PortfolioRevenueMixDrillDown", sqlParams);

            // Excel should contain only business/display columns.
            // Remove all internal ID/IDs columns from every drill-down level.
            var columns = rows.Count > 0
                ? rows[0].Keys
                    .Where(c => !IsExcelIdColumn(c))
                    .ToArray()
                : new[] { "Message" };

            // Level 5 contains additional technical/source fields which are required
            // by the drill-down logic but should not be exposed in the Excel report.
            if (level == 5)
            {
                columns = columns
                    .Where(c => !Level5ExcelExcludedColumns.Contains(c))
                    .ToArray();
            }

            return BuildExcelHtmlFile("Portfolio Revenue Drill Down - Level " + level, columns, rows);
        }

        private static readonly HashSet<string> Level5ExcelExcludedColumns =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MileStone",
                "BillAmount",
                "ActualCompletionDate",
                "CompletionPercentage",
                "RevenueStatusChangeDate",
                "Title",
                "ScheduleID",
                "BillableAmount",
                "PercentageComplete",
                "ProjectID",
                "ProjectName",
                "PortfolioID"
            };

        private static bool IsExcelIdColumn(string? columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return false;

            var name = columnName.Trim();

            // Covers ProjectID, PortfolioID, InvoiceID, EmployeeID, ScheduleID,
            // CustomerIDs, PortfolioIDs, etc. without changing the SP/UI response.
            return name.EndsWith("ID", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("IDs", StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] BuildExcelHtmlFile(string title, string[] columns, List<Dictionary<string, string?>> rows)
        {
            var sb = new StringBuilder();
            sb.Append("\uFEFF<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"><head>");
            sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><title>");
            sb.Append(System.Net.WebUtility.HtmlEncode(title)).Append("</title></head><body><h3>");
            sb.Append(System.Net.WebUtility.HtmlEncode(title)).Append("</h3><table border=\"1\"><thead><tr>");
            foreach (var column in columns) sb.Append("<th>").Append(System.Net.WebUtility.HtmlEncode(column)).Append("</th>");
            sb.Append("</tr></thead><tbody>");
            if (rows.Count == 0) sb.Append("<tr><td colspan=\"").Append(Math.Max(columns.Length, 1)).Append("\">No rows for this level in the selected period.</td></tr>");
            else foreach (var row in rows) { sb.Append("<tr>"); foreach (var column in columns) sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(GetRowValue(row, column) ?? string.Empty)).Append("</td>"); sb.Append("</tr>"); }
            sb.Append("</tbody></table></body></html>");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string? GetRowValue(Dictionary<string, string?> row, string column)
        {
            if (row == null || string.IsNullOrWhiteSpace(column)) return null;
            foreach (var kv in row)
            {
                if (string.Equals(kv.Key, column, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
            return null;
        }

        // Added by Vikas T on 17-09-2026 - Revenue vs Cost trend & forecast chart
        // Updated by Vikas T on 23-09-2026 - months follow CurrentFrom–CurrentTo (not a fixed 6+2 spine)
        // SP: usp_Whizible2_Sel_AnalyticsDB_RevenueCostTrend (self-contained; no nested SP EXEC)
        // Result set 1 = chart meta; result set 2 = months in CurrentFrom–CurrentTo
        // Forecast: trend-based from chart actuals with revenue (not AI/ML, not 1.03/1.07)
        public async Task<ResponseEntity> GetAnalyticsDBRevenueCostTrend(AnalyticsDBRevenueCostTrendRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBRevenueCostTrendRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                var previousFrom = ParseDateOrNull(request.PreviousFromDate);
                var previousTo = ParseDateOrNull(request.PreviousToDate);

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PreviousFromDate", SqlDbType.DateTime)
                    {
                        Value = previousFrom.HasValue ? previousFrom.Value : (object)DBNull.Value
                    },
                    new SqlParameter("@PreviousToDate", SqlDbType.DateTime)
                    {
                        Value = previousTo.HasValue ? previousTo.Value : (object)DBNull.Value
                    },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    }
                };

                var sets = await ReadSpMultiResultSetsAsStringDictAsync(
                    "usp_Whizible2_Sel_AnalyticsDB_RevenueCostTrend", sqlParams);

                var meta = sets.Count > 0 && sets[0].Count > 0
                    ? sets[0][0]
                    : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                var months = sets.Count > 1
                    ? sets[1]
                    : new List<Dictionary<string, string?>>();

                return Success("Revenue vs Cost trend", new AnalyticsDBRevenueCostTrendModel
                {
                    Meta = meta,
                    Months = months
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBRevenueCostTrend

        private static string RevenueDrillLevelLabel(int level)
        {
            return level switch
            {
                1 => "Portfolios",
                2 => "Projects",
                3 => "Invoices",
                4 => "Invoice lines",
                5 => "Line detail",
                6 => "Audit trail",
                _ => "Portfolios"
            };
        }

        // Added by Vikas T on 16-09-2026 - Utilization / Bench / Active Projects drill-through
        // SPs: usp_Whizible2_Sel_AnalyticsDBKPI_{Flag} at @DrillLevel 1–4
        public async Task<ResponseEntity> GetAnalyticsDBKPIDrillDown(AnalyticsDBKPIDrillDownRequest request)
        {
            try
            {
                if (request == null) request = new AnalyticsDBKPIDrillDownRequest();
                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                var flag = NormalizeFlag(request.Flag ?? "");
                if (!KpiFlagsRequiringDrillLevelZero.Contains(flag))
                    return Failure("Flag must be Utilization, Bench, or ActiveProjects.");

                var meta = FindKpiFlag(flag);
                if (meta == null || string.IsNullOrWhiteSpace(meta.SpName))
                    return Failure("Unknown KPI Flag.");

                var level = request.Level < 1 ? 1 : request.Level;
                var maxLevel = 4;
                if (level > maxLevel)
                    return Failure($"Level must be between 1 and {maxLevel} for {flag}.");

                var currentFrom = ParseDateOrNull(request.CurrentFromDate);
                var currentTo = ParseDateOrNull(request.CurrentToDate);
                if (currentFrom == null || currentTo == null)
                    return Failure("CurrentFromDate and CurrentToDate are required.");
                if (currentFrom > currentTo)
                    return Failure("CurrentFromDate cannot be after CurrentToDate.");

                var previousFrom = ParseDateOrNull(request.PreviousFromDate) ?? currentFrom.Value;
                var previousTo = ParseDateOrNull(request.PreviousToDate) ?? currentTo.Value;

                // Added by Vikas T on 16-09-2026 - required keys per SP drill level
                if (string.Equals(flag, "Utilization", StringComparison.OrdinalIgnoreCase))
                {
                    /* PortfolioID 0 = Unmapped / NA project group — allowed (same as Bench). */
                    if (level >= 2 && request.PortfolioID == null)
                        return Failure("PortfolioID (ProjectGroupID) is required for Utilization Level 2+.");
                    if (level >= 3 && (request.ProjectID == null || request.ProjectID <= 0))
                        return Failure("ProjectID is required for Utilization Level 3+.");
                    if (level >= 4 && (request.EmployeeID == null || request.EmployeeID <= 0))
                        return Failure("EmployeeID is required for Utilization Level 4.");
                }
                else if (string.Equals(flag, "Bench", StringComparison.OrdinalIgnoreCase))
                {
                    // Added by Vikas T on 23-09-2026 - Bench drill: Groups → Projects → People → Weeks
                    // PortfolioID 0 = Unallocated; ProjectID 0 = Unallocated project
                    // RoleID kept as fallback for the old published API (maps to ProjectID in SP)
                    if (level >= 2 && request.PortfolioID == null)
                        return Failure("PortfolioID (ProjectGroupID) is required for Bench Level 2+.");
                    if (level >= 3 && request.ProjectID == null && request.RoleID == null)
                        return Failure("ProjectID is required for Bench Level 3+.");
                    if (level >= 4 && (request.EmployeeID == null || request.EmployeeID <= 0))
                        return Failure("EmployeeID is required for Bench Level 4.");
                }
                else if (string.Equals(flag, "ActiveProjects", StringComparison.OrdinalIgnoreCase))
                {
                    if (level >= 2 && (request.ProjectID == null || request.ProjectID <= 0))
                        return Failure("ProjectID is required for Active Projects Level 2+.");
                    // MilestoneID 0 = project tasks with no milestone
                    if (level >= 3 && request.MilestoneID == null)
                        return Failure("MilestoneID is required for Active Projects Level 3+.");
                    if (level >= 4 && (request.TaskID == null || request.TaskID <= 0))
                        return Failure("TaskID is required for Active Projects Level 4.");
                }

                if (string.IsNullOrWhiteSpace(_connectionString))
                    return Failure("Database connection is not configured.");

                var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
                var pageSize = request.PageSize < 1 ? 500 : Math.Min(request.PageSize, 2000);

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DrillLevel", SqlDbType.TinyInt) { Value = (byte)level },
                    new SqlParameter("@CurrentFromDate", SqlDbType.DateTime) { Value = currentFrom.Value },
                    new SqlParameter("@CurrentToDate", SqlDbType.DateTime) { Value = currentTo.Value },
                    new SqlParameter("@PreviousFromDate", SqlDbType.DateTime) { Value = previousFrom },
                    new SqlParameter("@PreviousToDate", SqlDbType.DateTime) { Value = previousTo },
                    new SqlParameter("@CustomerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@ProjectManagerIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@PortfolioIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value
                    },
                   new SqlParameter("@RegionIDs", SqlDbType.VarChar, -1)
                    {
                        /* Added by Vikas T on 29-09-2026 - Organization Unit filter is Bench and Utilization drill. */
                        Value = string.Equals(flag, "Bench", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(flag, "Utilization", StringComparison.OrdinalIgnoreCase)
                            ? (object?)NullIfBlank(request.RegionIDs) ?? DBNull.Value
                            : DBNull.Value
                    },
                    new SqlParameter("@BillingTypeIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@HealthIDs", SqlDbType.VarChar, -1)
                    {
                        Value = (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value
                    },
                    new SqlParameter("@DashboardID", SqlDbType.Int)
                    {
                        Value = request.DashboardID > 0 ? request.DashboardID : (object)DBNull.Value
                    },
                    new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
                    new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize }
                };

                if (string.Equals(flag, "Utilization", StringComparison.OrdinalIgnoreCase))
                {
                    sqlParams.Add(new SqlParameter("@ProjectGroupID", SqlDbType.Int)
                    {
                        /* 0 = Unmapped / NA — pass through (do not convert to NULL). */
                        Value = request.PortfolioID.HasValue
                            ? request.PortfolioID.Value
                            : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID.HasValue && request.ProjectID.Value > 0
                            ? request.ProjectID.Value
                            : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@EmployeeID", SqlDbType.Int)
                    {
                        Value = request.EmployeeID.HasValue && request.EmployeeID.Value > 0
                            ? request.EmployeeID.Value
                            : (object)DBNull.Value
                    });
                }
                else if (string.Equals(flag, "Bench", StringComparison.OrdinalIgnoreCase))
                {
                    // Added by Vikas T on 23-09-2026 - PortfolioID→@ProjectGroupID (0=Unallocated OK)
                    // Send @ProjectID (0 OK). RoleID used only when ProjectID is missing (old aspx shim).
                    sqlParams.Add(new SqlParameter("@ProjectGroupID", SqlDbType.Int)
                    {
                        Value = request.PortfolioID.HasValue
                            ? request.PortfolioID.Value
                            : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID.HasValue
                            ? request.ProjectID.Value
                            : request.RoleID.HasValue
                                ? request.RoleID.Value
                                : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@EmployeeID", SqlDbType.Int)
                    {
                        Value = request.EmployeeID.HasValue && request.EmployeeID.Value > 0
                            ? request.EmployeeID.Value
                            : (object)DBNull.Value
                    });
                }
                else
                {
                    sqlParams.Add(new SqlParameter("@ProjectID", SqlDbType.Int)
                    {
                        Value = request.ProjectID.HasValue && request.ProjectID.Value > 0
                            ? request.ProjectID.Value
                            : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@MilestoneID", SqlDbType.Int)
                    {
                        Value = request.MilestoneID.HasValue
                            ? request.MilestoneID.Value
                            : (object)DBNull.Value
                    });
                    sqlParams.Add(new SqlParameter("@TaskID", SqlDbType.Int)
                    {
                        Value = request.TaskID.HasValue && request.TaskID.Value > 0
                            ? request.TaskID.Value
                            : (object)DBNull.Value
                    });
                }

                var rows = await ReadSpRowsAsStringDictAsync(meta.SpName, sqlParams);

                return Success($"{meta.Label} drill-down", new AnalyticsDBKPIDrillDownModel
                {
                    Flag = meta.Flag,
                    Level = level,
                    LevelLabel = OpsKpiDrillLevelLabel(flag, level),
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetAnalyticsDBKPIDrillDown

        // Added by Vikas T on 16-09-2026 - breadcrumb labels for Util / Bench / Active drills
        // Updated by Vikas T on 23-09-2026 - Util/Bench: Groups → Projects → People → Timesheet entries
        private static string OpsKpiDrillLevelLabel(string flag, int level)
        {
            if (string.Equals(flag, "Utilization", StringComparison.OrdinalIgnoreCase))
            {
                return level switch
                {
                    1 => "Portfolios",
                    2 => "Projects",
                    3 => "People",
                    4 => "Timesheet entries",
                    _ => "Portfolios"
                };
            }
            if (string.Equals(flag, "Bench", StringComparison.OrdinalIgnoreCase))
            {
                return level switch
                {
                    1 => "Portfolios",
                    2 => "Projects",
                    3 => "People",
                    4 => "Timesheet entries",
                    _ => "Portfolios"
                };
            }
            return level switch
            {
                1 => "Projects",
                2 => "Milestones",
                3 => "Work items",
                4 => "Activity log",
                _ => "Projects"
            };
        }

        /* Purpose: Read all SP rows as string dictionaries (flexible columns per Level). */
        private async Task<List<Dictionary<string, string?>>> ReadSpRowsAsStringDictAsync(
            string spName, List<SqlParameter> sqlParams)
        {
            var list = new List<Dictionary<string, string?>>();
            await using var reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, spName, sqlParams);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    if (reader.IsDBNull(i))
                    {
                        row[name] = null;
                        continue;
                    }
                    var raw = reader.GetValue(i);
                    if (raw is IFormattable fmt)
                        row[name] = fmt.ToString(null, CultureInfo.InvariantCulture);
                    else
                        row[name] = Convert.ToString(raw, CultureInfo.InvariantCulture);
                }
                list.Add(row);
            }
            return list;
        }

        /* Purpose: Read every result set from an SP as string dictionaries (chart SPs with meta + series). */
        private async Task<List<List<Dictionary<string, string?>>>> ReadSpMultiResultSetsAsStringDictAsync(
            string spName, List<SqlParameter> sqlParams)
        {
            var sets = new List<List<Dictionary<string, string?>>>();
            await using var reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, spName, sqlParams);
            do
            {
                var list = new List<Dictionary<string, string?>>();
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        if (reader.IsDBNull(i))
                        {
                            row[name] = null;
                            continue;
                        }
                        var raw = reader.GetValue(i);
                        if (raw is IFormattable fmt)
                            row[name] = fmt.ToString(null, CultureInfo.InvariantCulture);
                        else
                            row[name] = Convert.ToString(raw, CultureInfo.InvariantCulture);
                    }
                    list.Add(row);
                }
                sets.Add(list);
            }
            while (await reader.NextResultAsync().ConfigureAwait(false));
            return sets;
        }

        /* Purpose: Blank Flag → all KPI Flags; else one Flag if known. */
        private static List<string> ResolveKpiFlagList(string? flag)
        {
            var trimmed = NullIfBlank(flag);
            if (trimmed == null)
                return KpiCardRegistry.Select(x => x.Flag).ToList();

            var normalized = NormalizeFlag(trimmed);
            var match = KpiCardRegistry.FirstOrDefault(x =>
                x.Flag.Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (match == null) return new List<string>();
            return new List<string> { match.Flag };
        }

        /* Purpose: Find Flag definition (Label, SpName, Unit). */
        private static KpiFlagDef? FindKpiFlag(string flag)
        {
            return KpiCardRegistry.FirstOrDefault(x =>
                x.Flag.Equals(flag, StringComparison.OrdinalIgnoreCase));
        }

        /* Purpose: One path for every card — Flag → SP; bind that SP’s row to the card. */
        private async Task<AnalyticsDBKPICardModel> LoadKpiCard(
            string flag,
            AnalyticsDBKPICardsRequest request,
            DateTime currentFrom,
            DateTime currentTo,
            // Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
            // DateTime previousFrom,
            // DateTime previousTo)
            DateTime? previousFrom,
            DateTime? previousTo)
            // End of Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
        {
            var meta = FindKpiFlag(flag);
            if (meta == null)
            {
                return new AnalyticsDBKPICardModel
                {
                    Flag = flag,
                    Label = flag,
                    Trend = "FLAT",
                    IsImplemented = false
                };
            }

            if (string.IsNullOrWhiteSpace(_connectionString))
                return MapKpiCard(meta, null);

            try
            {
                // Added by Vikas T on 30-09-2026 - ActiveProjects card ignores Organization Unit (RegionIDs)
                // like its drill, so L0 count = L1 rows. Other cards keep RegionIDs (kpiFlag null).
                var sqlParams = BuildKpiFilterParameters(request, currentFrom, currentTo, previousFrom, previousTo,
                    string.Equals(flag, "ActiveProjects", StringComparison.OrdinalIgnoreCase) ? flag : null);
                // Added by Vikas T on 16-09-2026 - Util/Bench/Active card bind needs @DrillLevel=0
                // (Bench + ActiveProjects SP default is 1 = drill grid, not KPI summary).
                if (KpiFlagsRequiringDrillLevelZero.Contains(flag))
                {
                    sqlParams.Add(new SqlParameter("@DrillLevel", SqlDbType.TinyInt) { Value = (byte)0 });
                }
                // Manual DataReader bind — AutoMapper.DataReader fails on DECIMAL/FLOAT → model
                var row = await ReadKpiSpRowAsync(meta.SpName, sqlParams);
                return MapKpiCard(meta, row);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadKpiCard {flag} / {meta.SpName}: {ex}");
                return MapKpiCard(meta, null);
            }
        }

        /* Purpose: Execute Flag SP and read first row without AutoMapper. */
        private async Task<AnalyticsDBKPISpRowModel?> ReadKpiSpRowAsync(string spName, List<SqlParameter> sqlParams)
        {
            await using var reader = await new MSSQL().ExecuteReaderAsyncSP(_connectionString, spName, sqlParams);
            if (!await reader.ReadAsync().ConfigureAwait(false))
                return null;

            return new AnalyticsDBKPISpRowModel
            {
                // Added by Vikas T on 16-09-2026 - Map Util/Bench/Active L0 columns onto common card shape
                CurrentValue = ReadReaderString(reader,
                    "CurrentValue", "CurrentRevenue", "GrossProfitValue", "GrossMarginValue",
                    "UtilizationPct", "BenchFTE", "ActiveProjectsCount"),
                PreviousValue = ReadReaderString(reader,
                    "PreviousValue", "PreviousRevenue", "PreviousGrossProfit", "PreviousMarginPct",
                    "PreviousUtilizationPct", "PreviousBenchFTE", "PreviousActiveProjectsCount"),
                ValueChange = ReadReaderString(reader, "ValueChange", "RevenueChange"),
                // Added by Vikas T on 16-09-2026 - Util/Bench/Active return DeltaPct (not PercentageChange)
                PercentageChange = ReadReaderString(reader, "PercentageChange", "DeltaPct"),
                TrendDirection = ReadReaderString(reader, "TrendDirection"),
                TrendColorCode = ReadReaderString(reader, "TrendColorCode"),
                Note = ReadReaderString(reader, "Note"),
                // Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
                Badge = ReadReaderString(reader, "Badge"),
                // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
                BaseCurrencyCode = ReadReaderString(reader, "BaseCurrencyCode"),
                CurrencyCode = ReadReaderString(reader, "CurrencyCode"),
                Unit = ReadReaderString(reader, "Unit"),
                CurrentRevenue = ReadReaderString(reader, "CurrentRevenue"),
                PreviousRevenue = ReadReaderString(reader, "PreviousRevenue"),
                RevenueChange = ReadReaderString(reader, "RevenueChange")
            };
        }

        /* Purpose: Read column by name (case-insensitive), any SQL type → string. */
        private static string? ReadReaderString(SqlDataReader reader, params string[] columnNames)
        {
            if (reader == null || columnNames == null) return null;
            for (var c = 0; c < columnNames.Length; c++)
            {
                var want = columnNames[c];
                if (string.IsNullOrWhiteSpace(want)) continue;
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (!string.Equals(reader.GetName(i), want, StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (reader.IsDBNull(i)) return null;
                    var raw = reader.GetValue(i);
                    if (raw == null || raw is DBNull) return null;
                    if (raw is IFormattable fmt)
                        return fmt.ToString(null, CultureInfo.InvariantCulture);
                    return Convert.ToString(raw, CultureInfo.InvariantCulture);
                }
            }
            return null;
        }

        /* Purpose: Parse SP numeric string (AutoMapper-safe) to decimal. */
        private static decimal ParseSpDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            var s = value.Trim().Replace(",", "").Replace(" ", "");
            // Strip common currency symbols / units if SP ever returns formatted text
            s = s.Replace("₹", "", StringComparison.Ordinal)
                 .Replace("S$", "", StringComparison.OrdinalIgnoreCase)
                 .Replace("$", "", StringComparison.Ordinal);
            if (s.EndsWith("Cr", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(0, s.Length - 2).Trim();
            else if (s.EndsWith("k", StringComparison.OrdinalIgnoreCase) ||
                     s.EndsWith("L", StringComparison.OrdinalIgnoreCase) ||
                     s.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(0, s.Length - 1).Trim();
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out d))
                return d;
            // Extract first numeric token
            var m = System.Text.RegularExpressions.Regex.Match(s, @"-?\d+(\.\d+)?([eE][+-]?\d+)?");
            if (m.Success &&
                decimal.TryParse(m.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;
            return 0m;
        }

        //Added by Aditya J. on 21-09-2026 Cost & budget variance by project
        private static bool ParseSpBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var s = value.Trim();
            if (s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase)
                || s.Equals("yes", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
        //End of Added by Aditya J. on 21-09-2026 Cost & budget variance by project

        /* Purpose: SP row → one KPI card model (Flag / Label / Unit from registry). */
        private static AnalyticsDBKPICardModel MapKpiCard(KpiFlagDef meta, AnalyticsDBKPISpRowModel? row)
        {
            var note = NullIfBlank(row?.Note);
            // % KPIs (Gross Margin, Utilization, …) — do not surface Note on cards
            if (string.Equals(meta.Unit, "Pct", StringComparison.OrdinalIgnoreCase))
                note = null;

            var current = ParseSpDecimal(row?.CurrentValue);
            var previous = ParseSpDecimal(row?.PreviousValue);
            var change = ParseSpDecimal(row?.ValueChange);
            var pct = ParseSpDecimal(row?.PercentageChange);

            // Always try legacy revenue aliases when primary current is 0
            if (current == 0m)
            {
                var legacyCurrent = ParseSpDecimal(row?.CurrentRevenue);
                if (legacyCurrent != 0m) current = legacyCurrent;
            }
            if (previous == 0m)
            {
                var legacyPrevious = ParseSpDecimal(row?.PreviousRevenue);
                if (legacyPrevious != 0m) previous = legacyPrevious;
            }

            // Fall back to legacy Revenue column names if common columns are empty
            if (current == 0m && previous == 0m && change == 0m && pct == 0m)
            {
                current = ParseSpDecimal(row?.CurrentRevenue);
                previous = ParseSpDecimal(row?.PreviousRevenue);
                change = ParseSpDecimal(row?.RevenueChange);
                if (change == 0m) change = current - previous;
            }
            else if (change == 0m)
            {
                change = current - previous;
            }

            var trend = row == null || string.IsNullOrWhiteSpace(row.TrendDirection)
                ? ""
                : row.TrendDirection.Trim().ToUpperInvariant();

            // Derive UP/DOWN when SP only returns PercentageChange (GrossProfit / GrossMargin legacy)
            if (string.IsNullOrWhiteSpace(trend) || trend == "NONE")
            {
                if (pct > 0.05m) trend = "UP";
                else if (pct < -0.05m) trend = "DOWN";
                else trend = "FLAT";
            }

            // Purpose: 0.0% / FLAT → grey default (no green/red TrendColorCode).
            if (Math.Abs(pct) < 0.05m || trend == "FLAT" || trend == "NONE" || trend == "")
            {
                trend = "FLAT";
            }

            var trendColor = trend == "FLAT"
                ? null
                : NullIfBlank(row?.TrendColorCode)
                  ?? (trend == "UP" ? "#0D9488" : "#E11D48");

            /* Purpose: Money KPIs — SP Unit (k/L/Cr/M) by amount; do not force registry Cr.
               Dipali V. 22-09-2026 — if CurrentValue is 0.00, Unit must stay blank (no k/L/Cr/M / Score suffix). */
            string unit;
            if (string.Equals(meta.Unit, "Cr", StringComparison.OrdinalIgnoreCase)
                && row != null && row.Unit != null)
            {
                unit = string.IsNullOrWhiteSpace(row.Unit) ? "Abs" : row.Unit.Trim();
            }
            else
            {
                unit = NullIfBlank(row?.Unit) ?? meta.Unit;
            }

            if (Math.Abs(current) < 0.005m
                && (string.Equals(meta.Unit, "Cr", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(meta.Unit, "Score", StringComparison.OrdinalIgnoreCase)
                    || IsMoneyScaleUnit(unit)))
            {
                unit = "";
            }

            return new AnalyticsDBKPICardModel
            {
                Flag = meta.Flag,
                Label = meta.Label,
                SpName = meta.SpName,
                Unit = unit,
                GoodUp = meta.GoodUp,
                CurrentValue = current,
                PreviousValue = previous,
                ValueChange = change,
                DeltaPct = pct,
                Trend = trend,
                TrendColorCode = trendColor,
                Note = note,
                Insight = note,
                // Added By Vyankat B. on the 16th sep 2026 for the Delayed Projects watch badge
                // Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
                Badge = string.Equals(meta.Flag, "DelayedProjects", StringComparison.OrdinalIgnoreCase) && current > 0m
                    ? "watch"
                    : (string.Equals(meta.Flag, "CSAT", StringComparison.OrdinalIgnoreCase)
                        ? NullIfBlank(row?.Badge)
                        : null),
                // End of Commented and Added By Vyankat b. on 24th Sep 2026 for the CSAT Goal Below badge
                BaseCurrencyCode = NullIfBlank(row?.BaseCurrencyCode) ?? "",
                CurrencyCode = NullIfBlank(row?.CurrencyCode),
                IsImplemented = true
            };
        }

        /* Purpose: SP money scale units (value-based). */
        private static bool IsMoneyScaleUnit(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit)) return false;
            return unit.Equals("k", StringComparison.OrdinalIgnoreCase)
                || unit.Equals("L", StringComparison.OrdinalIgnoreCase)
                || unit.Equals("Cr", StringComparison.OrdinalIgnoreCase)
                || unit.Equals("M", StringComparison.OrdinalIgnoreCase)
                || unit.Equals("Abs", StringComparison.OrdinalIgnoreCase);
        }        /* Purpose: Shared SQL params for every KPI SP (calendar + chips). */
        private static List<SqlParameter> BuildKpiFilterParameters(
            AnalyticsDBKPICardsRequest request,
            DateTime currentFrom,
            DateTime currentTo,
            // Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
            // DateTime previousFrom,
            // DateTime previousTo)
            DateTime? previousFrom,
            DateTime? previousTo,
            string? kpiFlag = null)

            // End of Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
        {
            // Added by Vikas T on 29-09-2026 - @RegionIDs (Organization Unit) is sent only to
            // Bench and Utilization KPI cards. Mix / Revenue callers omit kpiFlag so they keep RegionIDs.
            var regionIds = kpiFlag == null
                ? NullIfBlank(request.RegionIDs)
                : (string.Equals(kpiFlag, "Bench", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kpiFlag, "Utilization", StringComparison.OrdinalIgnoreCase)
                    ? NullIfBlank(request.RegionIDs)
                    : null);

            return new List<SqlParameter>
            {
                new SqlParameter("@CurrentFromDate", currentFrom),
                new SqlParameter("@CurrentToDate", currentTo),
                // new SqlParameter("@PreviousFromDate", previousFrom),
                // new SqlParameter("@PreviousToDate", previousTo),
                new SqlParameter("@PreviousFromDate", SqlDbType.DateTime)
                {
                    Value = (object?)previousFrom ?? DBNull.Value
                },
                new SqlParameter("@PreviousToDate", SqlDbType.DateTime)
                {
                    Value = (object?)previousTo ?? DBNull.Value
                },
                // End of Commented and Added By Vyankat b. on 22th Sep 2026 for the Previous dates optional when Compare is off
                new SqlParameter("@CustomerIDs", (object?)NullIfBlank(request.CustomerIDs) ?? DBNull.Value),
                new SqlParameter("@ProjectManagerIDs", (object?)NullIfBlank(request.ProjectManagerIDs) ?? DBNull.Value),
                new SqlParameter("@PortfolioIDs", (object?)NullIfBlank(request.PortfolioIDs) ?? DBNull.Value),
                new SqlParameter("@RegionIDs", (object?)regionIds ?? DBNull.Value),
                new SqlParameter("@BillingTypeIDs", (object?)NullIfBlank(request.BillingTypeIDs) ?? DBNull.Value),
                new SqlParameter("@HealthIDs", (object?)NullIfBlank(request.HealthIDs) ?? DBNull.Value),
                new SqlParameter("@DashboardID", request.DashboardID)
            };
        }

        // Added by Dipali V. on 09-09-2026 - Save view (user + dashboard specific)
        public async Task<ResponseEntity> SaveUserFilter(AnalyticsDBUserFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBUserFilterRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (!TryParsePositiveInt(request.SessionEmployeeID, out var employeeId))
                    return Failure("SessionEmployeeID is required and must be a valid integer.");

                var filterJson = NullIfBlank(request.FilterJson);
                if (filterJson == null)
                    return Failure("FilterJson is required.");

                if (NullIfBlank(request.FilterName) == null)
                    return Failure("FilterName is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@UserFilterID",
                        request.UserFilterID.GetValueOrDefault() > 0
                            ? (object)request.UserFilterID!.Value
                            : DBNull.Value),
                    new SqlParameter("@FilterName", (object?)NullIfBlank(request.FilterName) ?? DBNull.Value),
                    new SqlParameter("@FilterJson", filterJson),
                    new SqlParameter("@IsDefaultFilter", request.IsDefaultFilter)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBUserFilterModel>(
                    "usp_Whizible2_InsUpd_AnalyticsDBUserFilter", sqlParams);

                return Success("User filter saved", ExtractList<AnalyticsDBUserFilterModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of SaveUserFilter

        // Added by Dipali V. on 09-09-2026 - Get saved views / default for login persist
        public async Task<ResponseEntity> GetUserFilter(AnalyticsDBUserFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBUserFilterRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (!TryParsePositiveInt(request.SessionEmployeeID, out var employeeId))
                    return Failure("SessionEmployeeID is required and must be a valid integer.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@UserFilterID",
                        request.UserFilterID.GetValueOrDefault() > 0
                            ? (object)request.UserFilterID!.Value
                            : DBNull.Value),
                    new SqlParameter("@GetDefaultOnly", request.GetDefaultOnly)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBUserFilterModel>(
                    "usp_Whizible2_Sel_AnalyticsDBUserFilter", sqlParams);

                return Success("User filter list", ExtractList<AnalyticsDBUserFilterModel>(result));
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of GetUserFilter

        // Added by Dipali V. on 09-09-2026 - Set or remove default saved view
        public async Task<ResponseEntity> SetDefaultUserFilter(AnalyticsDBUserFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBUserFilterRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (!TryParsePositiveInt(request.SessionEmployeeID, out var employeeId))
                    return Failure("SessionEmployeeID is required and must be a valid integer.");

                if (!request.UserFilterID.HasValue || request.UserFilterID.Value <= 0)
                    return Failure("UserFilterID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@UserFilterID", request.UserFilterID.Value),
                    new SqlParameter("@IsDefaultFilter", request.IsDefaultFilter)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBUserFilterModel>(
                    "usp_Whizible2_Upd_AnalyticsDBUserFilter_SetDefault", sqlParams);

                var rows = ExtractList<AnalyticsDBUserFilterModel>(result);
                if (rows.Count == 0)
                    return Failure("Saved filter was not found for this user and dashboard.");

                return Success(
                    request.IsDefaultFilter ? "Default filter set" : "Default filter removed",
                    rows);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of SetDefaultUserFilter

        // Added by Dipali V. on 10-09-2026 - Soft-delete login-specific saved view
        public async Task<ResponseEntity> DeleteUserFilter(AnalyticsDBUserFilterRequest request)
        {
            try
            {
                if (_repository == null) return RepositoryNotConfigured();
                if (request == null) request = new AnalyticsDBUserFilterRequest();

                if (!IsValidDashboardId(request.DashboardID))
                    return Failure("DashboardID is required.");

                if (!TryParsePositiveInt(request.SessionEmployeeID, out var employeeId))
                    return Failure("SessionEmployeeID is required and must be a valid integer.");

                if (!request.UserFilterID.HasValue || request.UserFilterID.Value <= 0)
                    return Failure("UserFilterID is required.");

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@DashboardID", request.DashboardID),
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@UserFilterID", request.UserFilterID.Value)
                };

                var result = await _repository.GetAsyncSP<AnalyticsDBUserFilterModel>(
                    "usp_Whizible2_Del_AnalyticsDBUserFilter", sqlParams);

                var rows = ExtractList<AnalyticsDBUserFilterModel>(result);
                if (rows.Count == 0)
                    return Failure("Saved filter was not found for this user and dashboard.");

                return Success("User filter deleted", rows);
            }
            catch (Exception ex)
            {
                return Failure(ex);
            }
        }
        // End of DeleteUserFilter
    }
}
