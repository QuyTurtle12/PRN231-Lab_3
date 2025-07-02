using DataAccess;
using IdentityAjaxClient.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add HttpContext Accessor
builder.Services.AddHttpContextAccessor();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MyCnn");

// Register DbContext with DI
builder.Services.AddDbContext<ProductManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register HttpClient for making API calls
builder.Services.AddTransient<JwtTokenHandler>();
builder.Services.AddHttpClient("API", client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings:BaseUrl").Value;
    client.BaseAddress = new Uri(apiSettings);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<JwtTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
