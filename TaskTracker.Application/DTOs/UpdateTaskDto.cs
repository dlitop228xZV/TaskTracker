using System.ComponentModel.DataAnnotations;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class UpdateTaskDto
    {
        [MinLength(3)]
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "AssigneeId должен быть положительным числом")]
        public int? AssigneeId { get; set; }

        public DateTime? DueDate { get; set; }

        [RegularExpression("New|InProgress|Done", ErrorMessage = "Статус должен быть New, InProgress или Done")]
        public string? Status { get; set; }

        [Range(1, 3, ErrorMessage = "Приоритет должен быть от 1 (Low) до 3 (High)")]
        public int? Priority { get; set; }

        public List<int>? TagIds { get; set; }
    }
}