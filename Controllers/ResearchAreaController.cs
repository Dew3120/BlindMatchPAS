using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "ModuleLeader,Admin")]
public class ResearchAreaController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResearchAreaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.ResearchAreas.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ResearchArea area)
    {
        if (ModelState.IsValid)
        {
            _context.Add(area);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(area);
    }
}