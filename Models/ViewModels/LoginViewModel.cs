using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "メールアドレスは必須です")]
        [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "パスワードは必須です")]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "ログイン状態を保持する")]// 「ログイン状態を保持する」というチェックボックスに対応するプロパティ
        public bool RememberMe { get; set; }
    }
}