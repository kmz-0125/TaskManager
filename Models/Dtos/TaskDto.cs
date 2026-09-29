using System.Text.Json.Serialization;

namespace TaskManager.Models.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // この属性を付けると、"NotStarted"、"High"のような文字列で返せる（Enumはそのままだと数値になるため）
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.TaskStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}