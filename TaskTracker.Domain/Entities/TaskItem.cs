using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int AssigneeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.New;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public User Assignee { get; set; }
        public List<TaskTag> TaskTags { get; set; } = new();

        public bool IsOverdue =>
            Status != TaskItemStatus.Done && DueDate.Date < DateTime.UtcNow.Date;
    }
}