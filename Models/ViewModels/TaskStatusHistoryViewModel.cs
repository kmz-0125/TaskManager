namespace TaskManager.Models.ViewModels
{
    public class TaskStatusHistoryViewModel
    {
        public int Id { get; set; }
        public TaskStatus OldStatus { get; set; }
        public TaskStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}