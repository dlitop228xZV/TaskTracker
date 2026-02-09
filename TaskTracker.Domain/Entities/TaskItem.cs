using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssigneeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        // Навигационные свойства
        public virtual User? Assignee { get; set; }
        public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();

        // Конструктор
        public TaskItem()
        {
            CreatedAt = DateTime.Now;
            Status = (TaskStatus)TaskItemStatus.New;
        }

        // Метод Update
        public void Update(
            string title,
            string description,
            int assigneeId,
            DateTime dueDate,
            TaskPriority priority,
            TaskStatus status)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required");

            if (title.Length < 3 || title.Length > 200)
                throw new ArgumentException("Title must be between 3 and 200 characters");

            // Обновление полей
            Title = title;
            Description = description;
            AssigneeId = assigneeId;
            DueDate = dueDate;
            Priority = priority;

            // Автоматическая установка CompletedAt при переводе в Done
            if (status == (TaskStatus)TaskItemStatus.Done && Status != (TaskStatus)TaskItemStatus.Done)
            {
                CompletedAt = DateTime.Now;
            }
            else if (status != (TaskStatus)TaskItemStatus.Done && CompletedAt.HasValue)
            {
                CompletedAt = null; // Сбрасываем, если убрали статус Done
            }

            Status = status;
        }
    }
}