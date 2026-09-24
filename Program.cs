using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using Microsoft.AspNetCore.Authentication.Cookies;// Cookie認証関連の機能を使うために必要な名前空間
using Microsoft.AspNetCore.Identity;
using TaskManager.Models;
using TaskManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();

// AppDbContextをDIコンテナに登録
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // パスワードの要件を設定(今回は当初の仕様に合わせて、8文字以上のみを必須とする)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
    // Identityが管理するユーザー情報などをAppDbContext(PostgreSQL)に保存してくださいという指定
    .AddEntityFrameworkStores<AppDbContext>()
    // Identityが提供するトークン(一時的な認証コード)関連の機能を有効にする設定
    .AddDefaultTokenProviders()
    // エラーメッセージを日本語に変更
    .AddErrorDescriber<JapaneseIdentityErrorDescriber>();

// Cookie自体の細かい挙動(ログインパス、有効期限など)を設定
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
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