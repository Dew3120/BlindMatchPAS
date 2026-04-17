using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin, ModuleLeader")] 
    public class ModuleLeaderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModuleLeaderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            ViewBag.TotalProposals = await _context.ProjectProposals.CountAsync();
            ViewBag.PendingProposals = await _context.ProjectProposals
                .CountAsync(p => p.Status == ProjectStatus.Pending);
            ViewBag.MatchedProposals = await _context.ProjectProposals
                .CountAsync(p => p.Status == ProjectStatus.Matched);
            
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.TotalSupervisors = supervisors.Count;

            
            var recentMatches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.SubmittedBy)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.ResearchArea)
                .Include(m => m.Supervisor)
                .OrderByDescending(m => m.ExpressedInterestAt)
                .Take(5) // Just the top 5 for the dashboard
                .ToListAsync();

            return View(recentMatches);
        }

        
        [HttpGet]
        public async Task<IActionResult> AllMatches()
        {
            var allMatches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.SubmittedBy)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.ResearchArea)
                .Include(m => m.Supervisor)
                .OrderByDescending(m => m.ExpressedInterestAt)
                .ToListAsync();

            
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.SupervisorList = new SelectList(supervisors, "Id", "FullName");

            return View(allMatches);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignProject(int matchId, string newSupervisorId)
        {
            if (string.IsNullOrEmpty(newSupervisorId))
            {
                TempData["Error"] = "Please select a supervisor.";
                return RedirectToAction("AllMatches");
            }

            var oldMatch = await _context.Matches
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (oldMatch == null)
            {
                TempData["Error"] = "Match record not found.";
                return RedirectToAction("AllMatches");
            }

            
            oldMatch.Status = MatchStatus.Rejected;

           
            var newMatch = new Match
            {
                ProjectProposalId = oldMatch.ProjectProposalId,
                SupervisorId = newSupervisorId,
                Status = MatchStatus.Interested,
                ExpressedInterestAt = DateTime.UtcNow,
                IsIdentityRevealed = false
            };

            '
            oldMatch.ProjectProposal.Status = ProjectStatus.UnderReview;

            try
            {
                _context.Matches.Add(newMatch);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Project successfully reassigned.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Database error during reassignment: " + ex.Message;
            }

            return RedirectToAction("AllMatches");
        }
    }
}
