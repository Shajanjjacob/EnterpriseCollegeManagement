using EnterpriseCollegeManagement.Portal.Clients;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



//service to service comm
builder.Services.AddHttpClient<IIdentityApiClient,IdentityApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayBaseUrl"]!);
    });

builder.Services.AddHttpClient<IGoogleAuthApiClient, GoogleAuthApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayBaseUrl"]!);
});

builder.Services.AddHttpClient<IGoogleAuthApiClient, GoogleAuthApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayBaseUrl"]!);
});

//cookie,sessions

builder.Services.AddDistributedMemoryCache(); //registers an in-memory cache service in ASP.NET Core's dependency injection container

//provides temporary server-side storage for the user's tokens
builder.Services.AddSession(option =>
{
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    option.IdleTimeout = TimeSpan.FromMinutes(30);

});


//creates a protected authentication cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "CollegePortal.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });


builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
