using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services
{
    public interface IMatchingService
    {
        Task<IEnumerable<ProjectProposal>> GetAnonymousProposalsForSupervisor(string supervisorId);
        Task<bool> ExpressInterest(string supervisorId, int projectProposalId);
        Task<bool> ConfirmMatch(int matchId, string supervisorId);
        Task<bool> RejectMatch(int matchId, string supervisorId);
        Task<ProjectProposal?> GetRevealedProposal(int proposalId, string userId);
        Task<Match?> GetMatchDetails(int matchId, string userId);
    }
}