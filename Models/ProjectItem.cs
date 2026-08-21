using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class ProjectItem
    {
        public int Id { get; set; }

        // ［］属性：データ検証ルールを表す注釈
        // DB設計への反映：EF Coreがこの情報を読み取り、DBのテーブル作成時に「NOT NULL制約」や「VARCHAR(50)」のような列定義を自動生成する
        // アプリ側のバリデーション：フォーム入力時に「この項目は必須です」といったエラーチェックにも使われる
        // DB設計とアプリのバリデーションルールを一箇所で管理できる

        // Required：この項目は必須（ＮＵＬＬ不可）
        [Required]
        // MaxLength(n)：最大５０文字まで
        [MaxLength(100)] 
        public string Name { get; set; } = string.Empty;

        // 説明・内容
        [MaxLength(500)]
        public string? Description { get; set; }

        // デフォルト値として現在時刻を自動セット
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 外部キー
        [ForeignKey(nameof(User))]// 「UserIdはUserプロパティに対応する外部キーですよ」とEF Coreに明示的に伝えている
        public int UserId { get; set; }// 実際にDBのテーブルに保存される列（外部キーそのもの）
        public User? User { get; set; }// C#上で親のUserオブジェクトを直接たどるためのナビゲーションプロパティ
                                       // EF Coreは指定しない限り自動的に関連データを読み込まない（遅延読み込みしない設定がデフォルト）ため、
                                       // 実際にコード上で意図的に読み込む処理（Includeなど）をしない限り、この値はNULLのまま。

        // ナビゲーションプロパティ
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}