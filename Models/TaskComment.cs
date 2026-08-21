using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 外部キー
        [ForeignKey(nameof(TaskItem))]
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
    }
}