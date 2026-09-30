using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskManager.Models.Dtos
{
    public class CreateTaskDto
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "タスク名は必須です")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.TaskStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
