using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialX.Data;
using SocialX.Models;

namespace SocialX.Controllers
{
    [Authorize] // Todas las operaciones requieren login
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private const int FeedSize = 30; // Entre 15 y 50

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Posts/Index (Home)
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(FeedSize)
                .ToListAsync();

            return View(posts);
        }

        // POST: Crear publicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 140)
            {
                TempData["PostError"] = "El mensaje debe tener entre 1 y 140 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge(); // fuerza login

            var post = new Post
            {
                Content = content.Trim(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Editar publicación
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (post.UserId != userId) return Forbid();

            if (!CanEdit(post))
            {
                TempData["PostError"] = "Sólo puedes editar publicaciones con menos de 5 minutos.";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // POST: Guardar edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (post.UserId != userId) return Forbid();

            if (!CanEdit(post))
            {
                TempData["PostError"] = "Sólo puedes editar publicaciones con menos de 5 minutos.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(content) || content.Length > 140)
            {
                ModelState.AddModelError("Content", "El mensaje debe tener entre 1 y 140 caracteres.");
                return View(post);
            }

            post.Content = content.Trim();
            post.IsEdited = true;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Regla de 5 minutos
        private bool CanEdit(Post post)
        {
            var diff = DateTime.UtcNow - post.CreatedAt;
            return diff.TotalMinutes < 5;
        }
    }
}
