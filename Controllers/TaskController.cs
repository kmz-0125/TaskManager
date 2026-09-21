using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        private DateTime? ToUtcKind(DateTime? dueDate)
        {
            if (dueDate == null)
            {
                return null;
            }

            DateTime changeTime;
            changeTime = DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc);
            return changeTime;

            /*
            Null条件演算子(?.)を使った、より簡潔な書き方(参考)
            private DateTime? ToUtcKind(DateTime? dateTime)
            {
                return dateTime.HasValue
                ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc)
                : null;
            }
            */
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
                DueDate = ToUtcKind(model.DueDate),
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { projectId = model.ProjectId });
        }

        // GET: /Project/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var model = new TaskViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.ProjectItem!.Name,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
            };

            return View(model);
        }

        // POST: /Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskViewModel model)
        {

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var oldStatus = task.Status;

            task.Title = model.Title;
            task.Description = model.Description;
            task.Status = model.Status;
            task.Priority = model.Priority;
            task.DueDate = ToUtcKind(model.DueDate);

            var newStatus = task.Status;

            if (oldStatus != newStatus)
            {
                AddStatusHistory(task, oldStatus, newStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = model.ProjectId });
        }

        // GET: /Project/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var model = new TaskViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.ProjectItem!.Name,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
            };

            return View(model);
        }

        // POST: /Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            /* 
             DBから取得したtask.ProjectIdを使う理由
             Controller側は既にDBからtask(本物のTaskItem)を取得しているので、わざわざフォームから送られてきた(改ざんの可能性がある)model.ProjectIdを信用する必要がない
             RemoveはSaveChangesAsync()が呼ばれるまでは、実際にはまだ削除をしない　
             また、taskという変数(C#のオブジェクト)自体は、メモリ上にまだ存在し続けているため、task.ProjectIdのように、そのプロパティにアクセスすることは問題なくできる
            */
            return RedirectToAction("Index", new { projectId = task.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            var oldStatus = task.Status;

            task.Status = task.Status switch
            {
                Models.TaskStatus.NotStarted => Models.TaskStatus.InProgress,
                Models.TaskStatus.InProgress => Models.TaskStatus.Completed,
                _ => task.Status // Completedの場合は変化なし
            };

            var newStatus = task.Status;

            AddStatusHistory(task, oldStatus, newStatus);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = task.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .Include(t => t.Comments)
                .Include(t => t.StatusHistories)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            // タスク自体の情報を、TaskViewModelに詰める
            var taskViewModel = new TaskViewModel
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.ProjectItem!.Name,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            // コメントの情報を、List<TaskCommentViewModel>に詰める
            var commentViewModels = task.Comments// Includeで既にDBに問い合わせ済みのためawaitは不要
                .Select(c => new TaskCommentViewModel
                {
                    Id = c.Id,
                    Comment = c.Comment,
                    CreatedAt = c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            // 履歴のステータスをList<TaskStatusHistoryViewModel>に詰める
            var statusHistories = task.StatusHistories
                .Select(h => new TaskStatusHistoryViewModel
                {
                    Id = h.Id,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    ChangedAt = h.ChangedAt
                })
                .OrderByDescending(h => h.ChangedAt)
                .ToList();

            // TaskViewModelとcommentViewModelsをまとめて、最終的にViewへ渡すモデルを作る
            var model = new TaskDetailsViewModel
            {
                Task = taskViewModel,
                Comments = commentViewModels,
                StatusHistories = statusHistories
            };

            // Viewへ渡す
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, TaskDetailsViewModel model)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems
                .Include(t => t.ProjectItem)
                .FirstOrDefaultAsync(t => t.Id == id && t.ProjectItem!.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.NewComment))
            {
                return RedirectToAction("Details", new { id = id });
            }

            var comment = new TaskComment
            {
                TaskItemId = id,
                Comment = model.NewComment,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }
    }
}