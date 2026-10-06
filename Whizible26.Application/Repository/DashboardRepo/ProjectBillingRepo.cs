
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
    // Added by Aditya J. on 11-08-2026 for Project Billing dashboard API
    public class ProjectBillingRepo : BaseRepository<ProjectBilling>
    {
        ProjectBillingRepo() { }

        public ProjectBillingRepo(string connectionstring) : base(connectionstring) { }
    }
    // End of Added by Aditya J. on 11-08-2026 for Project Billing dashboard API
}
