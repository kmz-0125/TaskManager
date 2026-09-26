namespace TaskManager.Models
{
    public class Holiday
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string HolidayName { get; set; } = string.Empty;
    }
}