using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Models.ViewModels;

namespace TaskManager.Controllers
{
    public class AccountController : Controller// SP.NET Core MVCのController基底クラスを継承(View()やRedirectToAction()などのメソッドが使えるようになる)
    {
        // DB操作を行うためのフィールド
        private readonly AppDbContext _context;

        // コンストラクタでappDbContextのインスタンスを受け取っているため、新たに作成する必要がなくなる(Program.csで登録したもの)
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        // 登録画面の表示
        [HttpGet]// ブラウザからページを表示しにきた(GETリクエスト)時に反応
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        // 登録処理本体
        [HttpPost]// 送信された(POSTリクエスト)時に反応
        [ValidateAntiForgeryToken]// CSRF(クロスサイトリクエストフォージェリ)攻撃を防ぐための仕組み
        // フォームから送信された値を受け取る
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 送信された内容のチェック
            if (!ModelState.IsValid)
            {
                // 内容に不備がある場合は登録画面にもどり、エラーメッセージを表示する
                return View(model);
            }

            // メールアドレスの重複チェック(DBのUsersテーブルの内容を確認)
            // メールアドレスが一致するレコードを検索し、最初の1件を取得(なければnull)
            // ※Emailには一意制約があるため、通常は0件か1件がヒットする
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // 同じメールアドレスが見つかった場合のエラー処理
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "このメールアドレスは既に使用されています");
                return View(model);
            }

            // パスワードのハッシュ化
            // BCrypt:自動的に**ソルト(ランダムな文字列)**を生成し、ハッシュ値に埋め込んでいるため、同じパスワードでも毎回異なるハッシュ値が生成される仕組みになっている
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // ユーザーの作成
            // 作成された内容はViewModelからModelへ渡される
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = passwordHash,// ハッシュ化したパスワードでDBに登録
                CreatedAt = DateTime.UtcNow
            };

            // DBにユーザーを登録するための準備
            _context.Users.Add(user);
            // DBに反映　変更をある程度溜めておき、SaveChangesが呼ばれた時にまとめて実行
            // 理由1．パフォーマンス：DBとの通信回数を減らし、処理を効率化する(notストアドプロシージャ)
            // 理由2.データ整合性：複数の変更を「全部成功」か「全部失敗」かのどちらかに保証し、中途半端な状態を防ぐ
            await _context.SaveChangesAsync();
            // RedirectToActionはブラウザに対して別のURLへ改めてアクセスし直すことを指示している
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        // ログイン画面表示
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        // 認証処理
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Verify()
            // user.PasswordHash(DBに保存されているハッシュ値)の中に埋め込まれているソルト情報を取り出す
            // そのソルトを使って、今回入力されたmodel.Password(生のパスワード)を同じ手順で再度ハッシュ化する
            // 計算し直した結果が、DBに保存されているハッシュ値と完全に一致するかどうかを比較する
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                // 引数が空文字の理由：どの項目が問題なのかわからないようにしている→不正ログインのための情報を与えないようにするため
                ModelState.AddModelError(string.Empty, "メールアドレスまたはパスワードが正しくありません");
                return View(model);
            }

            // ログイン成功 →Claimを作成
            // Claim(クレーム)とは、「このユーザーに関する情報の断片」
            // List化して複数の情報をひとまとめにする
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),// Identifier:“ユーザーを一意に識別するID”という種類の情報です　という型(種類)の指定
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // 複数のClaim(情報の断片)を1つにまとめて、「この人物である」という身元(Identity)を表現するオブジェクト
            // CookieAuthenticationDefaults.AuthenticationScheme:「この身元情報は、Cookie認証の仕組みによって検証されたものですよ」という認証方式の紐付け
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 認証に関する追加のオプション設定
            var authProperties = new AuthenticationProperties
            {
                // ログイン状態の保持
                IsPersistent = model.RememberMe,
            };

            // 処理中のリクエストに関する情報を扱っている
            // 第一引数:Cookie認証の方式で」という指定
            // 第二引数:認証済みの主体(この処理を行っている本人)」を表すオブジェクト
            // 第三引数:「保持するかどうか」などの追加設定
            // 認証済みの本人であるという確認とログインユーザーとして扱うための処理を行っている
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ログイン成功後のアクションへリダイレクト
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cookieを無効可（削除）してログイン状態を解除
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        // ログイン中に権限が足りない操作をしたとき対応
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}