using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Whizible26.Domain.Entity;
using Whizible26.API.Security;
using WhizibleAPI.API.Filters;
using System.Security.Claims;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Commented & Added by Ajit L on 08/10/2025 for Authentication
//add Code By SajiU 18-Sep-24
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
//    .EnableTokenAcquisitionToCallDownstreamApi()
//    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
//    .AddInMemoryTokenCaches();



// Configure JWT settings
builder.Services.Configure<JwtSettingsEntity>(builder.Configuration.GetSection("JwtSettings"));
//Added and modified by Vishal Mane on 02/06/2026 to fix security issues
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenSessionRegistry, TokenSessionRegistry>();
//Enf of Added and modified by Vishal Mane on 02/06/2026 to fix security issues
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "CombinedScheme"; // this decides dynamically which scheme to use
    //Added and modified by Vishal Mane on 02/06/2026 to fix security issues
    options.DefaultAuthenticateScheme = "CombinedScheme";
    options.DefaultChallengeScheme = "CombinedScheme";
})
.AddPolicyScheme(
    "CombinedScheme",
    JwtBearerDefaults.AuthenticationScheme,
    options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (
                authHeader != null
                && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            )
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();

                try
                {
                    var jwt = handler.ReadJwtToken(token);
                    var issuer = jwt.Issuer;

                    // Check if it's an AzureAD token
                    if (
                        !string.IsNullOrWhiteSpace(issuer)
                        && (
                            issuer.Contains("login.microsoftonline.com")
                            || issuer.Contains("sts.windows.net")
                        )
                    )
                    {
                        return "AzureAD"; // use AzureAD validation
                    }
                }
                catch
                {
                }
            }

            return "PersonalJWT";
        };
    }
)
.AddJwtBearer(
    "PersonalJWT",
    options =>
    {
        //Added and modified by Vishal Mane on 02/06/2026 to fix security issues
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? string.Empty);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            //Added and modified by Vishal Mane on 02/06/2026 to fix security issues
            ValidAudience = audience,
            ValidAudiences = string.IsNullOrEmpty(audience) ? null : new[] { audience },
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        // VAPT #2 — Improper session management: token binding + security stamp (legacy Startup.cs OnValidateIdentity)
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    context.Fail("invalid_token");
                    return;
                }

                if (!TokenSessionValidator.IsTokenBoundToCurrentRequest(identity, context.HttpContext, builder.Configuration))
                {
                    context.Fail("invalid_token");
                    return;
                }

                if (!await TokenSessionValidator.IsSecurityStampValidAsync(
                        identity, context.HttpContext, builder.Configuration, context.HttpContext.RequestAborted)
                    .ConfigureAwait(false))
                {
                    context.Fail("PASSWORDCHANGED");
                }
            }
        };
    }
)
.AddMicrosoftIdentityWebApi(
    jwtBearerScheme: "AzureAD", // The name of the scheme
    configurationSection: builder.Configuration.GetSection("AzureAd")
)
.EnableTokenAcquisitionToCallDownstreamApi()
.AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
.AddInMemoryTokenCaches();

//End of Commented & Added by Ajit L on 08/10/2025 for Authentication

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("CombinedScheme")
        .Build();
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnRedirectToIdentityProvider = context =>
    {
        context.ProtocolMessage.Prompt = "consent";
        return Task.CompletedTask;
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
    {
        b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
// End of Addtion 

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Code added By SajiU - Register the custom authorization attribute
builder.Services.AddScoped<ValidateHeadersAttribute>();
builder.Services.AddScoped<ValidateRateLimitAttribute>();
builder.Services.AddScoped<WhizibleTeams.API.Attributes.AuthorizeAuditAttribute>();
// Global rate limiting (applies to all API calls unless excluded)
var durationMinutes = builder.Configuration.GetValue("Security:RateLimit_Duration_Minutes", 5);
var maxCount = builder.Configuration.GetValue("Security:RateLimit_Count", 3);
var enableRateLimit = builder.Configuration.GetValue("Security:RateLimit_Enabled", true);

//Commented by Vishal Mane on 05/06/2026 to comment out global rate limiting logic
//builder.Services.AddRateLimiter(options =>
//{
//    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

//    options.AddFixedWindowLimiter("Fixedwindow", limiterOptions =>
//    {
//        limiterOptions.Window = TimeSpan.FromMinutes(durationMinutes);
//        limiterOptions.PermitLimit = maxCount;
//        limiterOptions.QueueLimit = 0;
//        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//    });

//    if (enableRateLimit)
//    {
//        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//        {
//            if (HttpMethods.IsOptions(context.Request.Method))
//            {
//                return RateLimitPartition.GetNoLimiter("cors-preflight");
//            }

//            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
//            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
//            var key = ip + "|" + path;

//            return RateLimitPartition.GetFixedWindowLimiter(
//                partitionKey: key,
//                factory: _ => new FixedWindowRateLimiterOptions
//                {
//                    PermitLimit = maxCount,
//                    Window = TimeSpan.FromMinutes(durationMinutes),
//                    QueueLimit = 0,
//                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
//                });
//        });
//    }
//});
//End of Commented by Vishal Mane on 05/06/2026 to comment out global rate limiting logic

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//Code Added By SajiU 
app.UseStaticFiles();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();
if (builder.Configuration.GetValue<bool>("EnableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

//Code added By SajiU 
app.UseExceptionHandler(_ => { });

// Use CORS
app.UseCors("AllowAll");

//Code added SajiU on 18-Sep-24
//app.UseRateLimiter();

// End of Addtion 

app.UseAuthentication();
app.UseMiddleware<TokenSessionValidationMiddleware>();

// End of Addtion 


app.UseAuthorization();

app.MapControllers();

app.Run();


// Added by Aditya J. on 11-12-2025
// Purpose: Required for WebApplicationFactory in integration testing.
// ASP.NET Core's Program class is marked as partial so the test project 
// can reference it and bootstrap the real application during tests.
public partial class Program { }
