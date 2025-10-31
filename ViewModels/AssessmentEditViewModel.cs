using System.Linq;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.ViewModels
{
    public class AssessmentEditViewModel : BaseViewModel
    {
        private readonly AppDatabase _db;
        private readonly INotificationService _notify;

        public Assessment? Assessment { get; private set; }

        public AssessmentEditViewModel(AppDatabase db, INotificationService notify)
        {
            _db = db;
            _notify = notify;
        }

        /// <summary>
        /// Load an existing assessment by id, or prepare a new one for the given course.
        /// Pass typeHint when creating a new assessment (Objective/Performance).
        /// </summary>
        public async Task LoadAsync(int courseId, int assessmentId, AssessmentType? typeHint = null)
        {
            if (assessmentId > 0)
            {
                var list = await _db.GetAssessmentsByCourseAsync(courseId);
                Assessment = list.FirstOrDefault(a => a.Id == assessmentId);
                if (Assessment is null)
                {
                    await Shell.Current.DisplayAlert("Error", "Assessment not found.", "OK");
                }
                return;
            }

            // New assessment defaults
            Assessment = new Assessment
            {
                CourseId = courseId,
                Title = string.Empty,
                Type = typeHint ?? AssessmentType.Objective,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(14),
                NotifyStart = false,
                NotifyEnd = false
            };
        }

        public async Task SaveAsync()
        {
            if (Assessment is null)
            {
                await Shell.Current.DisplayAlert("Error", "No assessment loaded.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Assessment.Title))
            {
                await Shell.Current.DisplayAlert("Validation", "Please enter an assessment title.", "OK");
                return;
            }

            await _db.SaveAssessmentAsync(Assessment);

            // Schedule notifications if toggled
            if (Assessment.NotifyStart)
                await _notify.ScheduleAsync(Assessment.Id * 10 + 1, "Assessment starts", Assessment.Title, Assessment.StartDate);

            if (Assessment.NotifyEnd)
                await _notify.ScheduleAsync(Assessment.Id * 10 + 2, "Assessment ends", Assessment.Title, Assessment.EndDate);
        }

        public async Task DeleteAsync()
        {
            if (Assessment is null)
            {
                await Shell.Current.DisplayAlert("Error", "No assessment loaded.", "OK");
                return;
            }

            if (Assessment.Id == 0)
            {
                // Not persisted yet; nothing to delete
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Delete", "Delete this assessment?", "Yes", "No");
            if (!confirm) return;

            await _db.DeleteAssessmentAsync(Assessment);
        }
    }
}
