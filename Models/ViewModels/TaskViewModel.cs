using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        // 一覧・詳細画面でプロジェクト名を表示するための項目(入力はさせない)
        public string ProjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "タスク名は必須です")]
        [MaxLength(200)]
        [Display(Name = "タスク名")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "詳細")]
        public string? Description { get; set; }

        [Display(Name = "ステータス")]
        public Models.TaskStatus Status { get; set; }

        [Display(Name = "優先度")]
        public Models.TaskPriority Priority { get; set; }

        [Display(Name = "期限")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}