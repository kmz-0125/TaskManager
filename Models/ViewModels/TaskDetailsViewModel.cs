using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class TaskDetailsViewModel
    {
        public TaskViewModel Task { get; set; } = new();

        // コメント一覧表示専用のリスト
        public List<TaskCommentViewModel> Comments { get; set; } = new();

        public List<TaskStatusHistoryViewModel> StatusHistories { get; set; } = new();

        // コメント投稿専用
        [Required(ErrorMessage = "コメントを入力してください")]
        [MaxLength(1000)]
        [Display(Name = "コメント")]
        public string NewComment { get; set; } = string.Empty;
    }

    public class TaskCommentViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}