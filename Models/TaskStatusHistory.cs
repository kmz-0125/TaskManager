using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class TaskStatusHistory
    {
        public int Id { get; set; }

        public TaskStatus OldStatus { get; set; }

        public TaskStatus NewStatus { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // 外部キー
        [ForeignKey(nameof(TaskItem))]
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
    }
}