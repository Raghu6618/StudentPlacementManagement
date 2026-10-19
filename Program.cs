using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services;
using StudentPlacementManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Controllers & Views
builder.Services.AddControllersWithViews();

// 2. Configure Entity Framework Core Database Provider
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=StudentPlacementManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection") 
    ?? "Data Source=StudentPlacementManagement.db";

// Determine database provider once at application startup
var useSqlite = string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);
if (!useSqlite)
{
    try
    {
        // Probe SQL Server once with 1 second timeout
        var connBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sqlServerConnection) { ConnectTimeout = 1 };
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connBuilder.ConnectionString);
        testConn.Open();
        testConn.Close();
    }
    catch
    {
        // If SQL Server is unreachable or LocalDB runtime is not present, gracefully fall back to SQLite for demo portability
        useSqlite = true;
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.ConfigureWarnings(warnings => 
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

    if (useSqlite)
    {
        options.UseSqlite(sqliteConnection, sqliteOptions =>
        {
            sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
    }
    else
    {
        options.UseSqlServer(sqlServerConnection, sqlOptions =>
        {
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
    }
});

// 3. Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// 4. Configure Application Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
});

// 5. Register Business Layer Services (Dependency Injection)
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IPlacementService, PlacementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// 6. Automatic Database Migration and Seeding with Graceful Resilience
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        logger.LogInformation("Verifying and initializing database schema (Provider: {Provider})...", context.Database.ProviderName);
        if (context.Database.IsSqlServer())
        {
            context.Database.Migrate();
            logger.LogInformation("SQL Server database migration applied successfully.");
        }
        else
        {
            context.Database.EnsureCreated();
            logger.LogInformation("Local database schema verified successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration warning. Ensuring created...");
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception fallbackEx)
        {
            logger.LogError(fallbackEx, "Database initialization error.");
        }
    }

    try
    {
        logger.LogInformation("Seeding development data and default roles...");
        await DbSeeder.SeedAsync(services, app.Configuration);
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception seedEx)
    {
        logger.LogError(seedEx, "Error occurred while seeding database.");
    }
}

// 7. Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
