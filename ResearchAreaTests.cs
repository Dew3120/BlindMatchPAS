using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class ResearchAreaTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateResearchArea_AddsToDatabase()
        {
            var context = GetDbContext("CreateAreaDb");

            var area = new ResearchArea
            {
                Name = "Machine Learning",
                Description = "ML and deep learning research",
                IsActive = true
            };

            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            var saved = await context.ResearchAreas.FirstOrDefaultAsync(r => r.Name == "Machine Learning");
            Assert.NotNull(saved);
            Assert.Equal("Machine Learning", saved.Name);
            Assert.True(saved.IsActive);
        }

        [Fact]
        public async Task EditResearchArea_UpdatesName()
        {
            var context = GetDbContext("EditAreaDb");

            var area = new ResearchArea { Name = "Old Name", IsActive = true };
            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            var existing = await context.ResearchAreas.FirstAsync();
            existing.Name = "Updated Name";
            await context.SaveChangesAsync();

            var updated = await context.ResearchAreas.FindAsync(existing.Id);
            Assert.Equal("Updated Name", updated!.Name);
        }

        [Fact]
        public async Task DeactivateResearchArea_SetsInactive()
        {
            var context = GetDbContext("DeactivateAreaDb");

            var area = new ResearchArea { Name = "Cybersecurity", IsActive = true };
            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            var existing = await context.ResearchAreas.FirstAsync();
            existing.IsActive = false;
            await context.SaveChangesAsync();

            var deactivated = await context.ResearchAreas.FindAsync(existing.Id);
            Assert.False(deactivated!.IsActive);
        }
    }
}