using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ReCenterHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<Notifier>();
builder.Services.AddScoped<ConcreteObserver>();

builder.Services.AddHttpContextAccessor();

var apiBase = builder.Configuration["ApiSettings:BaseUrl"]!.TrimEnd('/') + "/";

builder.Services.AddHttpClient<ApiAuthService>(client => client.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<IndividualBookingService>(client => client.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<WorkshopBookingService>(client => client.BaseAddress = new Uri(apiBase));
builder.Services.AddHttpClient<NoticeService>(client => client.BaseAddress = new Uri(apiBase));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program { } 