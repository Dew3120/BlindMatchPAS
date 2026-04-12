using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models
{
    public class ProjectProposal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string TechnicalStack { get; set; } = string.Empty;

        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [Required]
        public string SubmittedById { get; set; } = string.Empty;

        [ForeignKey("SubmittedById")]
        public ApplicationUser SubmittedBy { get; set; } = null!;

        public int ResearchAreaId { get; set; }

        [ForeignKey("ResearchAreaId")]
        public ResearchArea ResearchArea { get; set; } = null!;

        // Navigation
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
