// File: Views/ReportsPage.xaml.cs
using System.Collections.ObjectModel;
using System.Linq;
using ClassScheduleApp.Helpers;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.Views;

public partial class ReportsPage : ContentPage
{
    private readonly AppDatabase _db;

    // Row view-models for table-style reports
    private record TermReportRow(string Term, DateTime Start, DateTime End, int CourseCount);
    private record CourseReportRow(string Course, string Term, string Status, DateTime Start, DateTime End);
    private record TodoReportRow(string Task, string Course, DateTime? Due, bool Completed);

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
        // Pull once
        var terms = await _db.GetTermsAsync();
        var courses = await _db.GetAllCoursesAsync();
        var assessments = await _db.GetAllAssessmentsAsync();
        var todos = await _db.GetAllTodosAsync();

        // ===== Summary cards (existing) =====
        TermsCountLabel.Text = terms.Count.ToString();
        CoursesCountLabel.Text = courses.Count.ToString();
        OpenTodosLabel.Text = todos.Count(t => !t.IsCompleted).ToString();

        OaCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.Objective).ToString();
        PaCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.Performance).ToString();
        TestCountLabel.Text = assessments.Count(a => a.Type == AssessmentType.NormalTest).ToString();

        RecentAssessments.ItemsSource = assessments
            .OrderByDescending(a => a.EndDate)
            .Take(10)
            .ToList();

        // ====== Table Reports with titles + timestamps ======

        var nowStamp = DateTime.Now.ToString("MMM d, yyyy h:mm tt");

        TermsReportGeneratedAt.Text = $"Generated: {nowStamp}";
        CoursesReportGeneratedAt.Text = $"Generated: {nowStamp}";
        TodosReportGeneratedAt.Text = $"Generated: {nowStamp}";

        // Terms report rows
        var courseGroups = courses.GroupBy(c => c.TermId)
                                  .ToDictionary(g => g.Key, g => g.Count());
        var termRows = terms
            .OrderBy(t => t.StartDate)
            .Select(t => new TermReportRow(
                Term: t.Title,
                Start: t.StartDate,
                End: t.EndDate,
                CourseCount: courseGroups.TryGetValue(t.Id, out var cnt) ? cnt : 0))
            .ToList();
        TermsReportList.ItemsSource = new ObservableCollection<TermReportRow>(termRows);

        // Courses report rows
        var termLookup = terms.ToDictionary(t => t.Id, t => t.Title);
        var courseRows = courses
            .OrderBy(c => c.StartDate)
            .Select(c => new CourseReportRow(
                Course: c.Title,
                Term: termLookup.TryGetValue(c.TermId, out var tTitle) ? tTitle : "—",
                Status: c.Status.ToString(),
                Start: c.StartDate,
                End: c.EndDate))
            .ToList();
        CoursesReportList.ItemsSource = new ObservableCollection<CourseReportRow>(courseRows);

        // To-Dos report rows (multi-column)
        var courseTitleLookup = courses.ToDictionary(c => c.Id, c => c.Title);
        var todoRows = todos
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .Select(t => new TodoReportRow(
                Task: t.Title,
                Course: t.CourseId != 0 && courseTitleLookup.TryGetValue(t.CourseId, out var cTitle) ? cTitle : "—",
                Due: t.DueDate,
                Completed: t.IsCompleted))
            .ToList();
        TodosReportList.ItemsSource = new ObservableCollection<TodoReportRow>(todoRows);
    }
}
