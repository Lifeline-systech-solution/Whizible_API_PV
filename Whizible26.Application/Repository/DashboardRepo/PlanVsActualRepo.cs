
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
    public class PlanVsActualRepo : BaseRepository<PlanVsActualDashboard>
    {
        PlanVsActualRepo() { }

        public PlanVsActualRepo(string connectionstring) : base(connectionstring) { }
    }
}

// End of Repository Added by <Name> on 28-07-2026
