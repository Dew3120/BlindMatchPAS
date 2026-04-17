using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Added for Dropdown lists
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin, ModuleLeader")] // Added Admin so you can test easily
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

        // GET: ModuleLeader/Index (Dashboard with stats)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Fetch Stats for the Top Cards
            ViewBag.TotalProposals = await _context.ProjectProposals.CountAsync();
            ViewBag.PendingProposals = await _context.ProjectProposals
                .CountAsync(p => p.Status == ProjectStatus.Pending);
            ViewBag.MatchedProposals = await _context.ProjectProposals
                .CountAsync(p => p.Status == ProjectStatus.Matched);
            
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.TotalSupervisors = supervisors.Count;

            // 2. Fetch Recent Activity
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

        // GET: ModuleLeader/AllMatches
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

            // We need to provide a list of supervisors for the "Reassign" dropdown menu
            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.SupervisorList = new SelectList(supervisors, "Id", "FullName");

            return View(allMatches);
        }

        // POST: ModuleLeader/ReassignProject
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

            // 1. Cancel the old match
            oldMatch.Status = MatchStatus.Rejected;

            // 2. Create the new match for the new supervisor
            var newMatch = new Match
            {
                ProjectProposalId = oldMatch.ProjectProposalId,
                SupervisorId = newSupervisorId,
                Status = MatchStatus.Interested, // Set to Interested so the supervisor sees it
                ExpressedInterestAt = DateTime.UtcNow,
                IsIdentityRevealed = false
            };

            // 3. Ensure the project status remains 'UnderReview'
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