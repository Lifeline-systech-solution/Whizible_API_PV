using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whizible26.Application.CommandsQueries;
using WhizibleTeams.Application.Repository;

namespace Whizible26.Application.Repository.ReportsRepo
{
    public class SkillsInventoryRepo : BaseRepository<SkillsInventoryReport>
    {
        SkillsInventoryRepo() { }


        public SkillsInventoryRepo(string connectionstring) : base(connectionstring) { }
    }
}

