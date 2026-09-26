using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TaskManager.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> // IdentityDbContextクラスを継承
    {
        // コンストラクタ
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /*
        DbSet<T> プロパティ
        Modelクラスに対応するテーブルを操作する窓口
        Modelクラスそれぞれに対してDbSetを用意
        */
        public DbSet<ProjectItem> ProjectItems { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskStatusHistory> TaskStatusHistories { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        // Data Annotations（[Required]など）だけでは表現しきれない、より詳細なDB設計のルールをここで指定
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enumを文字列としてDBに保存する（可読性のため）
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasConversion<string>();// HasConversion<string>()を指定することで、DBには"InProgress"のような文字列で保存されるようになる。

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<TaskStatusHistory>()
                .Property(t => t.OldStatus)
                .HasConversion<string>();

            modelBuilder.Entity<TaskStatusHistory>()
                .Property(t => t.NewStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Holiday>()
                .HasIndex(h => h.Date)
                .IsUnique();
        }
    }
}