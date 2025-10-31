using ClassScheduleApp.Views;

namespace ClassScheduleApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register ONLY pages that are NOT Shell items
        Routing.RegisterRoute(nameof(TermDetailPage), typeof(TermDetailPage));
        Routing.RegisterRoute(nameof(CourseDetailPage), typeof(CourseDetailPage));
        Routing.RegisterRoute(nameof(AssessmentEditPage), typeof(AssessmentEditPage));
        Routing.RegisterRoute(nameof(CoursesPage), typeof(CoursesPage));
    }
}
