using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace WhizibleTeams.API.Attributes
{
    // No-op replacement for the original audit filter.
    // This keeps controller compilation working after your auth-only cleanup removed
    // the audit implementation dependencies (TokenAPI/TransactionAudit).
    public class AuthorizeAuditAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            return Task.CompletedTask;
        }
    }
}

