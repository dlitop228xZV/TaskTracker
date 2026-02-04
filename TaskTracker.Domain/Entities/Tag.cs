namespace TaskTracker.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<TaskTag> TaskTags { get; set; } = new();
    }
}