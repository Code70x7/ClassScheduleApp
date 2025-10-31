using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.ViewModels
{
    public class CourseDetailViewModel : BaseViewModel
    {
        private readonly AppDatabase _db;
        private readonly INotificationService _notify;

        public Course? Course { get; private set; }
        public ObservableCollection<Assessment> Assessments { get; } = new();

        // For binding to the Status Picker in XAML
        public Array StatusOptions => Enum.GetValues(typeof(CourseStatus));

        public CourseDetailViewModel(AppDatabase db, INotificationService notify)
        {
            _db = db;
            _notify = notify;
        }

        /// <summary>
        /// Load an existing course by ID and its assessments.
        /// Falls back to a "not found" alert if the ID isn't present.
        /// </summary>
        public async Task LoadAsync(int courseId)
        {
            Course = null;

            // Search across all terms using your existing DB API
            var terms = await _db.GetTermsAsync();
            foreach (var term in terms)
            {
                var courses = await _db.GetCoursesByTermAsync(term.Id);
                var found = courses.FirstOrDefault(c => c.Id == courseId);
                if (found != null)
                {
                    Course = found;
                    break;
                }
            }

            if (Course is null)
            {
                await Shell.Current.DisplayAlert("Error", "Course not found.", "OK");
                Assessments.Clear();
                return;
            }

            // Load assessments for this course
            Assessments.Clear();
            foreach (var a in await _db.GetAssessmentsByCourseAsync(Course.Id))
                Assessments.Add(a);
        }

        /// <summary>
        /// Prepare a brand-new course for a given term (when creating).
        /// </summary>
        public void NewForTerm(int termId)
        {
            Course = new Course
            {
                TermId = termId,
                Title = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                DueDate = DateTime.Today.AddMonths(1),
                Status = CourseStatus.PlanToTake,
                InstructorName = string.Empty,
                InstructorPhone = string.Empty,
                InstructorEmail = string.Empty,
                Notes = string.Empty,
                NotifyStart = false,
                NotifyEnd = false
            };

            Assessments.Clear();
        }

        /// <summary>
        /// Validate + save the course. Schedules notifications if toggled.
        /// </summary>
        public async Task SaveCourseAsync()
        {
            if (Course is null)
            {
                await Shell.Current.DisplayAlert("Error", "No course loaded.", "OK");
                return;
            }

            // Basic instructor validation per C3
            if (string.IsNullOrWhiteSpace(Course.InstructorName) ||
                string.IsNullOrWhiteSpace(Course.InstructorPhone) ||
                string.IsNullOrWhiteSpace(Course.InstructorEmail) ||
                !Regex.IsMatch(Course.InstructorEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Shell.Current.DisplayAlert("Validation",
                    "Enter a valid instructor name, phone, and email.", "OK");
                return;
            }

            await _db.SaveCourseAsync(Course);

            // Schedule start/end alerts per C4
            if (Course.NotifyStart)
                await _notify.ScheduleAsync(Course.Id * 10 + 1, "Course starts", Course.Title, Course.StartDate);

            if (Course.NotifyEnd)
                await _notify.ScheduleAsync(Course.Id * 10 + 2, "Course ends", Course.Title, Course.EndDate);
        }

        /// <summary>
        /// Delete the current course (if it exists).
        /// </summary>
        public async Task DeleteCourseAsync()
        {
            if (Course is null || Course.Id == 0)
                return;

            bool confirm = await Shell.Current.DisplayAlert("Delete",
                $"Delete course '{Course.Title}'?", "Yes", "No");
            if (!confirm) return;

            await _db.DeleteCourseAsync(Course);
        }

        /// <summary>
        /// Ensure exactly one Objective and one Performance assessment.
        /// If one exists, open it for edit; otherwise create a new one.
        /// </summary>
        public async Task AddOrEditAssessmentAsync(AssessmentType type)
        {
            if (Course is null)
            {
                await Shell.Current.DisplayAlert("Error", "No course loaded.", "OK");
                return;
            }

            var existing = Assessments.FirstOrDefault(a => a.Type == type);
            if (existing != null)
            {
                // Navigate to your Assessment edit page (route must be registered in AppShell)
                await Shell.Current.GoToAsync($"{nameof(Views.AssessmentEditPage)}?assessmentId={existing.Id}&courseId={Course.Id}");
                return;
            }

            var a = new Assessment
            {
                CourseId = Course.Id,
                Type = type,
                Title = type == AssessmentType.Objective ? "Objective Assessment" : "Performance Assessment",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(14),
                NotifyStart = false,
                NotifyEnd = false
            };

            await _db.SaveAssessmentAsync(a);
            Assessments.Add(a);

            // Optionally jump straight into editing the new assessment:
            await Shell.Current.GoToAsync($"{nameof(Views.AssessmentEditPage)}?assessmentId={a.Id}&courseId={Course.Id}");
        }
    }
}
