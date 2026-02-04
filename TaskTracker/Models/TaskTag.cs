namespace TaskTracker.Models
{
    public class TaskTag
    {
        public int TaskId { get; set; }
        public int TagId { get; set; }

        public TaskItem Task { get; set; }  // ← TaskItem вместо Task
        public Tag Tag { get; set; }
    }
}