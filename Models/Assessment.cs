using SQLite;

namespace ClassScheduleApp.Models
{
    [Table("Assessments")]
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int CourseId { get; set; }

        public AssessmentType Type { get; set; }

        [NotNull]
        public string Title { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DueDate { get; set; }

        public bool NotifyStart { get; set; }
        public bool NotifyEnd { get; set; }

        // 🆕 Optional notes for user comments or reminders
        public string? Notes { get; set; }
    }
}
