using SQLite;

namespace ClassScheduleApp.Models
{
    [Table("Terms")]
    public class Term
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [NotNull] public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [SQLite.Ignore] public string DateRangeText { get; set; } = "";

    }
}
