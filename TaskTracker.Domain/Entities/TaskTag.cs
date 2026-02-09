namespace TaskTracker.Domain.Entities
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        public int TagId { get; set; }

        public virtual TaskItem Task { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}