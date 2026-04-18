using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models.ViewModels
{
    public class CreateProposalViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Abstract is required")]
        [StringLength(2000)]
        public string Abstract { get; set; } = string.Empty;

        [Required(ErrorMessage = "Technical Stack is required")]
        [StringLength(500)]
        public string TechnicalStack { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Research Area")]
        public int ResearchAreaId { get; set; }

        public List<ResearchArea> ResearchAreas { get; set; } = new();
    }
}