using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int AssigneeId { get; set; }

        public string? AssigneeName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Фактический статус для отображения на клиенте.
        /// Если задача просрочена (IsOverdue == true) — возвращается "Overdue",
        /// иначе возвращается Status.ToString().
        /// Пример: "New", "InProgress", "Done", "Overdue".
        /// </summary>
        public string EffectiveStatus { get; set; } = string.Empty;

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public List<string> Tags { get; set; } = new();

        public static TaskDto FromEntity(TaskItem task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.Name,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                Status = task.Status,
                Priority = task.Priority,
                Tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList(),
                EffectiveStatus = task.IsOverdue ? "Overdue" : task.Status.ToString()
            };
        }
    }
}
