using System.Collections.ObjectModel;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.ViewModels
{
    public class TermDetailViewModel : BaseViewModel
    {
        private readonly AppDatabase _db;
        public Term? Term { get; private set; }
        public ObservableCollection<Course> Courses { get; } = new();

        public TermDetailViewModel(AppDatabase db) { _db = db; }

        public async Task LoadAsync(int termId)
        {
            Term = (await _db.GetTermsAsync()).First(t => t.Id == termId);
            Courses.Clear();
            foreach (var c in await _db.GetCoursesByTermAsync(termId)) Courses.Add(c);
        }

        public async Task AddCourseAsync()
        {
            if (Term is null)
            {
                await Shell.Current.DisplayAlert("Error", "No term loaded.", "OK");
                return;
            }
            if (Courses.Count >= 6)
            {
                await Shell.Current.DisplayAlert("Limit", "Each term can only have six courses.", "OK");
                return;
            }
            var course = new Course
            {
                TermId = Term.Id,
                Title = "New Course",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                Status = CourseStatus.PlanToTake,
                InstructorName = "Instructor",
                InstructorPhone = "555-000-0000",
                InstructorEmail = "email@example.com"
            };
            await _db.SaveCourseAsync(course);
            await LoadAsync(Term.Id);
        }
    }
}
