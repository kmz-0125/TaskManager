using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // 外部キー
        [ForeignKey(nameof(ProjectItem))]
        public int ProjectId { get; set; }// 実際にDBのテーブルに保存される列（外部キーそのもの）
        public ProjectItem? ProjectItem { get; set; }// C#上で親のProjectItemオブジェクトを直接たどるためのナビゲーションプロパティ

        // ナビゲーションプロパティ
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<TaskStatusHistory> StatusHistories { get; set; } = new List<TaskStatusHistory>();
    }
}