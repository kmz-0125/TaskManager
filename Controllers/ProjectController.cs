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
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        // ログイン中のユーザーIDを取得するヘルパーメソッド
        private int GetCurrentUserId()
        {
            // FindFirstはどんな種類のClaimでも汎用的に取り出せるメソッド
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // Claimの値は常に文字列として保存されているため変換しなおす必要がある
            // !(Null許容演算子):「このClaimは、ログイン済みである以上、絶対にnullにはならないはず」という開発者による保証を、コンパイラに伝えるための記号
            return int.Parse(userIdClaim!.Value);
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var projects = await _context.ProjectItems
                .Where(p => p.UserId == userId)// UserIdが今ログインしているユーザーのIDと一致するものだけに絞り込む
                .Select(p => new ProjectViewModel //絞り込んだProjectItem(Model)を、1件ずつProjectViewModel(ViewModel)に変換
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    TaskCount = p.Tasks.Count
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();// ここまで組み立てたクエリを、実際にDBに対して実行し、結果をリストとして取得する

            return View(projects);
        }

        // GET: /Project/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            // 送信された内容のチェック　不備がある場合は登録画面にもどり、エラーメッセージを表示する
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = GetCurrentUserId();

            var project = new ProjectItem
            {
                Name = model.Name,
                Description = model.Description,
                // ユーザーがフォームで直接UserIdを指定できると、悪意を持って他人のIDを入力し、他人のプロジェクトのように見せかけて登録する、といった不正が可能になる
                // UserIdは、必ずサーバー側(Controller)で、ログイン中のユーザー情報から機械的に決定するという設計にすることで、この種の不正を構造的に防ぐ
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectItems.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}