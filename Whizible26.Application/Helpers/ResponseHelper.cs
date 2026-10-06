using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhizibleTeams.Domain.Entity;

namespace WhizibleTeams.Application.Helpers
{
    public static class ResponseHelper
    {

        public static IActionResult BuildResponse(ControllerBase controller, ResponseEntity response)
        {
            if (response.Status.Equals(ResponseStatus.FAILURE))
            {
                return controller.BadRequest(response.Data);
            }
            else
            {
                return controller.Ok(response.Data);
            }
        }




    }
}
