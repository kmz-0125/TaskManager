using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Models.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!.Value);
        }

        // GET: /Task/Index/5  (5はProjectId)
        [HttpGet("Task/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            int userId = GetCurrentUserId();

            // 指定されたプロジェクトが、本当に自分のものか確認
            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .Select(t => new TaskViewModel
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = project.Name,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .OrderBy(t => t.Status)
                .ThenByDescending(t => t.Priority)
                .ToListAsync();
            // ControllerからViewへ、一時的なデータを手軽に渡すための仕組み ViewBagは型を指定せず、なんでも自由に詰め込める入れ物
            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;

            return View(tasks);
        }

        // GET: /Task/Create/5 (5はProjectId)
        [HttpGet("Task/Create/{projectId}")]
        public async Task<IActionResult> Create(int projectId)
        {
            int userId = GetCurrentUserId();

            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            var model = new TaskViewModel
            {
                ProjectId = projectId,
                ProjectName = project.Name
            };

            return View(model);
        }

        // POST: /Task/Create/5 (5はProjectId)
        [HttpPost("Task/Create/{projectId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskViewModel model)
        {
            int userId = GetCurrentUserId();

            // 所属先のプロジェクトが、本当に自分のものか確認(なりすまし防止)
            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == model.ProjectId && p.UserId == userId);

            // 「本人のプロジェクトかどうか」という安全性の確認を、真っ先に済ませてから、入力内容の妥当性(バリデーション)を確認
            if (project == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ProjectName = project.Name;
                return View(model);
            }

            var task = new TaskItem
            {
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                Priority = model.Priority,
                DueDate = model.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { projectId = model.ProjectId });
        }
    }
}