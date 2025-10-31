using ClassScheduleApp.Helpers;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;

namespace ClassScheduleApp.Views;

[QueryProperty(nameof(NavAssessment), "assessment")]
[QueryProperty(nameof(NavCourse), "course")]
public partial class AssessmentEditPage : ContentPage
{
    private readonly AppDatabase _db;
    private Assessment _a = new();
    private Course? _course;

    public Assessment? NavAssessment { get; set; }
    public Course? NavCourse { get; set; }

    public AssessmentEditPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    public AssessmentEditPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (NavAssessment != null) _a = NavAssessment;
        if (NavCourse != null) _course = NavCourse;

        // Title & dates
        TitleEntry.Text = _a.Title ?? string.Empty;
        StartPicker.Date = _a.StartDate == default ? DateTime.Today : _a.StartDate;
        EndPicker.Date = _a.EndDate == default ? StartPicker.Date.AddDays(7) : _a.EndDate;

        // Type mapping (enum -> picker)
        var typeName = _a.Type.ToString(); // enum, not nullable
        TypePicker.SelectedIndex = typeName switch
        {
            "Objective" => 0,
            "Performance" => 1,
            "NormalTest" => 2,
            _ => 0
        };
    }

    private async void Save_Clicked(object? sender, EventArgs e)
    {
        var title = (TitleEntry.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Assessment", "Please enter a title.", "OK");
            return;
        }
        if (EndPicker.Date < StartPicker.Date)
        {
            await DisplayAlert("Assessment", "End date must be after start date.", "OK");
            return;
        }

        _a.Title = title;
        _a.StartDate = StartPicker.Date;
        _a.EndDate = EndPicker.Date;

        // picker -> enum
        _a.Type = (TypePicker.SelectedIndex) switch
        {
            0 => AssessmentType.Objective,
            1 => AssessmentType.Performance,
            2 => AssessmentType.NormalTest,
            _ => AssessmentType.Objective
        };

        // ensure CourseId if it's a new item
        if (_a.Id == 0 && _course is not null)
            _a.CourseId = _course.Id;

        await _db.SaveAssessmentAsync(_a);
        await Shell.Current.GoToAsync("..");
    }
}
