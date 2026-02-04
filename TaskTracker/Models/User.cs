namespace TaskTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();  // ← TaskItem вместо Task
    }
}