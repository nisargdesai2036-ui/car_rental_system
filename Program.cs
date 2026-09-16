using dotenv.net;
using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Services;

// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Resolve Supabase Connection String from Environment Variables
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var host = Environment.GetEnvironmentVariable("SUPABASE_HOST");
    var port = Environment.GetEnvironmentVariable("SUPABASE_PORT") ?? "5432";
    var db = Environment.GetEnvironmentVariable("SUPABASE_DATABASE") ?? "postgres";
    var user = Environment.GetEnvironmentVariable("SUPABASE_USER") ?? "postgres";
    var pass = Environment.GetEnvironmentVariable("SUPABASE_PASSWORD");
    var ssl = Environment.GetEnvironmentVariable("SUPABASE_SSL_MODE") ?? "Require";
    var trustCert = Environment.GetEnvironmentVariable("SUPABASE_TRUST_SERVER_CERT") ?? "true";

    if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(pass))
    {
        connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode={ssl};Trust Server Certificate={trustCert};";
    }
}

// Register Supabase PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register In-Memory DataStore (Singleton for Phase 1)
builder.Services.AddSingleton<IDataStore, InMemoryDataStore>();

// Register Application Business Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed initial data into Supabase if empty
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

app.Run();
