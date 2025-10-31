namespace ClassScheduleApp.Models
{
    // Keep exactly what the app references today:
    // - PlanToTake is used by defaults & ViewModels
    // - NormalTest is the new type; Normal is an alias for older code paths
    public enum CourseStatus
    {
        InProgress = 0,
        Completed = 1,
        Dropped = 2,
        PlanToTake = 3
    }

    public enum AssessmentType
    {
        Objective = 0,
        Performance = 1,
        NormalTest = 2,

        // Back-compat alias so any "AssessmentType.Normal" compiles:
        Normal = NormalTest
    }

    // Optional (kept in case you use it later)
    public enum NotificationType
    {
        None = 0,
        Start = 1,
        End = 2,
        Due = 3
    }
}
