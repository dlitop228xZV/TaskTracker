namespace TaskTracker.Application.DTOs
{
    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public int? Priority { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}