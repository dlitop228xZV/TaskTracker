namespace TaskTracker.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}