using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models.ViewModels
{
    public class MyMatchViewModel
    {
        public int MatchId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string TechnicalStack { get; set; } = string.Empty;
        public string ResearchArea { get; set; } = string.Empty;
        public MatchStatus Status { get; set; }
        public bool IsIdentityRevealed { get; set; }
        public DateTime ExpressedInterestAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // Only populated after identity reveal
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
    }
}