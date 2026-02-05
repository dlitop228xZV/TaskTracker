using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public int AssigneeId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public List<int> TagIds { get; set; } = new();
    }
}