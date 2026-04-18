using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Controllers
{
[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IMatchingService _matchingService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public StudentController(IMatchingService matchingService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _matchingService = matchingService;
        _userManager = userManager;
        _context = context;
    }

    // Dashboard - show all proposals by this student
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        var proposals = await _context.ProjectProposals
            .Include(p => p.ResearchArea)
            .Where(p => p.SubmittedById == user!.Id)
            .ToListAsync();
        return View(proposals);
    }

    // Create - GET
    public async Task<IActionResult> Create()
    {
        var vm = new CreateProposalViewModel
        {
            ResearchAreas = await _context.ResearchAreas.Where(r => r.IsActive).ToListAsync()
        };
        return View(vm);
    }

    // Create - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProposalViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResearchAreas = await _context.ResearchAreas.Where(r => r.IsActive).ToListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var proposal = new ProjectProposal
        {
            Title = model.Title,
            Abstract = model.Abstract,
            TechnicalStack = model.TechnicalStack,
            ResearchAreaId = model.ResearchAreaId,
            SubmittedById = user!.Id,
            Status = ProjectStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.ProjectProposals.Add(proposal);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Proposal submitted successfully!";
        return RedirectToAction(nameof(Dashboard));
    }

    // Edit - GET
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var proposal = await _context.ProjectProposals
            .FirstOrDefaultAsync(p => p.Id == id && p.SubmittedById == user!.Id);

        if (proposal == null) return NotFound();
        if (proposal.Status != ProjectStatus.Pending)
        {
            TempData["Error"] = "Only pending proposals can be edited.";
            return RedirectToAction(nameof(Dashboard));
        }

        var vm = new CreateProposalViewModel
        {
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechnicalStack = proposal.TechnicalStack,
            ResearchAreaId = proposal.ResearchAreaId,
            ResearchAreas = await _context.ResearchAreas.Where(r => r.IsActive).ToListAsync()
        };
        return View(vm);
    }

    // Edit - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateProposalViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResearchAreas = await _context.ResearchAreas.Where(r => r.IsActive).ToListAsync();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var proposal = await _context.ProjectProposals
            .FirstOrDefaultAsync(p => p.Id == id && p.SubmittedById == user!.Id);

        if (proposal == null) return NotFound();
        if (proposal.Status != ProjectStatus.Pending)
        {
            TempData["Error"] = "Only pending proposals can be edited.";
            return RedirectToAction(nameof(Dashboard));
        }

        proposal.Title = model.Title;
        proposal.Abstract = model.Abstract;
        proposal.TechnicalStack = model.TechnicalStack;
        proposal.ResearchAreaId = model.ResearchAreaId;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Proposal updated successfully!";
        return RedirectToAction(nameof(Dashboard));
    }

    // Withdraw
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var proposal = await _context.ProjectProposals
            .FirstOrDefaultAsync(p => p.Id == id && p.SubmittedById == user!.Id);

        if (proposal == null) return NotFound();
        if (proposal.Status != ProjectStatus.Pending)
        {
            TempData["Error"] = "Only pending proposals can be withdrawn.";
            return RedirectToAction(nameof(Dashboard));
        }

        proposal.Status = ProjectStatus.Withdrawn;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Proposal withdrawn.";
        return RedirectToAction(nameof(Dashboard));
    }

    // Details
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var proposal = await _context.ProjectProposals
            .Include(p => p.ResearchArea)
            .FirstOrDefaultAsync(p => p.Id == id && p.SubmittedById == user!.Id);

        if (proposal == null) return NotFound();

        if (proposal.Status == ProjectStatus.Matched)
        {
            var revealed = await _matchingService.GetRevealedProposal(id, user!.Id);
            return View(revealed);
        }

        return View(proposal);
    }
}
}