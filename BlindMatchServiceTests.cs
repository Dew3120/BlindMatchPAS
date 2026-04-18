using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Services;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class BlindMatchServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new ApplicationDbContext(options);
            return context;
        }

        private void SeedTestData(ApplicationDbContext context, string supervisorId, string studentId)
        {
            var area = new ResearchArea { Id = 1, Name = "AI", IsActive = true };
            context.ResearchAreas.Add(area);

            context.SupervisorExpertises.Add(new SupervisorExpertise
            {
                SupervisorId = supervisorId,
                ResearchAreaId = 1
            });

            context.ProjectProposals.Add(new ProjectProposal
            {
                Id = 1,
                Title = "Test Proposal",
                Abstract = "Test abstract",
                TechnicalStack = "C#",
                SubmittedById = studentId,
                ResearchAreaId = 1,
                Status = ProjectStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAnonymousProposals_HidesStudentIdentity()
        {
            var context = GetDbContext("HidesIdentityDb");
            var supervisorId = "sup-1";
            var studentId = "stu-1";
            SeedTestData(context, supervisorId, studentId);

            var service = new BlindMatchService(context);
            var proposals = await service.GetAnonymousProposalsForSupervisor(supervisorId);

            foreach (var p in proposals)
            {
                Assert.Equal(string.Empty, p.SubmittedById);
                Assert.Null(p.SubmittedBy);
            }
        }

        [Fact]
        public async Task ExpressInterest_CreatesMatchRecord()
        {
            var context = GetDbContext("CreatesMatchDb");
            var supervisorId = "sup-2";
            var studentId = "stu-2";
            SeedTestData(context, supervisorId, studentId);

            var service = new BlindMatchService(context);
            var result = await service.ExpressInterest(supervisorId, 1);

            Assert.True(result);
            var match = await context.Matches.FirstOrDefaultAsync();
            Assert.NotNull(match);
            Assert.Equal(MatchStatus.Interested, match.Status);
            Assert.Equal(supervisorId, match.SupervisorId);
        }

        [Fact]
        public async Task ExpressInterest_FailsIfAlreadyInterested()
        {
            var context = GetDbContext("AlreadyInterestedDb");
            var supervisorId = "sup-3";
            var studentId = "stu-3";
            SeedTestData(context, supervisorId, studentId);

            var service = new BlindMatchService(context);
            await service.ExpressInterest(supervisorId, 1);
            var secondAttempt = await service.ExpressInterest(supervisorId, 1);

            Assert.False(secondAttempt);
        }

        [Fact]
        public async Task ConfirmMatch_RevealsIdentity()
        {
            var context = GetDbContext("RevealsIdentityDb");
            var supervisorId = "sup-4";
            var studentId = "stu-4";
            SeedTestData(context, supervisorId, studentId);

            var service = new BlindMatchService(context);
            await service.ExpressInterest(supervisorId, 1);
            var match = await context.Matches.FirstAsync();

            var result = await service.ConfirmMatch(match.Id, supervisorId);

            Assert.True(result);
            var confirmedMatch = await context.Matches.FindAsync(match.Id);
            Assert.True(confirmedMatch!.IsIdentityRevealed);
            Assert.Equal(MatchStatus.Confirmed, confirmedMatch.Status);
        }

        [Fact]
        public async Task ConfirmMatch_RejectsOtherMatches()
        {
            var context = GetDbContext("RejectsOthersDb");
            var sup1 = "sup-5a";
            var sup2 = "sup-5b";
            var studentId = "stu-5";
            SeedTestData(context, sup1, studentId);

            context.SupervisorExpertises.Add(new SupervisorExpertise
            {
                SupervisorId = sup2,
                ResearchAreaId = 1
            });
            context.SaveChanges();

            var service = new BlindMatchService(context);
            await service.ExpressInterest(sup1, 1);
            await service.ExpressInterest(sup2, 1);

            var match1 = await context.Matches.FirstAsync(m => m.SupervisorId == sup1);
            await service.ConfirmMatch(match1.Id, sup1);

            var match2 = await context.Matches.FirstAsync(m => m.SupervisorId == sup2);
            Assert.Equal(MatchStatus.Rejected, match2.Status);
        }

        [Fact]
        public async Task RejectMatch_SetsStatusToRejected()
        {
            var context = GetDbContext("RejectMatchDb");
            var supervisorId = "sup-6";
            var studentId = "stu-6";
            SeedTestData(context, supervisorId, studentId);

            var service = new BlindMatchService(context);
            await service.ExpressInterest(supervisorId, 1);
            var match = await context.Matches.FirstAsync();

            var result = await service.RejectMatch(match.Id, supervisorId);

            Assert.True(result);
            var rejectedMatch = await context.Matches.FindAsync(match.Id);
            Assert.Equal(MatchStatus.Rejected, rejectedMatch!.Status);
        }
    }
}