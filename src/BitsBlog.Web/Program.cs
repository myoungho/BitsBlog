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

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();
