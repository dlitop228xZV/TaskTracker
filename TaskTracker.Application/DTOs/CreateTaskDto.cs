using System.ComponentModel.DataAnnotations;

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

        [Range(1, 3)]
        public int Priority { get; set; } = 2;

        public List<int> TagIds { get; set; } = new();
    }
}