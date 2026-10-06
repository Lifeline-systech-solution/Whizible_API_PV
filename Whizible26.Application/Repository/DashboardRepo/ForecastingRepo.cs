using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries;
using Whizible26.Application.CommandsQueries.Dashboard;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.DashboardRepo
{
    // Added by <Name> on 12-08-2026 - Repository for the Forecasting dashboard.
    // Thin wrapper over BaseRepository<T>, same pattern as PlanVsActualRepo / ProjectHealthRepo.
    public class ForecastingRepo : BaseRepository<ForecastingDashboard>
    {
        ForecastingRepo() { }

        public ForecastingRepo(string connectionstring) : base(connectionstring) { }
    }
}

// End of Repository Added by <Name> on 12-08-2026
