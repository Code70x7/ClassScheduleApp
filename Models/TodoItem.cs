using SQLite;

namespace ClassScheduleApp.Models
{
    /// <summary>
    /// A task tied to a Course.
    /// </summary>
    [Table("TodoItem")] // <-- table name used by SQLite
    public class TodoItem
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [Indexed] public int CourseId { get; set; }

        [NotNull] public string Title { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Optional local notification flag (used by SeedData and pages).
        /// </summary>
        public bool NotifyDue { get; set; } = false;

        // Timestamps (optional but handy)
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedUtc { get; set; }
    }
}
