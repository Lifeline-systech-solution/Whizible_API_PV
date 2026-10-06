using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries.Dashboard;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.DashboardRepo
{
    // Added by Vyankat B. on 07-08-2026 - Repository for Project Profitability Dashboard
    public class PM_ProjectProfDashRepo : BaseRepository<PM_ProjectProfDash>
    {
        PM_ProjectProfDashRepo() { }

        public PM_ProjectProfDashRepo(string connectionstring) : base(connectionstring) { }
    }
    // End of Repository Added by Vyankat B. on 07-08-2026
}
