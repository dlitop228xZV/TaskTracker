namespace TaskTracker.Application.DTOs
{
    public class OverdueByAssigneeDto
    {
        /// <summary>
        /// Имя исполнителя (если нет навигации — будет "User#{AssigneeId}").
        /// </summary>
        public string Assignee { get; set; } = string.Empty;

        /// <summary>
        /// Количество просроченных задач у исполнителя.
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Детализация: список просроченных задач (TaskDto).
        /// </summary>
        public List<TaskDto> Tasks { get; set; } = new();
    }
}
