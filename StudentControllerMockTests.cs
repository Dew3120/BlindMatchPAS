using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Services;
using BlindMatchPAS.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class StudentControllerMockTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task Dashboard_ReturnsOnlyCurrentStudentProposals()
        {
            var context = GetDbContext("MockDashboardDb");
            var mockUserManager = GetMockUserManager();
            var mockMatchingService = new Mock<IMatchingService>();

            var student1 = new ApplicationUser { Id = "student-1", UserName = "s1@test.com", FullName = "Student One" };

            context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
            context.ProjectProposals.Add(new ProjectProposal
            {
                Id = 1, Title = "Student 1 Proposal", Abstract = "Test abstract",
                TechnicalStack = "C#", SubmittedById = "student-1",
                ResearchAreaId = 1, Status = ProjectStatus.Pending, SubmittedAt = DateTime.UtcNow
            });
            context.ProjectProposals.Add(new ProjectProposal
            {
                Id = 2, Title = "Student 2 Proposal", Abstract = "Test abstract",
                TechnicalStack = "Java", SubmittedById = "student-2",
                ResearchAreaId = 1, Status = ProjectStatus.Pending, SubmittedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(student1);

            var controller = new StudentController(mockMatchingService.Object, mockUserManager.Object, context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Dashboard() as ViewResult;
            var proposals = result?.Model as List<ProjectProposal>;

            Assert.NotNull(proposals);
            Assert.Single(proposals);
            Assert.Equal("Student 1 Proposal", proposals[0].Title);
        }

        [Fact]
        public async Task Dashboard_ReturnsEmptyListForStudentWithNoProposals()
        {
            var context = GetDbContext("MockEmptyDb");
            var mockUserManager = GetMockUserManager();
            var mockMatchingService = new Mock<IMatchingService>();

            var student = new ApplicationUser { Id = "no-proposals", UserName = "empty@test.com", FullName = "Empty Student" };

            context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
            await context.SaveChangesAsync();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(student);

            var controller = new StudentController(mockMatchingService.Object, mockUserManager.Object, context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Dashboard() as ViewResult;
            var proposals = result?.Model as List<ProjectProposal>;

            Assert.NotNull(proposals);
            Assert.Empty(proposals);
        }

        [Fact]
        public async Task Create_GET_ReturnsViewWithResearchAreas()
        {
            var context = GetDbContext("MockCreateDb");
            var mockUserManager = GetMockUserManager();
            var mockMatchingService = new Mock<IMatchingService>();

            context.ResearchAreas.Add(new ResearchArea { Id = 1, Name = "AI", IsActive = true });
            context.ResearchAreas.Add(new ResearchArea { Id = 2, Name = "Web Dev", IsActive = true });
            await context.SaveChangesAsync();

            var controller = new StudentController(mockMatchingService.Object, mockUserManager.Object, context);

            var result = await controller.Create() as ViewResult;

            Assert.NotNull(result);
        }
    }
}