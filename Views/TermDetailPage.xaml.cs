using ClassScheduleApp.Models;
using ClassScheduleApp.Services;
using ClassScheduleApp.Helpers;

namespace ClassScheduleApp.Views;

[QueryProperty(nameof(TermId), "termId")]
public partial class TermDetailPage : ContentPage
{
    private readonly AppDatabase _db;
    private Term _term = new();
    private bool _loaded;

    public int TermId { get; set; }

    public TermDetailPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    public TermDetailPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (TermId > 0)
            _term = await _db.GetTermByIdAsync(TermId) ?? new Term();

        TitleEntry.Text = _term.Title;
        StartDatePicker.Date = _term.StartDate == default ? DateTime.Today : _term.StartDate;
        EndDatePicker.Date = _term.EndDate == default ? StartDatePicker.Date.AddMonths(3) : _term.EndDate;

        await LoadCoursesAsync();
    }


    // ---------- Helpers ----------
    private async Task LoadCoursesAsync()
    {
        if (_term.Id == 0)
        {
            CoursesCollection.ItemsSource = Array.Empty<Course>();
            return;
        }

        var courses = await _db.GetCoursesByTermAsync(_term.Id);
        CoursesCollection.ItemsSource = courses;
    }

    private async Task<bool> EnsureSavedAsync()
    {
        // Save the term if it's new (so courses can reference it)
        if (_term.Id != 0) return true;

        var title = (TitleEntry.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Term", "Enter a term name before adding courses.", "OK");
            return false;
        }
        if (EndDatePicker.Date < StartDatePicker.Date)
        {
            await DisplayAlert("Term", "End date must be after start date.", "OK");
            return false;
        }

        _term.Title = title;
        _term.StartDate = StartDatePicker.Date;
        _term.EndDate = EndDatePicker.Date;
        await _db.SaveTermAsync(_term); // inserts and assigns Id
        await LoadCoursesAsync();
        return true;
    }

    // ---------- Buttons / UI Events ----------
    private async void Save_Clicked(object? sender, EventArgs e)
    {
        var title = (TitleEntry.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Term", "Please enter a term name.", "OK");
            return;
        }
        if (EndDatePicker.Date < StartDatePicker.Date)
        {
            await DisplayAlert("Term", "End date must be after start date.", "OK");
            return;
        }

        _term.Title = title;
        _term.StartDate = StartDatePicker.Date;
        _term.EndDate = EndDatePicker.Date;

        await _db.SaveTermAsync(_term);
        await DisplayAlert("Saved", "Term saved successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void Delete_Clicked(object? sender, EventArgs e)
    {
        if (_term.Id == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        // Prevent accidental deletes when there are courses
        var courses = await _db.GetCoursesByTermAsync(_term.Id);
        var hasCourses = courses?.Count > 0;

        string msg = hasCourses
            ? "This term has courses. Deleting it will remove the term only (courses remain orphaned). It's recommended to delete courses first.\n\nDelete term anyway?"
            : "Delete this term?";

        var confirm = await DisplayAlert("Delete Term", msg, "Delete", "Cancel");
        if (!confirm) return;

        await _db.DeleteTermAsync(_term);
        await Shell.Current.GoToAsync("..");
    }

    private async void AddCourse_Clicked(object? sender, EventArgs e)
    {
        if (!await EnsureSavedAsync()) return;

        // Navigate to Course editor with this term
        await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object>
        {
            ["term"] = _term
        });
    }

    private void Collection_ClearSelection(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView cv) cv.SelectedItem = null;
    }

    private async void EditCourse_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Course course) return;

        await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object>
        {
            ["course"] = course,
            ["term"] = _term
        });
    }

    private async void DeleteCourse_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Course course) return;

        var ok = await DisplayAlert("Delete Course",
            $"Delete '{course.Title}' from this term?", "Delete", "Cancel");
        if (!ok) return;

        await _db.DeleteCourseAsync(course);
        await LoadCoursesAsync();
    }
}
