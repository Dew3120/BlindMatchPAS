using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "ModuleLeader,Admin")]
    public class ResearchAreaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResearchAreaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ResearchArea
        public async Task<IActionResult> Index()
        {
            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        // GET: ResearchArea/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ResearchArea/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResearchArea area)
        {
            if (ModelState.IsValid)
            {
                area.IsActive = true;
                _context.ResearchAreas.Add(area);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research area created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        // GET: ResearchArea/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return NotFound();
            return View(area);
        }

        // POST: ResearchArea/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResearchArea area)
        {
            if (id != area.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.ResearchAreas.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Name = area.Name;
                existing.Description = area.Description;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Research area updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        // POST: ResearchArea/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return NotFound();

            area.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Research area deactivated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: ResearchArea/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return NotFound();

            area.IsActive = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Research area activated.";
            return RedirectToAction(nameof(Index));
        }
    }
}