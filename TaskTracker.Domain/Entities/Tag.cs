namespace TaskTracker.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}