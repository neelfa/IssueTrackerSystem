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
        Console.WriteLine("Checking database and applying migrations...");
        
        // Apply any pending migrations instead of using EnsureCreated
        context.Database.Migrate();
        
        Console.WriteLine("Database migrations applied successfully.");
        
        // Check if we need to seed test users
        var existingUsers = context.Users.Any();
        if (!existingUsers)
        {
            Console.WriteLine("No users found, creating sample data...");
            
            var users = new[]
            {
                new User { Email = "admin@test.com", Password = "admin123", Role = "Admin" },
                new User { Email = "engineer@test.com", Password = "engineer123", Role = "Engineer" },
                new User { Email = "customer@test.com", Password = "customer123", Role = "Customer" }
            };
            
            context.Users.AddRange(users);
            context.SaveChanges();
            
            Console.WriteLine("Sample users created.");
            
            // Debug: Let's verify what was actually saved
            var savedUsers = context.Users.ToList();
            foreach (var user in savedUsers)
            {
                Console.WriteLine($"User created: {user.Email} with role: '{user.Role}'");
            }
        }
        else
        {
            Console.WriteLine("Users already exist in database.");
            
            // Debug: Let's check the admin user specifically
            var adminUser = context.Users.FirstOrDefault(u => u.Email == "admin@test.com");
            if (adminUser != null)
            {
                Console.WriteLine($"Admin user found: {adminUser.Email} with role: '{adminUser.Role}'");
            }
            else
            {
                Console.WriteLine("Admin user not found in database!");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
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