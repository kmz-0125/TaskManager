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
        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // Claimの値は常に文字列として保存されているため変換しなおす必要がある
            // !(Null許容演算子):「このClaimは、ログイン済みである以上、絶対にnullにはならないはず」という開発者による保証を、コンパイラに伝えるための記号
            return userIdClaim!.Value;
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            // 自分が所有するプロジェクトのみ取得
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

            var userId = GetCurrentUserId();

            var project = new ProjectItem
            {
                //ユーザーがフォームで直接UserIdを指定できると、悪意を持って他人のIDを入力し、他人のプロジェクトのように見せかけて登録する、といった不正が可能になる
                // UserIdは、必ずサーバー側(Controller)で、ログイン中のユーザー情報から機械的に決定するという設計にすることで、この種の不正を構造的に防ぐ
                Name = model.Name,
                Description = model.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectItems.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // GET: /Project/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();

            /* Id一致だけでなくUserId一致も条件に含め、他人のプロジェクトを弾く
             「指定されたIDのプロジェクトが存在するか」だけでなく、「それが本当に自分のプロジェクトか」まで、1つのクエリで同時にチェック
              URLの操作によって他人のデータを不正に編集されることを防ぐ、重要な防御になっている
            */
            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            var model = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };

            return View(model);
        }

        // POST: /Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            /* URLのidとhidden inputのmodel.Idが一致するか(改ざん検知)
                GET版で画面を表示した後、POST版が呼ばれるまでの間に、時間差が存在するため「GET版で確認したから、POST版では確認しなくて良い」ということには絶対にならない
               「URLで指定されたID」と「フォームの中に隠されていたID」が、送信の過程で食い違っていないか、というデータの整合性を見ている
            */
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();


            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            project.Name = model.Name;
            project.Description = model.Description;
            // Addではなく、取得済みエンティティを直接書き換え → EF Coreが変更を自動検知
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // GET: /Project/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            var project = await _context.ProjectItems
                .Include(p => p.Tasks)// プロジェクトに関連するタスクも同時に読み込む
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            var model = new ProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TaskCount = project.Tasks.Count
            };

            return View(model);
        }

        // POST: /Project/Delete/5
        [HttpPost, ActionName("Delete")]// C#上のメソッド名はDeleteConfirmed、ASP.NET Core MVCのルーティング上はDeleteという名前として扱うための指示
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var project = await _context.ProjectItems
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            _context.ProjectItems.Remove(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}