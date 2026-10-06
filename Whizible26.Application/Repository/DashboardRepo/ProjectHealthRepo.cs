using Whizible26.Application.CommandsQueries.Dashboard;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.DashboardRepo
{
    // Added by <Name> on 07-08-2026 - Repository for the Project Health dashboard.
    // NOTE / ASSUMPTION: this relies on BaseRepository already exposing
    // GetAsyncSP<T1..T5>(sp, params) (confirmed from PlanVsActualRepo), plus
    // GetScalarAsyncSP<T>(sp, params) and ExecuteAsyncSP(sp, params) for the
    // scalar (EV-applicable flag, chkFilterExists-style) and non-query (save
    // SQERT values) calls this dashboard also needs. If BaseRepository does not
    // yet have those two members they'll need to be added there -- flagging this
    // explicitly rather than guessing at their signatures.
    public class ProjectHealthRepo : BaseRepository<ProjectHealthDashboard>
    {
        ProjectHealthRepo() { }

        public ProjectHealthRepo(string connectionstring) : base(connectionstring) { }
    }
}

// End of Repository Added by <Name> on 07-08-2026
