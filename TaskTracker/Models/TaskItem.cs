using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models  // ← пространство имён должно быть TaskTracker.Models
{
    public class TaskItem  // ← переименовали из Task в TaskItem
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

        public string Status { get; set; } = "New";

        public int Priority { get; set; } = 2;

        public User Assignee { get; set; }
        public List<TaskTag> TaskTags { get; set; } = new();

        public bool IsOverdue =>
            Status != "Done" && DueDate.Date < DateTime.UtcNow.Date;
    }
}