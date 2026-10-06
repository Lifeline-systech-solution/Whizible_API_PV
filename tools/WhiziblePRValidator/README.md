# Whizible PR Validator 1.0

Command-line gate for changed ASPX/ASCX pages and ASP.NET Core API controllers. It reports problems and does not modify source files.

This repository's API projects are SDK-style **.NET 8** projects opened in **Visual Studio 2022**. The related WebForms pages use a **VB.NET** code-behind (`Language="vb"`, `.aspx.vb`) and are not part of `Whizible26API.sln`. The validator therefore checks:

- ASPX/ASCX markup
- C# code-behind (`.aspx.cs`, `.ascx.cs`) with Roslyn
- VB code-behind (`.aspx.vb`, `.ascx.vb`) with the Roslyn Visual Basic parser
- API controller actions with Roslyn syntax trees

SQL validation is not included.

## Run locally

From the repository root:

```powershell
dotnet run --project tools/WhiziblePRValidator -- "D:\path\to\repo" "D:\path\to\changed-files.txt"
```

Or, after `dotnet build tools/WhiziblePRValidator -c Release`:

```powershell
tools\WhiziblePRValidator\bin\Release\net8.0\WhiziblePRValidator.exe --root "D:\path\to\repo" --changed-files changed-files.txt --format text
```

`changed-files.txt` lists paths relative to the repository root, or absolute paths:

```text
Whizible26.API/Controller/PM_AnalyticsCXO_DashboardController.cs
Pages/PM_AnalyticsCXO_Dashboard.aspx
```

Validate every ASPX page and `*Controller.cs` under the root:

```powershell
WhiziblePRValidator.exe --root "D:\path\to\repo" --all
```

JSON for a pipeline:

```powershell
WhiziblePRValidator.exe --root "D:\path\to\repo" --changed-files changed-files.txt --format json
```

Exit codes:

| Code | Meaning |
| --- | --- |
| 0 | No errors. Warnings and info do not fail the run. |
| 1 | One or more errors. |
| 2 | The validator could not start (bad arguments or an unexpected failure). |

A changed-file list from git:

```powershell
git diff --name-only origin/main...HEAD | Out-File -Encoding utf8 changed-files.txt
```

## What is checked

ASPX markup uses a stack parser. Script and style bodies are raw text, so JavaScript such as `a < b` is not treated as a tag. Rules ASPX-001 through ASPX-005 cover markup, server controls, duplicate IDs, `runat`, and directives.

`runat="server"` and `runat="Server"` are both valid. Duplicate IDs are scoped to naming containers. Repeater, GridView, and template tags each get their own ID scope. Unknown `<asp:>` types are rejected only when `System.Web.dll` can be loaded from the .NET Framework installation. `<%@ Register %>` custom controls are accepted. If System.Web is missing, the tool reports an info message and does not guess control names.

C# code-behind syntax comes from Roslyn diagnostics (ASPX-006 to ASPX-009). A missing semicolon is ASPX-007. An unresolved type is ASPX-010 when `CSharpSyntaxValidator.AnalyzeCompilation` is used. The command-line run uses syntax diagnostics for project files so missing project references are not reported as errors for every controller DTO. Full-solution semantic analysis is intentionally not a default PR failure.

API endpoints are methods Roslyn sees with `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`, `[HttpPatch]`, `[HttpHead]`, `[HttpOptions]`, or `[AcceptVerbs]`, including route arguments such as `[HttpPost("GetAnalyticsDBFilterFlagWiseDependent")]`. Commented methods and commented attributes are ignored because they are not syntax nodes.

Every endpoint must have:

- `[Authorize]` (API-001)
- `[ServiceFilter(typeof(AuthorizeAuditAttribute))]` (API-002)
- `[ServiceFilter(typeof(ValidateHeadersAttribute))]` (API-003)

`ProjectHealthDashboardController` puts those three attributes on the class. That pattern is accepted unless `acceptControllerLevelAttributes` is false or the process is started with `--method-attributes`. Attributes on the class and on the action are combined. A commented `[Authorize]` does not count.

C# syntax errors in a controller file are API-004.

## Configuration

`Configuration/validator.json` is copied beside the built executable.

```json
{
  "acceptControllerLevelAttributes": true,
  "excludeControllers": [],
  "excludeMethods": [],
  "probeSystemWeb": true
}
```

`AuthController.GetToken` does not have `[Authorize]` because it issues the token. To keep that endpoint out of the gate, set:

```json
"excludeMethods": [ "GetToken" ]
```

or `"excludeControllers": [ "AuthController" ]`.

## CI gate

The tool does not need Visual Studio to be open. A pipeline can fail the PR when the exit code is not 0.

```yaml
- script: |
    git diff --name-only origin/main...HEAD > changed-files.txt
    dotnet run --project tools/WhiziblePRValidator -c Release -- "$(Build.SourcesDirectory)" changed-files.txt
  displayName: Whizible PR validation
```

For Azure DevOps or GitHub Actions, use the same command and treat a non-zero exit code as a failed check. `--format json` can be archived as a build artifact.

## Tests

```powershell
dotnet test tools/WhiziblePRValidator.Tests/WhiziblePRValidator.Tests.csproj
```

## Not in this version

SQL validation, unit-test validation, naming rules, and automatic fixes are not implemented. The runner is structured so a later validator can append `ValidationError` items without changing ASPX or API rules.
