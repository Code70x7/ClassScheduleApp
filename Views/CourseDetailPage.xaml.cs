// File: Views/CourseDetailPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassScheduleApp.Helpers;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.Views;

[QueryProperty(nameof(NavCourse), "course")]
[QueryProperty(nameof(NavTerm), "term")]
public partial class CourseDetailPage : ContentPage
{
    private readonly AppDatabase _db;
    private Course? _course;
    private Term? _term;
    private bool _initialized;

    public Course? NavCourse { get; set; }
    public Term? NavTerm { get; set; }

    // Shell/XAML default ctor -> forwards to DI
    public CourseDetailPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    private static bool LooksLikeEmail(string s)
    => !string.IsNullOrWhiteSpace(s) && s.Contains('@') && s.Contains('.');

    // DI ctor
    public CourseDetailPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized) return;

        _course = NavCourse;
        _term = NavTerm;

        if (_course is null)
        {
            _course = new Course
            {
                TermId = _term?.Id ?? 0,
                Title = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                DueDate = DateTime.Today.AddDays(7),
                Status = CourseStatus.PlanToTake
            };
        }

        PushToUI(_course);

        if (_course.Id > 0)
        {
            await LoadAssessmentsAsync();
            await LoadTodosAsync();
        }

        _initialized = true;
    }

    // ---------------- Helpers ----------------

    private async Task EnsureCourseHasIdAsync()
    {
        if (_course == null) return;

        if (_course.Id == 0)
        {
            if (!TryPullFromUI(out var msg))
            {
                await DisplayAlert("Course", msg, "OK");
                return;
            }
            await _db.SaveCourseAsync(_course);
        }
    }

    private async Task LoadAssessmentsAsync()
    {
        if (_course?.Id > 0)
            AssessmentsCollection.ItemsSource = await _db.GetAssessmentsByCourseAsync(_course.Id);
        else
            AssessmentsCollection.ItemsSource = Array.Empty<Assessment>();
    }

    private async Task LoadTodosAsync()
    {
        if (_course?.Id > 0)
            TodosCollection.ItemsSource = await _db.GetTodosByCourseAsync(_course.Id);
        else
            TodosCollection.ItemsSource = Array.Empty<TodoItem>();
    }

    // Persist all current assessment notes (used when saving course)
    private async Task PersistAllAssessmentNotesAsync()
    {
        if (_course?.Id > 0 && AssessmentsCollection?.ItemsSource is IEnumerable<Assessment> list)
        {
            foreach (var a in list)
            {
                await _db.SaveAssessmentAsync(a);
            }
        }
    }

    // ---------------- UI <-> Model ----------------

    private void PushToUI(Course c)
    {
        TitleEntry.Text = c.Title;

        var start = c.StartDate == default ? DateTime.Today : c.StartDate;
        var end = c.EndDate == default ? start.AddMonths(1) : c.EndDate;
        var due = c.DueDate ?? DateTime.Today.AddDays(7);

        StartDatePicker.Date = start;
        EndDatePicker.Date = end;
        DueDatePicker.Date = due;

        StatusPicker.SelectedItem = c.Status.ToString();

        InstructorNameEntry.Text = c.InstructorName;
        InstructorPhoneEntry.Text = c.InstructorPhone;
        InstructorEmailEntry.Text = c.InstructorEmail;
        NotesEditor.Text = c.Notes;
    }

    private bool TryPullFromUI(out string error)
    {
        error = string.Empty;
        if (_course is null)
        {
            error = "No course in context.";
            return false;
        }

        var title = (TitleEntry.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            error = "Please enter a course title.";
            return false;
        }

        DateTime start = StartDatePicker.Date;
        DateTime end = EndDatePicker.Date;
        DateTime due = DueDatePicker.Date;

        if (end < start)
        {
            error = "End Date must be on/after Start Date.";
            return false;
        }

        // NEW: required instructor fields
        var instrName = (InstructorNameEntry.Text ?? "").Trim();
        var instrPhone = (InstructorPhoneEntry.Text ?? "").Trim();
        var instrEmail = (InstructorEmailEntry.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(instrName))
        {
            error = "Instructor name is required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(instrPhone))
        {
            error = "Instructor phone is required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(instrEmail) || !LooksLikeEmail(instrEmail))
        {
            error = "A valid instructor email is required.";
            return false;
        }

        var statusText = (StatusPicker.SelectedItem as string) ?? "PlanToTake";
        if (!Enum.TryParse(statusText, ignoreCase: true, out CourseStatus status))
            status = CourseStatus.PlanToTake;

        _course.Title = title;
        _course.StartDate = start;
        _course.EndDate = end;
        _course.DueDate = due;
        _course.Status = status;
        _course.InstructorName = instrName;
        _course.InstructorPhone = instrPhone;
        _course.InstructorEmail = instrEmail;
        _course.Notes = NotesEditor.Text;

        if (_course.TermId == 0 && _term != null)
            _course.TermId = _term.Id;

        return true;
    }

    // ---------------- Buttons/handlers ----------------

    private async void SaveCourse_Clicked(object? sender, EventArgs e)
    {
        if (!TryPullFromUI(out var msg))
        {
            await DisplayAlert("Validation", msg, "OK");
            return;
        }

        await _db.SaveCourseAsync(_course!);

        // Persist any edited assessment notes as well
        await PersistAllAssessmentNotesAsync();

        if (_course!.Id > 0)
        {
            await LoadAssessmentsAsync();
            await LoadTodosAsync();
        }

        await Shell.Current.GoToAsync("..");
    }

    // Create assessments immediately and refresh list
    private async void AddOA_Clicked(object? sender, EventArgs e)
        => await AddAssessmentAsync(AssessmentType.Objective, "Objective Assessment");

    private async void AddPA_Clicked(object? sender, EventArgs e)
        => await AddAssessmentAsync(AssessmentType.Performance, "Performance Assessment");

    private async void AddTest_Clicked(object? sender, EventArgs e)
        => await AddAssessmentAsync(AssessmentType.NormalTest, "Normal Test");

    private async Task AddAssessmentAsync(AssessmentType type, string defaultTitle)
    {
        if (_course is null)
        {
            await DisplayAlert("Course", "Please save the course first.", "OK");
            return;
        }

        await EnsureCourseHasIdAsync();
        if (_course.Id == 0) return; // if save failed/aborted

        var a = new Assessment
        {
            CourseId = _course.Id,
            Title = defaultTitle,
            Type = type,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(7),
            NotifyStart = false,
            NotifyEnd = false,
            // Notes optional, user can type inline
        };

        await _db.SaveAssessmentAsync(a);
        await LoadAssessmentsAsync();
    }

    // Edit existing assessment (opens your dedicated page)
    private async void EditAssessment_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is Assessment a && _course is not null)
        {
            await Shell.Current.GoToAsync(nameof(AssessmentEditPage),
                new Dictionary<string, object> { ["assessment"] = a, ["course"] = _course });
        }
    }

    // Delete assessment from the list
    private async void DeleteAssessment_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not Assessment a) return;

        var ok = await DisplayAlert("Delete Assessment", $"Delete '{a.Title}'?", "Delete", "Cancel");
        if (!ok) return;

        await _db.DeleteAssessmentAsync(a);
        await LoadAssessmentsAsync();
    }

    // Inline notes autosave when Editor loses focus
    private async void AssessmentNotes_Unfocused(object? sender, FocusEventArgs e)
    {
        try
        {
            if (sender is Editor ed && ed.BindingContext is Assessment a)
            {
                await _db.SaveAssessmentAsync(a);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("AssessmentNotes_Unfocused save failed: " + ex);
        }
    }

    // --------- To-Do handlers ---------

    private async void AddTodo_Clicked(object? sender, EventArgs e)
    {
        if (_course is null) return;

        var text = NewTodoEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        await EnsureCourseHasIdAsync();
        if (_course.Id == 0) return;

        var todo = new TodoItem
        {
            CourseId = _course.Id,
            Title = text,
            DueDate = TodoDueDate.Date,
            IsCompleted = false
        };

        await _db.UpsertTodoAsync(todo);
        NewTodoEntry.Text = string.Empty;

        await LoadTodosAsync();
    }

    private async void DeleteTodo_Clicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not TodoItem t) return;
        await _db.DeleteTodoAsync(t);
        await LoadTodosAsync();
    }
}
