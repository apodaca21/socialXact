using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialX.Data;
using SocialX.Models;

namespace SocialX.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private const int FeedSize = 30; // entre 15 y 50

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Feed visible para todos
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

        // Crear post (solo logueados)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content")] Post post)
        {
            if (!ModelState.IsValid)
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(FeedSize)
                    .ToListAsync();

                return View("Index", posts);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // fuerza login
            }

            post.UserId = user.Id;
            post.CreatedAt = DateTime.UtcNow;
            post.IsEdited = false;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Editar
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

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content")] Post edited)
        {
            if (!ModelState.IsValid)
                return View(edited);

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id || !post.CanEdit)
                return Forbid();

            post.Content = edited.Content;
            post.IsEdited = true;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
