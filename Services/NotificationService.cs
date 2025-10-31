namespace ClassScheduleApp.Services
{
    using Plugin.LocalNotification;
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Schedules a local notification at a specific time.
        /// </summary>
        public async Task ScheduleAsync(int id, string title, string message, DateTime when)
        {
            try
            {
                var request = new NotificationRequest
                {
                    NotificationId = id,
                    Title = title,
                    Description = message,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = when,
                        NotifyRepeatInterval = null
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
                System.Diagnostics.Debug.WriteLine($"[Notification] Scheduled '{title}' for {when}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notification] Error scheduling: {ex}");
            }
        }

        /// <summary>
        /// Cancels a scheduled or already-shown notification by its ID.
        /// </summary>
        public void Cancel(int id)
        {
            try
            {
                LocalNotificationCenter.Current.Cancel(id);
                System.Diagnostics.Debug.WriteLine($"[Notification] Cancelled notification {id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notification] Error cancelling: {ex}");
            }
        }

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        public void CancelAll()
        {
            try
            {
                LocalNotificationCenter.Current.CancelAll();
                System.Diagnostics.Debug.WriteLine("[Notification] Cancelled all notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notification] Error cancelling all: {ex}");
            }
        }
    }
}
