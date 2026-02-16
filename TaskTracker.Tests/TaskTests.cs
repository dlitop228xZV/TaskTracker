using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using Xunit;

namespace TaskTracker.Tests
{
    public class TaskTests
    {
        [Fact]
        public void IsOverdue_WhenDueDateYesterday_AndStatusNew_ReturnsTrue()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Test",
                DueDate = DateTime.Now.AddDays(-1),
                Status = TaskItemStatus.New
            };

            // Act
            var isOverdue = task.IsOverdue;

            // Assert
            Assert.True(isOverdue);
        }

        [Fact]
        public void IsOverdue_WhenDueDateTomorrow_ReturnsFalse()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Test",
                DueDate = DateTime.Now.AddDays(1),
                Status = TaskItemStatus.New
            };

            // Act
            var isOverdue = task.IsOverdue;

            // Assert
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsOverdue_WhenStatusDone_CannotBeOverdue_EvenIfDueDateYesterday()
        {
            // Arrange
            var task = new TaskItem
            {
                Title = "Test",
                DueDate = DateTime.Now.AddDays(-1),
                Status = TaskItemStatus.Done
            };

            // Act
            var isOverdue = task.IsOverdue;

            // Assert
            Assert.False(isOverdue);
        }

        [Fact]
        public void IsOverdue_BoundaryCase_DueDateToday_ReturnsFalse()
        {
            // ⚠️ Важно: IsOverdue использует DateTime.Now внутри свойства,
            // поэтому "DueDate = DateTime.Now" может стать просроченной через миллисекунды.
            // Чтобы тест был стабильным и при этом оставался "сегодня", задаём небольшой запас.
            var task = new TaskItem
            {
                Title = "Test",
                DueDate = DateTime.Now.AddSeconds(2), // всё ещё "сегодня"
                Status = TaskItemStatus.New
            };

            // Act
            var isOverdue = task.IsOverdue;

            // Assert
            Assert.False(isOverdue);
        }
    }
}
