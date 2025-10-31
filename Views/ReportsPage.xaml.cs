// File: Views/ReportsPage.xaml.cs
using System.Linq;
using ClassScheduleApp.Helpers;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.Views;

public partial class ReportsPage : ContentPage
{
    private readonly AppDatabase _db;

    public ReportsPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }

    public ReportsPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        // Pull data (small datasets; simple and reliable)
        var terms = await _db.GetTermsAsync();
        var courses = await _db.GetAllCoursesAsync();
        var assessments = await _db.GetAllAssessmentsAsync();
        var todos = await _db.GetAllTodosAsync();

        // Summary
        TermsCountLabel.Text = terms.Count.ToString();
        CoursesCountLabel.Text = courses.Count.ToString();
        OpenTodosLabel.Text = todos.Count(t => !t.IsCompleted).ToString();

        // Assessments by type
        OaCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.Objective).ToString();
        PaCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.Performance).ToString();
        TestCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.NormalTest).ToString();

        // Recent assessments (latest 10)
        RecentAssessments.ItemsSource = assessments
            .OrderByDescending(a => a.EndDate)
            .Take(10)
            .ToList();
    }
}
