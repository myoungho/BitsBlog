using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Infrastructure;
using BitsBlog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();
app.Run();

