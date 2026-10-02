using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Extensions;
using TaskManager.Models;
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
        private void AddStatusHistory(TaskItem task, TaskManager.Models.TaskStatus oldStatus, TaskManager.Models.TaskStatus newStatus)
        {
            var history = new TaskStatusHistory
            {
                TaskItemId = task.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow
            };

            _context.TaskStatusHistories.Add(history);
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTaskDto dto) //JSONを受け取るには、引数に[FromBody]という新しい属性を付ける
        {
            var userId = GetCurrentUserId();

            // 所属プロジェクトが本当に自分のものか確認する(NotFound)
            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            // 新しいTaskItemを作成し、DBに保存する
            var task = new TaskItem
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                DueDate = dto.DueDate.ToUtcKind(),
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // 作成したタスクをTaskDtoに詰め替えて、Ok(dto)で返す
            // 本来CreatedAtAction(...)という、201(作成成功)を表す専用のメソッドを使うのがWebAPIの定石
            var result = new TaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = dto.DueDate.ToUtcKind(),
                CreatedAt = task.CreatedAt
            };

            return Ok(result);
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateTaskDto dto)
        {
            var userId = GetCurrentUserId();

            // 所有者チェック(タスクが本当に自分のものか)
            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            // タスクの各項目を、dtoの内容で書き換える(DueDateはToUtcKind()を忘れずに)
            if (task == null)
            {
                return NotFound();
            }

            var oldStatus = task.Status;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate.ToUtcKind();

            var newStatus = task.Status;

            // ステータス変更があれば、AddStatusHistoryを呼ぶかどうかも考えてみる(任意)
            if (oldStatus != newStatus)
            {
                AddStatusHistory(task, oldStatus, newStatus);
            }

            // SaveChangesAsync
            await _context.SaveChangesAsync();

            // 更新後の内容をTaskDtoに詰め替えて、Ok(dto)で返す
            var result = new TaskDto
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

            return Ok(result);
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            // 所有者チェック(タスクが本当に自分のものか)
            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            // 削除、SaveChangesAsync
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            // 「削除に成功しました。返すデータは特にありません」という意味のステータスコード(204)
            return NoContent();
        }
    }
}