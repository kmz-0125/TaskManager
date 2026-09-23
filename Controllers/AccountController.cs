using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Models.ViewModels;

namespace TaskManager.Controllers
{
    public class AccountController : Controller
    {
        // ユーザーの作成、検索、パスワード管理などを担当
        private readonly UserManager<ApplicationUser> _userManager;
        // ログイン・ログアウトの処理を担当
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email, // Emailと同じ値を、UserNameにも入れている(ログイン時の識別子)
                Email = model.Email,
                Name = model.Name
            };

            // 「パスワードのハッシュ化」「Email重複チェック」「DB保存」(SaveChangesAsync相当の処理まで)
            // CreateAsyncの戻り値(result)は、「成功したか、失敗したか、失敗ならどんなエラーがあったか」を、まとめて教えてくれるオブジェクト
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            // 失敗した場合の、エラー内容はリストの中からIdentityが自動的に判定
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 対応するユーザーの検索、ハッシュ値の照合、Claim(NameIdentifier、UserNameなど)作成を自動的に実行
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, // ユーザー名
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false); // ログイン失敗時にアカウントをロックするかどうか

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "メールアドレスまたはパスワードが正しくありません");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cookieを無効可（削除）してログイン状態を解除
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}