// File: Services/SearchResult.cs
using System.Collections.Generic;
using ClassScheduleApp.Models;

namespace ClassScheduleApp.Services
{
    public sealed class SearchResult
    {
        public List<Term> Terms { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<Assessment> Assessments { get; set; } = new();
        public List<TodoItem> Todos { get; set; } = new();

        // Convenience flag (optional to use)
        public bool IsEmpty =>
            (Terms?.Count ?? 0) == 0 &&
            (Courses?.Count ?? 0) == 0 &&
            (Assessments?.Count ?? 0) == 0 &&
            (Todos?.Count ?? 0) == 0;
    }
}
