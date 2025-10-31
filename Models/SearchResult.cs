using System.Collections.Generic;

namespace ClassScheduleApp.Models
{
    public sealed class SearchResult
    {
        public List<Term> Terms { get; init; } = new();
        public List<Course> Courses { get; init; } = new();
        public List<Assessment> Assessments { get; init; } = new();
        public List<TodoItem> Todos { get; init; } = new();
    }
}
