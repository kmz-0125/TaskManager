using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Models.Dtos;
using TaskManager.Models.ViewModels;

namespace TaskManager.Controllers.Api
{
    [Authorize]
    // Web APIに便利ないくつかの挙動が自動で有効になる
    [ApiController]
    [Route("api/tasks")]
    // ControllerBaseは、ControllerからView関連の機能(View()メソッドなど)を取り除いた、Web API専用の基底クラス
    public class TasksApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksApiController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim!.Value;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId();

            var tasks = await _context.TaskItems
                .Where(t => t.ProjectItem!.UserId == userId)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var model = new TaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return Ok(model);
        }
    }
}