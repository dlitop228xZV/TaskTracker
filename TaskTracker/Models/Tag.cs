using TaskTracker.Models;

namespace TaskTracker.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<TaskTag> TaskTags { get; set; } = new();
    }
}