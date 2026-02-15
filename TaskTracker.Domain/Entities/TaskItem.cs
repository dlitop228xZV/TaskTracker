using System.ComponentModel.DataAnnotations.Schema;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int AssigneeId { get; set; }

        public User? Assignee { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public List<TaskTag> TaskTags { get; set; } = new();

        /// <summary>
        /// Вычисляемое свойство: просрочена ли задача.
        /// Не хранится в БД (computed property).
        /// Правило: DueDate < DateTime.Now && Status != Done
        /// </summary>
        [NotMapped]
        public bool IsOverdue => DueDate < DateTime.Now && Status != TaskItemStatus.Done;
    }
}
