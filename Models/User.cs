using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ナビゲーションプロパティ　「このユーザーが持っているProjectItemの一覧」をC#のコード上で直接たどれるようにするためのプロパティ
        // 裏側ではEF CoreがJOINを含むSQLを自動生成
        // DB上には対応するカラムは存在せず、あくまでC#上でオブジェクト同士の関連をたどるための仕組み
        public ICollection<ProjectItem> Projects { get; set; } = new List<ProjectItem>();
    }
}