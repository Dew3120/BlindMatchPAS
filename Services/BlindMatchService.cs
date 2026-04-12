using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Services
{
    public class BlindMatchService : IMatchingService
    {
        private readonly ApplicationDbContext _context;

        public BlindMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Returns proposals WITHOUT student identity - the core "blind" feature
        public async Task<IEnumerable<ProjectProposal>> GetAnonymousProposalsForSupervisor(string supervisorId)
        {
            // Get supervisor's preferred research areas
            var expertiseAreaIds = await _context.SupervisorExpertises
                .Where(se => se.SupervisorId == supervisorId)
                .Select(se => se.ResearchAreaId)
                .ToListAsync();

            // Get available proposals filtered by supervisor's expertise
            // CRITICAL: Do NOT include SubmittedBy navigation - this enforces anonymity
            var proposals = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
                .Where(p => expertiseAreaIds.Contains(p.ResearchAreaId))
                .Where(p => !p.Matches.Any(m => m.SupervisorId == supervisorId))
                .Select(p => new ProjectProposal
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechnicalStack = p.TechnicalStack,
                    Status = p.Status,
                    SubmittedAt = p.SubmittedAt,
                    ResearchAreaId = p.ResearchAreaId,
                    ResearchArea = p.ResearchArea,
                    // SubmittedById and SubmittedBy are intentionally excluded
                    SubmittedById = string.Empty
                })
                .ToListAsync();

            return proposals;
        }

        // Supervisor expresses interest in a project
        public async Task<bool> ExpressInterest(string supervisorId, int projectProposalId)
        {
            // Check if proposal exists and is available
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == projectProposalId);

            if (proposal == null) return false;
            if (proposal.Status == ProjectStatus.Matched || proposal.Status == ProjectStatus.Withdrawn)
                return false;

            // Check if supervisor already expressed interest
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m => m.SupervisorId == supervisorId && m.ProjectProposalId == projectProposalId);

            if (existingMatch != null) return false;

            // Create match record with "Interested" status
            var match = new Match
            {
                SupervisorId = supervisorId,
                ProjectProposalId = projectProposalId,
                Status = MatchStatus.Interested,
                ExpressedInterestAt = DateTime.UtcNow,
                IsIdentityRevealed = false
            };

            _context.Matches.Add(match);

            // Update proposal status to UnderReview
            proposal.Status = ProjectStatus.UnderReview;
            proposal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Supervisor confirms the match - triggers the IDENTITY REVEAL
        public async Task<bool> ConfirmMatch(int matchId, string supervisorId)
        {
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId && m.SupervisorId == supervisorId);

            if (match == null) return false;
            if (match.Status != MatchStatus.Interested) return false;

            // Confirm the match
            match.Status = MatchStatus.Confirmed;
            match.ConfirmedAt = DateTime.UtcNow;
            match.IsIdentityRevealed = true;

            // Update proposal status to Matched
            match.ProjectProposal.Status = ProjectStatus.Matched;
            match.ProjectProposal.UpdatedAt = DateTime.UtcNow;

            // Reject all other pending matches for this proposal
            var otherMatches = await _context.Matches
                .Where(m => m.ProjectProposalId == match.ProjectProposalId && m.Id != matchId)
                .ToListAsync();

            foreach (var otherMatch in otherMatches)
            {
                otherMatch.Status = MatchStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Supervisor rejects a match
        public async Task<bool> RejectMatch(int matchId, string supervisorId)
        {
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId && m.SupervisorId == supervisorId);

            if (match == null) return false;
            if (match.Status != MatchStatus.Interested) return false;

            match.Status = MatchStatus.Rejected;

            // If no other interested matches exist, revert proposal to Pending
            var otherActiveMatches = await _context.Matches
                .AnyAsync(m => m.ProjectProposalId == match.ProjectProposalId
                    && m.Id != matchId
                    && m.Status == MatchStatus.Interested);

            if (!otherActiveMatches)
            {
                match.ProjectProposal.Status = ProjectStatus.Pending;
                match.ProjectProposal.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Get proposal with identity ONLY if match is confirmed
        public async Task<ProjectProposal?> GetRevealedProposal(int proposalId, string userId)
        {
            var proposal = await _context.ProjectProposals
                .Include(p => p.SubmittedBy)
                .Include(p => p.ResearchArea)
                .Include(p => p.Matches)
                    .ThenInclude(m => m.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) return null;

            // Check if user is the student who submitted it
            if (proposal.SubmittedById == userId) return proposal;

            // Check if user is a supervisor with a confirmed match
            var confirmedMatch = proposal.Matches
                .FirstOrDefault(m => m.SupervisorId == userId && m.IsIdentityRevealed);

            if (confirmedMatch != null) return proposal;

            // No access - return null to enforce blind constraint
            return null;
        }

        // Get match details with identity reveal check
        public async Task<Match?> GetMatchDetails(int matchId, string userId)
        {
            var match = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.SubmittedBy)
                .Include(m => m.Supervisor)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return null;

            // Only return full details if identity is revealed
            if (match.IsIdentityRevealed &&
                (match.SupervisorId == userId || match.ProjectProposal.SubmittedById == userId))
            {
                return match;
            }

            // If not revealed, strip student identity for supervisor
            if (match.SupervisorId == userId)
            {
                match.ProjectProposal.SubmittedBy = null!;
                match.ProjectProposal.SubmittedById = string.Empty;
                return match;
            }

            return null;
        }
    }
}