using Whizible.PRValidator;
using Whizible.PRValidator.Models;
using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Validators;

namespace WhiziblePRValidator.Tests;

public class ApiValidatorTests
{
    [Fact]
    public void PostWithRoute_AndAllAttributes_Passes()
    {
        var source = """
            using Microsoft.AspNetCore.Mvc;
            public class EmployeeController : ControllerBase
            {
                [HttpPost("GetAnalyticsDBFilterFlagWiseDependent")]
                [Authorize]
                [ServiceFilter(typeof(AuthorizeAuditAttribute))]
                [ServiceFilter(typeof(ValidateHeadersAttribute))]
                public async Task<IActionResult> GetAnalyticsDBFilterFlagWiseDependent()
                {
                    return null;
                }
            }
            """;
        Assert.Empty(Errors(source));
    }

    [Fact]
    public void GetEndpoint_WithAllAttributes_Passes()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpGet]
                [Authorize]
                [ServiceFilter(typeof(AuthorizeAuditAttribute))]
                [ServiceFilter(typeof(ValidateHeadersAttribute))]
                public IActionResult GetData()
                {
                    return null;
                }
            }
            """;
        Assert.Empty(Errors(source));
    }

    [Fact]
    public void PutEndpoint_WithAllAttributes_Passes()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpPut("{id}")]
                [Authorize]
                [ServiceFilter(typeof(AuthorizeAuditAttribute))]
                [ServiceFilter(typeof(ValidateHeadersAttribute))]
                public IActionResult Update(int id)
                {
                    return null;
                }
            }
            """;
        Assert.Empty(Errors(source));
    }

    [Fact]
    public void ControllerLevelAttributes_CoverEndpoints()
    {
        var source = """
            [Authorize]
            [ServiceFilter(typeof(AuthorizeAuditAttribute))]
            [ServiceFilter(typeof(ValidateHeadersAttribute))]
            public class ProjectHealthDashboardController : ControllerBase
            {
                [HttpPost("SaveFilter")]
                public Task<IActionResult> SaveFilter()
                {
                    return null;
                }
            }
            """;
        Assert.Empty(Errors(source, acceptControllerLevel: true));
    }

    [Fact]
    public void MethodLevelRequired_DoesNotInheritControllerAttributes()
    {
        var source = """
            [Authorize]
            [ServiceFilter(typeof(AuthorizeAuditAttribute))]
            [ServiceFilter(typeof(ValidateHeadersAttribute))]
            public class ProjectHealthDashboardController : ControllerBase
            {
                [HttpPost("SaveFilter")]
                public Task<IActionResult> SaveFilter()
                {
                    return null;
                }
            }
            """;
        var issues = Errors(source, acceptControllerLevel: false);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorize);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorizeAudit);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiValidateHeaders);
    }

    [Fact]
    public void MissingAuthorize_IsReported()
    {
        var issues = Errors(EndpointWithout("[Authorize]"));
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorize && i.MethodName == "GetAnalyticsDBFilterFlagWiseDependent");
    }

    [Fact]
    public void MissingAuthorizeAudit_IsReported()
    {
        var issues = Errors(EndpointWithout("[ServiceFilter(typeof(AuthorizeAuditAttribute))]"));
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorizeAudit);
    }

    [Fact]
    public void MissingValidateHeaders_IsReported()
    {
        var issues = Errors(EndpointWithout("[ServiceFilter(typeof(ValidateHeadersAttribute))]"));
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiValidateHeaders);
    }

    [Fact]
    public void MissingTwoAttributes_IsReported()
    {
        var source = EndpointWithout("[Authorize]", "[ServiceFilter(typeof(AuthorizeAuditAttribute))]");
        var issues = Errors(source);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorize);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorizeAudit);
        Assert.DoesNotContain(issues, i => i.RuleId == RuleIds.ApiValidateHeaders);
    }

    [Fact]
    public void MissingAllThreeAttributes_IsReported()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpPost("GetAnalyticsDBFilterFlagWiseDependent")]
                public async Task<IActionResult> GetAnalyticsDBFilterFlagWiseDependent()
                {
                    return null;
                }
            }
            """;
        var issues = Errors(source);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorize);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiAuthorizeAudit);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiValidateHeaders);
    }

    [Fact]
    public void CommentedAttributes_AreNotTreatedAsPresent()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpPost("Save")]
                //[Authorize]
                //[ServiceFilter(typeof(AuthorizeAuditAttribute))]
                //[ServiceFilter(typeof(ValidateHeadersAttribute))]
                public IActionResult Save()
                {
                    var note = "[Authorize]";
                    return null;
                }
            }
            """;
        var issues = Errors(source);
        Assert.Equal(3, issues.Count(i => i.RuleId.StartsWith("API-00", StringComparison.Ordinal) && i.RuleId != RuleIds.ApiCSharpSyntax));
    }

    [Fact]
    public void CommentedEndpoint_IsIgnored()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                // [HttpPost("Hidden")]
                // public IActionResult Hidden() { return null; }
            }
            """;
        Assert.Empty(Errors(source));
    }

    [Fact]
    public void SyntaxError_IsReported()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpGet]
                [Authorize]
                [ServiceFilter(typeof(AuthorizeAuditAttribute))]
                [ServiceFilter(typeof(ValidateHeadersAttribute))]
                public IActionResult GetData()
                {
                    var name = "Aditya"
                    return null;
                }
            }
            """;
        var issues = Errors(source);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiCSharpSyntax && i.Message.Contains("';", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidEndpointDeclaration_IsReported()
    {
        var source = """
            public class EmployeeController : ControllerBase
            {
                [HttpPost]
                public void Save(
                {
                }
            }
            """;
        var issues = Errors(source);
        Assert.Contains(issues, i => i.RuleId == RuleIds.ApiCSharpSyntax);
    }

    [Fact]
    public void ChangedFileList_ValidatesOnlyListedController()
    {
        var root = Path.Combine(Path.GetTempPath(), "whizible-pr-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var controller = Path.Combine(root, "EmployeeController.cs");
        File.WriteAllText(controller, """
            public class EmployeeController : ControllerBase
            {
                [HttpGet]
                public IActionResult GetData() { return null; }
            }
            """);
        var list = Path.Combine(root, "changed.txt");
        File.WriteAllText(list, "EmployeeController.cs");
        var result = ValidationRunner.Run(new ValidatorOptions
        {
            RepositoryRoot = root,
            ChangedFilesPath = list,
            ProbeSystemWeb = false
        });
        Assert.Contains(result.Issues, i => i.RuleId == RuleIds.ApiAuthorize);
        Assert.False(result.Passed);
    }

    private static List<ValidationError> Errors(string source, bool acceptControllerLevel = true)
    {
        var path = Path.Combine(Path.GetTempPath(), "whizible-pr-" + Guid.NewGuid().ToString("N") + "Controller.cs");
        File.WriteAllText(path, source);
        var result = new ValidationResult();
        new ApiValidator(acceptControllerLevel).ValidateFile(path, result, new ValidatorOptions());
        return result.Issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
    }

    private static string EndpointWithout(params string[] removed)
    {
        var attributes = new List<string>
        {
            "[Authorize]",
            "[ServiceFilter(typeof(AuthorizeAuditAttribute))]",
            "[ServiceFilter(typeof(ValidateHeadersAttribute))]"
        };
        attributes.RemoveAll(a => removed.Contains(a));
        return """
            public class EmployeeController : ControllerBase
            {
                [HttpPost("GetAnalyticsDBFilterFlagWiseDependent")]
            """ + string.Join(Environment.NewLine, attributes) + """
                public async Task<IActionResult> GetAnalyticsDBFilterFlagWiseDependent()
                {
                    return null;
                }
            }
            """;
    }
}
