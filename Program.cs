using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using Microsoft.AspNetCore.Authentication.Cookies;// Cookie認証関連の機能を使うために必要な名前空間

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();

// AppDbContextをDIコンテナに登録
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie認証を追加(DIコンテナに登録)
// .AddAuthentication(...):「このアプリで認証機能を使います」という宣言
// CookieAuthenticationDefaults.AuthenticationScheme:「認証の方式(スキーム)は、Cookieベースにします」という指定
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>                                                           
    {
        options.LoginPath = "/Account/Login";// ログインしていないユーザーが、認証が必要なページにアクセスしようとした時、自動的にこのURLへリダイレクトする
        options.LogoutPath = "/Account/Logout";// それぞれの状況で使われるURLをあらかじめ指定
        options.AccessDeniedPath = "/Account/AccessDenied";// それぞれの状況で使われるURLをあらかじめ指定
        options.ExpireTimeSpan = TimeSpan.FromDays(7);// ログイン状態(Cookie)の有効期限
        options.SlidingExpiration = true;// スライディング有効期限」という機能。ユーザーがアプリを操作するたびに、有効期限が7日間にリセットされ続ける設定。
                                         // falseにすると、最初にログインした時点から7日間経てば、操作していても強制的にログアウト
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();// このユーザーは誰か、を判定する処理 UseAuthorization()より前に書く必要がある
app.UseAuthorization();// このユーザーはこのページにアクセスして良いか、を判定する処理

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
