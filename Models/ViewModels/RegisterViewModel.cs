using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "名前は必須です")]
        [MaxLength(50)]
        [Display(Name = "名前")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "メールアドレスは必須です")]// ErrorMessageで、検証に失敗した時にユーザーへ表示するメッセージを指定できる
        [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]// 入力された文字列が「メールアドレスの形式」になっているかを検証する属性
        [Display(Name = "メールアドレス")]// 画面(View)上でこの項目のラベルとして表示する文字列を指定　Viewを作る際、この設定を使って自動的にラベルが生成される
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "パスワードは必須です")]
        [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください")]
        [DataType(DataType.Password)]// この項目が「パスワード」であることを示す指定　後でViewを作成する際、入力欄が自動的に**マスク表示になる
        [Display(Name = "パスワード")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認用パスワードは必須です")]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード(確認)")]
        [Compare("Password", ErrorMessage = "パスワードが一致しません")]// ConfirmPassword(確認用パスワード)が、Passwordプロパティの値と一致しているかを検証
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}