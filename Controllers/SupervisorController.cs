using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using System.Threading.Tasks;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor,Admin")]  
    public class SupervisorController : Controller
    {
        private readonly IMatchingService _matchingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(IMatchingService matchingService, UserManager<ApplicationUser> userManager)
        {
            _matchingService = matchingService;
            _userManager = userManager;
        }

        // 1. The Main Dashboard View
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var proposals = await _matchingService.GetAnonymousProposalsForSupervisor(user!.Id);
            
            return View(proposals);
        }

        // 2. Browse Anonymous Proposals
        public async Task<IActionResult> BrowseProposals()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var proposals = await _matchingService.GetAnonymousProposalsForSupervisor(user!.Id);
            
            return View(proposals);
        }

        // 3. Express Interest (POST Action)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Call the service to handle the business logic
            var success = await _matchingService.ExpressInterest(user!.Id, proposalId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Interest expressed! Waiting for final confirmation.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not process request. You may have already expressed interest.";
            }
    
            return RedirectToAction(nameof(BrowseProposals));
        }

=======
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatchingService _matchingService;

        public SupervisorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMatchingService matchingService)
        {
            _context = context;
            _userManager = userManager;
            _matchingService = matchingService;
        }

        // GET: Supervisor/MyExpertise
        [HttpGet]
        public async Task<IActionResult> MyExpertise()
        {
            var user = await _userManager.GetUserAsync(User);
            var allAreas = await _context.ResearchAreas
                .Where(r => r.IsActive)
                .ToListAsync();

            var myExpertiseIds = await _context.SupervisorExpertises
                .Where(se => se.SupervisorId == user!.Id)
                .Select(se => se.ResearchAreaId)
                .ToListAsync();

            var viewModel = allAreas.Select(a => new ExpertiseViewModel
            {
                ResearchAreaId = a.Id,
                Name = a.Name,
                IsSelected = myExpertiseIds.Contains(a.Id)
            }).ToList();

            return View(viewModel);
        }

        // POST: Supervisor/MyExpertise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyExpertise(List<int> selectedAreas)
        {
            var user = await _userManager.GetUserAsync(User);

            var existing = await _context.SupervisorExpertises
                .Where(se => se.SupervisorId == user!.Id)
                .ToListAsync();
            _context.SupervisorExpertises.RemoveRange(existing);

            if (selectedAreas != null)
            {
                foreach (var areaId in selectedAreas)
                {
                    _context.SupervisorExpertises.Add(new SupervisorExpertise
                    {
                        SupervisorId = user!.Id,
                        ResearchAreaId = areaId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Expertise areas updated successfully.";
            return RedirectToAction("MyExpertise");
        }

        // GET: Supervisor/MyMatches
        [HttpGet]
        public async Task<IActionResult> MyMatches()
        {
            var user = await _userManager.GetUserAsync(User);

            var matches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.ResearchArea)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.SubmittedBy)
                .Where(m => m.SupervisorId == user!.Id)
                .OrderByDescending(m => m.ExpressedInterestAt)
                .ToListAsync();

            var viewModel = matches.Select(m => new MyMatchViewModel
            {
                MatchId = m.Id,
                ProjectTitle = m.ProjectProposal.Title,
                Abstract = m.ProjectProposal.Abstract,
                TechnicalStack = m.ProjectProposal.TechnicalStack,
                ResearchArea = m.ProjectProposal.ResearchArea.Name,
                Status = m.Status,
                IsIdentityRevealed = m.IsIdentityRevealed,
                ExpressedInterestAt = m.ExpressedInterestAt,
                ConfirmedAt = m.ConfirmedAt,
                StudentName = m.IsIdentityRevealed ? m.ProjectProposal.SubmittedBy.FullName : null,
                StudentEmail = m.IsIdentityRevealed ? m.ProjectProposal.SubmittedBy.Email : null
            }).ToList();

            return View(viewModel);
        }

        // POST: Supervisor/ConfirmMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _matchingService.ConfirmMatch(matchId, user!.Id);

            if (result)
                TempData["Success"] = "Match confirmed! Student identity has been revealed.";
            else
                TempData["Error"] = "Unable to confirm match.";

            return RedirectToAction("MyMatches");
        }

        // POST: Supervisor/RejectMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _matchingService.RejectMatch(matchId, user!.Id);

            if (result)
                TempData["Success"] = "Match rejected.";
            else
                TempData["Error"] = "Unable to reject match.";

            return RedirectToAction("MyMatches");
        }
>>>>>>> main
    }
}