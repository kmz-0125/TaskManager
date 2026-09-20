namespace TaskManager.Extensions
{
    public static class DateTimeExtensions
    {
        // 2つ目の引数:書式を指定する文字列
        public static string ToJstString(this DateTime dateTime, string format)
        {
            // 東京標準時刻を取得
            var jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            // 引数で受け取ったdateTimeを東京標準時刻のタイムゾーンに変換
            var jstDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, jstZone);

            return jstDateTime.ToString(format);
        }
    }
}