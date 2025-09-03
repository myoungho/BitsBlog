var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
builder.Services.AddHttpClient("api", client =>
{
    if (!string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        client.BaseAddress = new Uri(apiBaseUrl!);
    }
});

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();
