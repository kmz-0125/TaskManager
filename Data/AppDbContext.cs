using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class AppDbContext : DbContext// EF CoreのDbContextクラスを継承
    {
        // コンストラクタ
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet<T> プロパティ
        // Modelクラスに対応するテーブルを操作する窓口
        // 5つのModelクラスそれぞれに対してDbSetを用意
        public DbSet<User> Users { get; set; }
        public DbSet<ProjectItem> ProjectItems { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskStatusHistory> TaskStatusHistories { get; set; }

        // Data Annotations（[Required]など）だけでは表現しきれない、より詳細なDB設計のルールをここで指定
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Userのメールアドレスは一意にする
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

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
        }
    }
}