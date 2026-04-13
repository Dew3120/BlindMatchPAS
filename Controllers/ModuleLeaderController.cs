using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data; 
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "ModuleLeader")] 
    public class ModuleLeaderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuleLeaderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // This pulls every match from the database and includes 
            // the names of the student and the supervisor.
            var allMatches = await _context.Matches
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p.SubmittedBy)
                .Include(m => m.Supervisor)
                .ToListAsync();

            return View(allMatches);
        }
    }
}