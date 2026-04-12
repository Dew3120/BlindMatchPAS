using System.ComponentModel.DataAnnotations.Schema;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models
{
    public class Match
    {
        public int Id { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.Interested;

        public DateTime ExpressedInterestAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        public bool IsIdentityRevealed { get; set; } = false;

        // Foreign Keys
        public int ProjectProposalId { get; set; }

        [ForeignKey("ProjectProposalId")]
        public ProjectProposal ProjectProposal { get; set; } = null!;

        public string SupervisorId { get; set; } = string.Empty;

        [ForeignKey("SupervisorId")]
        public ApplicationUser Supervisor { get; set; } = null!;
    }
}