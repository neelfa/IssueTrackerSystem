using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// register EF Core DbContext with your connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();

// Add session support for simple authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure database is created with correct schema
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Only create database if it doesn't exist (preserves existing data)
        bool databaseCreated = context.Database.EnsureCreated();
        
        if (databaseCreated)
        {
            Console.WriteLine("Database created successfully.");
            
            // Add some sample data only if database was just created
            var users = new[]
            {
                new User { Email = "admin@test.com", Password = "admin123", Role = "Admin" },
                new User { Email = "engineer@test.com", Password = "engineer123", Role = "Engineer" },
                new User { Email = "customer@test.com", Password = "customer123", Role = "Customer" }
            };
            
            context.Users.AddRange(users);
            context.SaveChanges();
            
            Console.WriteLine("Sample users created.");
        }
        else
        {
            Console.WriteLine("Database already exists, preserving existing data.");
        }
    }
    catch (Exception ex)
    {
        // Log the error - for now just write to console
        Console.WriteLine($"Database initialization error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw; // Re-throw to see the error in detail
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Enable session middleware
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
