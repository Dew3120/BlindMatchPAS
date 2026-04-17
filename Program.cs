using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure login path
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// Register Blind Matching Service with Dependency Injection
builder.Services.AddScoped<IMatchingService, BlindMatchService>();

var app = builder.Build();

// Seed roles and test users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "ModuleLeader", "Supervisor", "Student" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create default Admin user
    var adminEmail = "admin@blindmatch.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Create test Supervisor
    var supervisorEmail = "supervisor@blindmatch.com";
    var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);
    if (supervisorUser == null)
    {
        supervisorUser = new ApplicationUser
        {
            UserName = supervisorEmail,
            Email = supervisorEmail,
            FullName = "Dr. Test Supervisor",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(supervisorUser, "Super123");
        await userManager.AddToRoleAsync(supervisorUser, "Supervisor");
    }

    // Create test Student
    var studentEmail = "student@blindmatch.com";
    var studentUser = await userManager.FindByEmailAsync(studentEmail);
    if (studentUser == null)
    {
        studentUser = new ApplicationUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            FullName = "Test Student",
            StudentId = "STU001",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(studentUser, "Student123");
        await userManager.AddToRoleAsync(studentUser, "Student");
    }

    // Create test Module Leader
    var leaderEmail = "leader@blindmatch.com";
    var leaderUser = await userManager.FindByEmailAsync(leaderEmail);
    if (leaderUser == null)
    {
        leaderUser = new ApplicationUser
        {
            UserName = leaderEmail,
            Email = leaderEmail,
            FullName = "Prof. Module Leader",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(leaderUser, "Leader123");
        await userManager.AddToRoleAsync(leaderUser, "ModuleLeader");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();