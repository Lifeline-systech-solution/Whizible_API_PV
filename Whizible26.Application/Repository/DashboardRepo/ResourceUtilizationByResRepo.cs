
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
    // Added for Resource Utilization By Resource API
    public class ResourceUtilizationByResRepo : BaseRepository<ResourceUtilizationByRes>
    {
        ResourceUtilizationByResRepo() { }

        public ResourceUtilizationByResRepo(string connectionstring) : base(connectionstring) { }
    }
    // End of Added for Resource Utilization By Resource API
}
