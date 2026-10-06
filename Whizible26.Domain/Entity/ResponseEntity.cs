using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhizibleTeams.Domain.Entity
{

    public enum ResponseStatus
    {
        SUCCESS,
        FAILURE,
        UNAUTHORIZED
    }
    public class ResponseEntity
    {

        public ResponseStatus Status { get; set; }
        public object? Data { get; set; }

    }
}
