using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // 4. View My Matches
        public async Task<IActionResult> MyMatches()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(null); 
        }
    }
}