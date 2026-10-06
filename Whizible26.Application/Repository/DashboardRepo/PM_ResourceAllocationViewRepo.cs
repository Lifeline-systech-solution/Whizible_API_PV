using Whizible26.Application.CommandsQueries.Dashboard;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.DashboardRepo
{
    // Added by Vyankat B. on 11-08-2026 for the Resource Allocation View
    public class PM_ResourceAllocationViewRepo : BaseRepository<PM_ResourceAllocationView>
    {
        PM_ResourceAllocationViewRepo() { }

        public PM_ResourceAllocationViewRepo(string connectionstring) : base(connectionstring) { }
    }
}

