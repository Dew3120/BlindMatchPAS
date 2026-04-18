using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Dashboard with stats
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.TotalUsers = users.Count;

            var students = await _userManager.GetUsersInRoleAsync("Student");
            ViewBag.TotalStudents = students.Count;

            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.TotalSupervisors = supervisors.Count;

            var leaders = await _userManager.GetUsersInRoleAsync("ModuleLeader");
            ViewBag.TotalLeaders = leaders.Count;

            return View();
        }

        // View All Users
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<(ApplicationUser User, string Role)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add((user, roles.FirstOrDefault() ?? "No Role"));
            }

            ViewBag.UserList = userList;
            return View();
        }

        // GET: Create User
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["Success"] = $"User {model.FullName} created successfully.";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // POST: Delete User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"User {user.FullName} deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction("ManageUsers");
        }
    }
}