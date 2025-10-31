namespace ClassScheduleApp.Services
{
    public interface INotificationService
    {
        Task ScheduleAsync(int id, string title, string message, DateTime when);
        void Cancel(int id);
        void CancelAll();
    }
}
