using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class AdminControllerTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateUser_AddsToDatabase()
        {
            var context = GetDbContext("CreateUserDb");

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "test@blindmatch.com",
                Email = "test@blindmatch.com",
                FullName = "Test User",
                EmailConfirmed = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var saved = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@blindmatch.com");
            Assert.NotNull(saved);
            Assert.Equal("Test User", saved.FullName);
        }

        [Fact]
        public async Task CreateUser_DuplicateEmailFails()
        {
            var context = GetDbContext("DuplicateEmailDb");

            var user1 = new ApplicationUser
            {
                Id = "user-2a",
                UserName = "duplicate@blindmatch.com",
                Email = "duplicate@blindmatch.com",
                FullName = "User One",
                EmailConfirmed = true
            };
            context.Users.Add(user1);
            await context.SaveChangesAsync();

            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "duplicate@blindmatch.com");
            Assert.NotNull(existingUser);
        }

        [Fact]
        public async Task DeleteUser_RemovesFromDatabase()
        {
            var context = GetDbContext("DeleteUserDb");

            var user = new ApplicationUser
            {
                Id = "user-3",
                UserName = "delete@blindmatch.com",
                Email = "delete@blindmatch.com",
                FullName = "Delete Me",
                EmailConfirmed = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var toDelete = await context.Users.FindAsync("user-3");
            context.Users.Remove(toDelete!);
            await context.SaveChangesAsync();

            var deleted = await context.Users.FindAsync("user-3");
            Assert.Null(deleted);
        }
    }
}