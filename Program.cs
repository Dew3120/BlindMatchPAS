using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
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

// Seed roles, users, research areas, proposals, and matches
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    // --- ROLES ---
    string[] roles = { "Admin", "ModuleLeader", "Supervisor", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // --- ADMIN ---
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

    // --- MODULE LEADER ---
    var leaderEmail = "leader@blindmatch.com";
    var leaderUser = await userManager.FindByEmailAsync(leaderEmail);
    if (leaderUser == null)
    {
        leaderUser = new ApplicationUser
        {
            UserName = leaderEmail,
            Email = leaderEmail,
            FullName = "Prof. Nimal Perera",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(leaderUser, "Leader123");
        await userManager.AddToRoleAsync(leaderUser, "ModuleLeader");
    }

    // --- SUPERVISORS ---
    var sup1Email = "supervisor@blindmatch.com";
    var sup1 = await userManager.FindByEmailAsync(sup1Email);
    if (sup1 == null)
    {
        sup1 = new ApplicationUser
        {
            UserName = sup1Email,
            Email = sup1Email,
            FullName = "Dr. Kamal Silva",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(sup1, "Super123");
        await userManager.AddToRoleAsync(sup1, "Supervisor");
    }

    var sup2Email = "supervisor2@blindmatch.com";
    var sup2 = await userManager.FindByEmailAsync(sup2Email);
    if (sup2 == null)
    {
        sup2 = new ApplicationUser
        {
            UserName = sup2Email,
            Email = sup2Email,
            FullName = "Dr. Amaya Fernando",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(sup2, "Super123");
        await userManager.AddToRoleAsync(sup2, "Supervisor");
    }

    // --- STUDENTS ---
    var stu1Email = "student@blindmatch.com";
    var stu1 = await userManager.FindByEmailAsync(stu1Email);
    if (stu1 == null)
    {
        stu1 = new ApplicationUser
        {
            UserName = stu1Email,
            Email = stu1Email,
            FullName = "Ashan Bandara",
            StudentId = "STU001",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(stu1, "Student123");
        await userManager.AddToRoleAsync(stu1, "Student");
    }

    var stu2Email = "student2@blindmatch.com";
    var stu2 = await userManager.FindByEmailAsync(stu2Email);
    if (stu2 == null)
    {
        stu2 = new ApplicationUser
        {
            UserName = stu2Email,
            Email = stu2Email,
            FullName = "Dilini Jayawardena",
            StudentId = "STU002",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(stu2, "Student123");
        await userManager.AddToRoleAsync(stu2, "Student");
    }

    var stu3Email = "student3@blindmatch.com";
    var stu3 = await userManager.FindByEmailAsync(stu3Email);
    if (stu3 == null)
    {
        stu3 = new ApplicationUser
        {
            UserName = stu3Email,
            Email = stu3Email,
            FullName = "Ruwan Wickramasinghe",
            StudentId = "STU003",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(stu3, "Student123");
        await userManager.AddToRoleAsync(stu3, "Student");
    }

    // --- RESEARCH AREAS ---
    if (!context.ResearchAreas.Any())
    {
        var areas = new List<ResearchArea>
        {
            new ResearchArea { Name = "Artificial Intelligence", Description = "Machine learning, deep learning, NLP, and computer vision", IsActive = true },
            new ResearchArea { Name = "Cybersecurity", Description = "Network security, cryptography, ethical hacking, and threat analysis", IsActive = true },
            new ResearchArea { Name = "Cloud Computing", Description = "AWS, Azure, distributed systems, and serverless architecture", IsActive = true },
            new ResearchArea { Name = "Web Development", Description = "Full-stack development, frontend frameworks, and REST APIs", IsActive = true },
            new ResearchArea { Name = "Data Science", Description = "Data analytics, big data, statistical modelling, and visualisation", IsActive = true },
            new ResearchArea { Name = "Mobile Development", Description = "iOS, Android, cross-platform apps, and mobile UX", IsActive = true },
            new ResearchArea { Name = "Internet of Things", Description = "Embedded systems, sensor networks, smart devices, and edge computing", IsActive = true },
            new ResearchArea { Name = "Software Engineering", Description = "Agile methodologies, DevOps, CI/CD, and software architecture", IsActive = true }
        };
        context.ResearchAreas.AddRange(areas);
        await context.SaveChangesAsync();
    }

    // --- SUPERVISOR EXPERTISE ---
    if (!context.SupervisorExpertises.Any())
    {
        var aiArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Artificial Intelligence");
        var cyberArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Cybersecurity");
        var cloudArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Cloud Computing");
        var webArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Web Development");
        var dataArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Data Science");
        var iotArea = context.ResearchAreas.FirstOrDefault(r => r.Name == "Internet of Things");

        var s1 = await userManager.FindByEmailAsync("supervisor@blindmatch.com");
        var s2 = await userManager.FindByEmailAsync("supervisor2@blindmatch.com");

        if (s1 != null && aiArea != null && webArea != null && dataArea != null)
        {
            context.SupervisorExpertises.AddRange(
                new SupervisorExpertise { SupervisorId = s1.Id, ResearchAreaId = aiArea.Id },
                new SupervisorExpertise { SupervisorId = s1.Id, ResearchAreaId = webArea.Id },
                new SupervisorExpertise { SupervisorId = s1.Id, ResearchAreaId = dataArea.Id }
            );
        }

        if (s2 != null && cyberArea != null && cloudArea != null && iotArea != null)
        {
            context.SupervisorExpertises.AddRange(
                new SupervisorExpertise { SupervisorId = s2.Id, ResearchAreaId = cyberArea.Id },
                new SupervisorExpertise { SupervisorId = s2.Id, ResearchAreaId = cloudArea.Id },
                new SupervisorExpertise { SupervisorId = s2.Id, ResearchAreaId = iotArea.Id }
            );
        }

        await context.SaveChangesAsync();
    }

    // --- PROJECT PROPOSALS ---
    if (!context.ProjectProposals.Any())
    {
        var st1 = await userManager.FindByEmailAsync("student@blindmatch.com");
        var st2 = await userManager.FindByEmailAsync("student2@blindmatch.com");
        var st3 = await userManager.FindByEmailAsync("student3@blindmatch.com");

        var ai = context.ResearchAreas.FirstOrDefault(r => r.Name == "Artificial Intelligence");
        var cyber = context.ResearchAreas.FirstOrDefault(r => r.Name == "Cybersecurity");
        var cloud = context.ResearchAreas.FirstOrDefault(r => r.Name == "Cloud Computing");
        var web = context.ResearchAreas.FirstOrDefault(r => r.Name == "Web Development");
        var data = context.ResearchAreas.FirstOrDefault(r => r.Name == "Data Science");
        var iot = context.ResearchAreas.FirstOrDefault(r => r.Name == "Internet of Things");

        if (st1 != null && st2 != null && st3 != null)
        {
            var proposals = new List<ProjectProposal>
            {
                new ProjectProposal
                {
                    Title = "Smart Crop Disease Detection Using Deep Learning",
                    Abstract = "This project aims to develop a mobile application that uses convolutional neural networks to identify crop diseases from leaf images captured by farmers. The system will provide real-time diagnosis and treatment recommendations, targeting paddy and tea crops commonly grown in Sri Lanka.",
                    TechnicalStack = "Python, TensorFlow, Flutter, Firebase",
                    SubmittedById = st1.Id,
                    ResearchAreaId = ai!.Id,
                    Status = ProjectStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ProjectProposal
                {
                    Title = "Real-Time Network Intrusion Detection System",
                    Abstract = "A machine learning-based intrusion detection system that monitors network traffic in real time and classifies packets as normal or malicious. The system uses ensemble methods combining Random Forest and XGBoost classifiers trained on the CICIDS2017 dataset.",
                    TechnicalStack = "Python, Scikit-learn, Wireshark, Flask",
                    SubmittedById = st1.Id,
                    ResearchAreaId = cyber!.Id,
                    Status = ProjectStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(-4)
                },
                new ProjectProposal
                {
                    Title = "Automated Lecture Timetable Generator",
                    Abstract = "An intelligent scheduling system that automatically generates conflict-free university timetables considering room availability, lecturer preferences, and student group constraints. Uses genetic algorithms to optimise for minimal gaps and balanced workloads.",
                    TechnicalStack = "ASP.NET Core, SQL Server, C#, Blazor",
                    SubmittedById = st2.Id,
                    ResearchAreaId = web!.Id,
                    Status = ProjectStatus.UnderReview,
                    SubmittedAt = DateTime.UtcNow.AddDays(-6)
                },
                new ProjectProposal
                {
                    Title = "Weather-Based Smart Irrigation Controller",
                    Abstract = "An IoT-based irrigation system that uses weather forecast data and soil moisture sensors to automatically control water supply for agricultural fields. The controller connects to a cloud dashboard for remote monitoring and manual override.",
                    TechnicalStack = "Arduino, ESP32, MQTT, React, Node.js",
                    SubmittedById = st2.Id,
                    ResearchAreaId = iot!.Id,
                    Status = ProjectStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(-3)
                },
                new ProjectProposal
                {
                    Title = "Student Performance Prediction Dashboard",
                    Abstract = "A data analytics platform that predicts student academic performance using historical grade data, attendance records, and engagement metrics. The dashboard provides early warning alerts for at-risk students and generates personalised study recommendations.",
                    TechnicalStack = "Python, Pandas, Plotly, Django, PostgreSQL",
                    SubmittedById = st3.Id,
                    ResearchAreaId = data!.Id,
                    Status = ProjectStatus.Matched,
                    SubmittedAt = DateTime.UtcNow.AddDays(-7)
                },
                new ProjectProposal
                {
                    Title = "Serverless E-Commerce Platform with Auto-Scaling",
                    Abstract = "A fully serverless e-commerce platform built on AWS Lambda and DynamoDB that automatically scales based on traffic. Features include product catalogue management, shopping cart, payment integration with Stripe, and order tracking with real-time notifications.",
                    TechnicalStack = "AWS Lambda, DynamoDB, React, Stripe API",
                    SubmittedById = st3.Id,
                    ResearchAreaId = cloud!.Id,
                    Status = ProjectStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(-2)
                }
            };
            context.ProjectProposals.AddRange(proposals);
            await context.SaveChangesAsync();
        }
    }

    // --- MATCHES ---
    if (!context.Matches.Any())
    {
        var supervisor1 = await userManager.FindByEmailAsync("supervisor@blindmatch.com");
        var supervisor2 = await userManager.FindByEmailAsync("supervisor2@blindmatch.com");

        // Confirmed match: supervisor1 matched with "Student Performance Prediction Dashboard"
        var matchedProposal = context.ProjectProposals.FirstOrDefault(p => p.Status == ProjectStatus.Matched);
        if (matchedProposal != null && supervisor1 != null)
        {
            context.Matches.Add(new Match
            {
                ProjectProposalId = matchedProposal.Id,
                SupervisorId = supervisor1.Id,
                Status = MatchStatus.Confirmed,
                ExpressedInterestAt = DateTime.UtcNow.AddDays(-5),
                ConfirmedAt = DateTime.UtcNow.AddDays(-3),
                IsIdentityRevealed = true
            });
        }

        // Pending match: supervisor2 interested in "Automated Lecture Timetable Generator"
        var underReviewProposal = context.ProjectProposals.FirstOrDefault(p => p.Status == ProjectStatus.UnderReview);
        if (underReviewProposal != null && supervisor2 != null)
        {
            context.Matches.Add(new Match
            {
                ProjectProposalId = underReviewProposal.Id,
                SupervisorId = supervisor2.Id,
                Status = MatchStatus.Interested,
                ExpressedInterestAt = DateTime.UtcNow.AddDays(-2),
                IsIdentityRevealed = false
            });
        }

        await context.SaveChangesAsync();
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