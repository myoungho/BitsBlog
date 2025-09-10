using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Infrastructure;
using BitsBlog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Enable CORS services
builder.Services.AddCors();
builder.Services.AddSingleton<Ganss.Xss.IHtmlSanitizer>(_ =>
{
    var sanitizer = new Ganss.Xss.HtmlSanitizer();
    // 허용 태그/속성 추가
    sanitizer.AllowedTags.UnionWith(new[] {
        "h1","h2","h3","h4","h5","h6",
        "p","span","pre","code","blockquote",
        "ul","ol","li","strong","em","u","a",
        "table","thead","tbody","tr","th","td",
        "img","hr","br","figure","figcaption"
    });
    sanitizer.AllowedAttributes.UnionWith(new[] {
        "href","title","target","rel",
        "src","alt","width","height","class"
    });
    sanitizer.AllowedSchemes.Add("data"); // base64 이미지 허용
    return sanitizer;
});

builder.Services.AddDbContext<BitsBlogDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "bitsblog";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Swagger UI를 루트("/")에서 실행되도록
    });
}
app.UseCors(cors =>
    cors
        .WithOrigins(
            "http://localhost:5173",
            "https://localhost:52013",
            "http://localhost:52014"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
);
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BitsBlogDbContext>();
    db.Database.Migrate();
    var customers = scope.ServiceProvider.GetRequiredService<ICustomerService>();
    await SeedAdminAsync(customers, app.Configuration);
}

app.Run();

static async Task SeedAdminAsync(ICustomerService customers, IConfiguration config)
{
    var adminEmail = (config["AdminSeed:Email"] ?? string.Empty).Trim().ToLowerInvariant();
    var adminPassword = config["AdminSeed:Password"];
    var displayName = config["AdminSeed:DisplayName"] ?? "Admin";
    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword)) return;

    await customers.EnsureAdminAsync(adminEmail, adminPassword, displayName);
}

