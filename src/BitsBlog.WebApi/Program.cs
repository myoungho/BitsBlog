using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Infrastructure;
using BitsBlog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = "Server=(local);Database=BitsBlog;User Id=sa;Password=123456;TrustServerCertificate=True";
builder.Services.AddDbContext<BitsBlogDbContext>(opt =>
    opt.UseSqlServer(connectionString));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BitsBlogDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
