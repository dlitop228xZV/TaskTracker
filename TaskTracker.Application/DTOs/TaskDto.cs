using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}