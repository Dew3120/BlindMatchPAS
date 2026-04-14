using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using System.Threading.Tasks;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor")]  
    public class SupervisorController : Controller
    {
        private readonly IMatchingService _matchingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(IMatchingService matchingService, UserManager<ApplicationUser> userManager)
        {
            _matchingService = matchingService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var proposals = await _matchingService.GetAnonymousProposalsForSupervisor(user!.Id);
            
            return View(proposals);
        }
    }
}