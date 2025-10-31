// File: Views/CoursesPage.xaml.cs
using ClassScheduleApp.Helpers;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.Views;

[QueryProperty(nameof(TermId), "termId")]
public partial class CoursesPage : ContentPage
{
    private readonly AppDatabase _db;
    private Term? _term;
    private List<Course> _courses = new();

    public int TermId { get; set; }

    // Shell/XAML ctor -> resolve AppDatabase from DI
    public CoursesPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }

    // DI-friendly ctor
    public CoursesPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var terms = await _db.GetTermsAsync();
            _term = terms.FirstOrDefault(t => t.Id == TermId);

            if (_term == null)
            {
                await DisplayAlert("Error", "Term not found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Header (make sure your XAML has a Label x:Name="TermHeader")
            TermHeader.Text = $"{_term.Title}  ({_term.StartDate:MMM d, yyyy} - {_term.EndDate:MMM d, yyyy})";

            // Load courses (ensure your XAML has CollectionView x:Name="CoursesCollection"
            // and a Button x:Name="AddCourseBtn")
            _courses = await _db.GetCoursesByTermAsync(_term.Id);
            CoursesCollection.ItemsSource = _courses;

            // Optional: enforce 6-course limit
            AddCourseBtn.IsEnabled = _courses.Count < 6;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load courses.\n{ex.Message}", "OK");
        }
    }

    // Add Course
    private async void AddCourse_Clicked(object? sender, EventArgs e)
    {
        try
        {
            if (_term == null)
            {
                await DisplayAlert("Error", "Term not found.", "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object>
            {
                ["term"] = _term
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    // Edit Course
    private async void EditCourse_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Course course) return;

        try
        {
            await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object>
            {
                ["course"] = course,
                ["term"] = _term
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }

    // Delete Course
    private async void DeleteCourse_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Course course) return;

        bool confirm = await DisplayAlert("Delete Course",
            $"Are you sure you want to delete '{course.Title}'?", "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteCourseAsync(course);
            await DisplayAlert("Deleted", $"Course '{course.Title}' removed.", "OK");

            // Reload
            if (_term != null)
            {
                _courses = await _db.GetCoursesByTermAsync(_term.Id);
                CoursesCollection.ItemsSource = _courses;
                AddCourseBtn.IsEnabled = _courses.Count < 6;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to delete course.\n{ex.Message}", "OK");
        }
    }

    // Clear selection highlight (wire up in XAML: SelectionChanged="OnCourseSelected")
    private void OnCourseSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView cv)
            cv.SelectedItem = null;
    }
}
