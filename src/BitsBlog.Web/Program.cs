using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BitsBlog.Web.Services.CookieAuthHandler>();
builder.Services.AddHttpClient("api", client =>
{
    if (!string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        client.BaseAddress = new Uri(apiBaseUrl!);
    }
}).AddHttpMessageHandler<BitsBlog.Web.Services.CookieAuthHandler>();

// Authentication/Authorization for MVC using JWT in cookie ("jwt")
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "bitsblog";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev only
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token) && ctx.Request.Cookies.TryGetValue("jwt", out var token))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                // For MVC pages, redirect to login instead of 401 JSON
                if (!ctx.Response.HasStarted && !ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.HandleResponse();
                    var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
                    var redirect = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                    ctx.Response.Redirect(redirect);
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Enable areas and default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();
app.Run();
