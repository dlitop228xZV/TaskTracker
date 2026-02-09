using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class UpdateTaskDto
    {
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int? AssigneeId { get; set; }

        public DateTime? DueDate { get; set; }

        public TaskPriority? Priority { get; set; }

        public TaskItemStatus? Status { get; set; }

        public List<int> TagIds { get; set; } = new();
    }
}