using Whizible.PRValidator.Rules;
using Whizible.PRValidator.Services;
using Whizible.PRValidator.Validators;

namespace WhiziblePRValidator.Tests;

public class AspxValidatorTests
{
    private static AspNetControlCatalog Catalog()
    {
        var catalog = new AspNetControlCatalog { LoadedFromAssemblies = true };
        catalog.ServerControlNames.Add("Label");
        catalog.ServerControlNames.Add("TextBox");
        catalog.ServerControlNames.Add("GridView");
        catalog.ServerControlNames.Add("Button");
        catalog.NamingContainerNames.Add("Repeater");
        catalog.NamingContainerNames.Add("GridView");
        return catalog;
    }

    [Fact]
    public void ValidPage_HasNoErrors()
    {
        var markup = """
            <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Employee.aspx.cs" Inherits="Whizible.EmployeePage" %>
            <html>
            <head runat="server"></head>
            <body>
              <form runat="server">
                <asp:Label ID="lblName" runat="server" Text="Aditya" />
                <asp:TextBox ID="txtName" runat="Server" />
              </form>
            </body>
            </html>
            """;

        var (document, issues) = new AspxMarkupParser(markup, "Employee.aspx", Catalog(), true).Parse();
        Assert.DoesNotContain(issues, i => i.Severity == Whizible.PRValidator.Models.ValidationSeverity.Error);
        Assert.Contains(document.Directives, d => d.Name.Equals("Page", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(2, document.ServerElements.Count);
    }

    [Fact]
    public void UnclosedTag_IsReported()
    {
        var markup = """
            <html>
              <asp:Label ID="lblName" runat="server">
            </html>
            """;
        var issues = Parse(markup);
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxMarkup && i.Message.Contains("Unclosed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InvalidServerControl_IsReported()
    {
        var issues = Parse("""<asp:NotARealControl ID="x" runat="server" />""");
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxServerControl);
    }

    [Fact]
    public void RegisteredCustomControl_IsAccepted()
    {
        var markup = """
            <%@ Register TagPrefix="uc" TagName="Header" Src="Header.ascx" %>
            <uc:Header ID="header" runat="server" />
            """;
        var issues = Parse(markup);
        Assert.DoesNotContain(issues, i => i.RuleId == RuleIds.AspxServerControl);
    }

    [Fact]
    public void DuplicateServerId_IsReported()
    {
        var markup = """
            <asp:Label ID="lblName" runat="server" />
            <asp:TextBox ID="lblName" runat="server" />
            """;
        var issues = Parse(markup);
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxDuplicateId);
    }

    [Fact]
    public void DuplicateIdInsideSeparateNamingContainers_IsAllowed()
    {
        var markup = """
            <asp:Repeater ID="repA" runat="server">
              <ItemTemplate><asp:Label ID="lblName" runat="server" /></ItemTemplate>
            </asp:Repeater>
            <asp:Repeater ID="repB" runat="server">
              <ItemTemplate><asp:Label ID="lblName" runat="server" /></ItemTemplate>
            </asp:Repeater>
            """;
        var issues = Parse(markup);
        Assert.DoesNotContain(issues, i => i.RuleId == RuleIds.AspxDuplicateId);
    }

    [Fact]
    public void InvalidRunat_IsReported_AndServerCasingIsValid()
    {
        var bad = Parse("""<div ID="panel" runat="client"></div>""");
        Assert.Contains(bad, i => i.RuleId == RuleIds.AspxRunat);

        var good = Parse("""<asp:Label ID="lblName" runat="Server" />""");
        Assert.DoesNotContain(good, i => i.RuleId == RuleIds.AspxRunat && i.Message.Contains("Invalid runat", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingRunatOnServerControl_IsReported()
    {
        var issues = Parse("""<asp:Label ID="lblName" />""");
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxRunat);
    }

    [Fact]
    public void InvalidRegisterDirective_IsReported()
    {
        var markup = """<%@ Register TagPrefix="uc" %>""";
        var directory = Path.Combine(Path.GetTempPath(), "whizible-pr-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var page = Path.Combine(directory, "Page.aspx");
        File.WriteAllText(page, markup);
        var result = new Whizible.PRValidator.Models.ValidationResult();
        new AspxValidator(Catalog()).ValidateMarkup(page, result, false);
        Assert.Contains(result.Issues, i => i.RuleId == RuleIds.AspxDirective && i.Severity == Whizible.PRValidator.Models.ValidationSeverity.Error);
    }

    [Fact]
    public void ScriptComparison_IsNotParsedAsATag()
    {
        var markup = """
            <html>
            <script type="text/javascript">
              if (labels.length < 10 && value > 1) { var html = "<div>"; }
            </script>
            </html>
            """;
        var issues = Parse(markup);
        Assert.DoesNotContain(issues, i => i.RuleId == RuleIds.AspxMarkup);
    }

    [Fact]
    public void MissingSemicolon_IsReported()
    {
        var source = """
            public class EmployeePage
            {
                public void SaveEmployee()
                {
                    string employeeName = "Aditya"
                }
            }
            """;
        var issues = new AspxCodeBehindValidator().ValidateSyntax("Employee.aspx.cs", source).ToList();
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxMissingSemicolon && i.Message.Contains("';", StringComparison.Ordinal));
    }

    [Fact]
    public void MissingBrace_IsReported()
    {
        var source = """
            public class EmployeePage
            {
                public void Test()
                {
                    if (true)
                    {
                        DoSomething();
                }
            }
            """;
        var issues = new AspxCodeBehindValidator().ValidateSyntax("Employee.aspx.cs", source).ToList();
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxMissingBrace);
    }

    [Fact]
    public void InvalidMethodDeclaration_IsReported()
    {
        var source = """
            public class EmployeePage
            {
                public void SaveEmployee(
                {
                }
            }
            """;
        var issues = new AspxCodeBehindValidator().ValidateSyntax("Employee.aspx.cs", source).ToList();
        Assert.Contains(issues, i => i.RuleId is RuleIds.AspxInvalidMethod or RuleIds.AspxCSharpSyntax or RuleIds.AspxMissingBrace);
    }

    [Fact]
    public void InvalidReference_IsReportedByCompilation()
    {
        var source = """
            public class EmployeePage
            {
                public UnknownEmployeeType Employee { get; set; }
            }
            """;
        var issues = CSharpSyntaxValidator.AnalyzeCompilation("Employee.aspx.cs", source, false).ToList();
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxInvalidReference);
    }

    [Fact]
    public void CodeBehindControlMissingFromMarkup_IsReported()
    {
        var source = """
            public partial class EmployeePage
            {
                public void Save()
                {
                    lblEmployeeName.Text = "Aditya";
                }
            }
            """;
        var issues = new AspxCodeBehindValidator()
            .FindMissingServerControls("Employee.aspx.cs", source, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
            .ToList();
        Assert.Contains(issues, i => i.RuleId == RuleIds.AspxRunat && i.Message.Contains("lblEmployeeName", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidCodeBehind_HasNoSyntaxErrors()
    {
        var source = """
            public partial class EmployeePage
            {
                public void SaveEmployee()
                {
                    string employeeName = "Aditya";
                }
            }
            """;
        var issues = new AspxCodeBehindValidator().ValidateSyntax("Employee.aspx.cs", source).ToList();
        Assert.Empty(issues);
    }

    private static List<Whizible.PRValidator.Models.ValidationError> Parse(string markup)
    {
        return new AspxMarkupParser(markup, "Page.aspx", Catalog(), true).Parse().Issues;
    }
}
