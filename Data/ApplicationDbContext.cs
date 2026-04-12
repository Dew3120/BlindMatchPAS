using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectProposal> ProjectProposals { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<SupervisorExpertise> SupervisorExpertises { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ProjectProposal -> SubmittedBy (Student)
            builder.Entity<ProjectProposal>()
                .HasOne(p => p.SubmittedBy)
                .WithMany(u => u.ProjectProposals)
                .HasForeignKey(p => p.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectProposal -> ResearchArea
            builder.Entity<ProjectProposal>()
                .HasOne(p => p.ResearchArea)
                .WithMany(r => r.ProjectProposals)
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupervisorExpertise -> Supervisor
            builder.Entity<SupervisorExpertise>()
                .HasOne(se => se.Supervisor)
                .WithMany(u => u.SupervisorExpertises)
                .HasForeignKey(se => se.SupervisorId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupervisorExpertise -> ResearchArea
            builder.Entity<SupervisorExpertise>()
                .HasOne(se => se.ResearchArea)
                .WithMany(r => r.SupervisorExpertises)
                .HasForeignKey(se => se.ResearchAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: one supervisor can't have same area twice
            builder.Entity<SupervisorExpertise>()
                .HasIndex(se => new { se.SupervisorId, se.ResearchAreaId })
                .IsUnique();

            // Match -> ProjectProposal
            builder.Entity<Match>()
                .HasOne(m => m.ProjectProposal)
                .WithMany(p => p.Matches)
                .HasForeignKey(m => m.ProjectProposalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match -> Supervisor
            builder.Entity<Match>()
                .HasOne(m => m.Supervisor)
                .WithMany(u => u.SupervisorMatches)
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one supervisor can't match same project twice
            builder.Entity<Match>()
                .HasIndex(m => new { m.SupervisorId, m.ProjectProposalId })
                .IsUnique();
        }
    }
}