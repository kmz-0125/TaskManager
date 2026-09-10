using System.ComponentModel.DataAnnotations;

namespace TaskManager.Extensions
{
    // 拡張メソッドを定義するクラスは、必ずstatic class(静的クラス)である必要がある
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            // リフレクションという仕組み　プログラムの実行中に、クラスやプロパティの構造自体を調べたり、操作したりできる
            var field = value.GetType().GetField(value.ToString());

            // 何らかの理由でフィールド情報が取得できなかった場合Enumの元の名前(英語)をそのまま返す
            if (field == null)
            {
                return value.ToString();
            }

            // [Display(...)]という属性がついているかを調べ、ついていれば、その属性の情報を取得する
            var displayAttribute = field.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            // フィールドに[Display(Name = "未着手")]がついているか確認し、ついていれば "未着手" を返し、なければ "NotStarted"を返す
            return displayAttribute?.Name ?? value.ToString();
        }
    }
}