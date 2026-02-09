using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssigneeId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "AssigneeId must be valid")]
        public int AssigneeId { get; set; }

        [Required(ErrorMessage = "DueDate is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public TaskStatus Status { get; set; }

        public List<int> TagIds { get; set; } = new List<int>();
    }
}