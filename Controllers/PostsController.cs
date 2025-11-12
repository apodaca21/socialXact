using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialX.Data;
using SocialX.Models;

namespace SocialX.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private const int FeedSize = 30;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(FeedSize)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 140)
            {
                TempData["Error"] = "El contenido debe tener entre 1 y 140 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var post = new Post
            {
                Content = content.Trim(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id || !post.CanEdit)
                return Forbid();

            return View(post);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 140)
            {
                TempData["Error"] = "El contenido debe tener entre 1 y 140 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id || !post.CanEdit)
                return Forbid();

            post.Content = content.Trim();
            post.IsEdited = true;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
