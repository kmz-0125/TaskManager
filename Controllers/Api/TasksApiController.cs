using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Models.Dtos;

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
    }
}