using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class ProjectViewModel
    {
        // 「どのプロジェクトを更新するか」を特定するために必要
        public int Id { get; set; }

        [Required(ErrorMessage = "プロジェクト名は必須です")]
        [MaxLength(100)]
        [Display(Name = "プロジェクト名")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "説明")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // 一覧画面で「このプロジェクトに何件のタスクがあるか」を表示するための項目
        public int TaskCount { get; set; }
    }
}