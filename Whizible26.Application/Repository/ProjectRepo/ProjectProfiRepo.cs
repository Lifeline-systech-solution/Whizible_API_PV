using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.ProjectRepo
{
    public class ProjectProfiRepo : BaseRepository<ProjectProfitability>
    {
        ProjectProfiRepo() { }

        public ProjectProfiRepo(string connectionstring) : base(connectionstring) { }
    }
}

//End of Repository Added by Vyankat B. on 15-12-2025
