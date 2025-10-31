// File: MauiProgram.cs
using System.IO;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;          // for FileSystem
using SQLite;
using SQLitePCL;

using ClassScheduleApp.Services;       // AppDatabase, IAuthService, etc.
using ClassScheduleApp.Views;          // Pages for DI

namespace ClassScheduleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- SQLite native provider (iOS/Android need this) ---
            Batteries_V2.Init();

            // --- SQLite connection (singleton) ---
            builder.Services.AddSingleton<SQLiteAsyncConnection>(_ =>
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "classschedule.db3");
                var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
                return new SQLiteAsyncConnection(dbPath, flags);
            });

            // --- App services (DI) ---
            builder.Services.AddSingleton<AppDatabase>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            // If you don’t have a concrete NotificationService yet, temporarily use a no-op:
            // builder.Services.AddSingleton<INotificationService, NoOpNotificationService>();

            // --- Pages (so Navigation/ServiceHelper can resolve them) ---
            builder.Services.AddTransient<TermsPage>();
            builder.Services.AddTransient<CoursesPage>();
            builder.Services.AddTransient<CourseDetailPage>();
            builder.Services.AddTransient<TermDetailPage>();
            builder.Services.AddTransient<AssessmentEditPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<SearchPage>();
            builder.Services.AddTransient<ReportsPage>();

            var app = builder.Build();

            // Allow ServiceHelper.GetRequiredService<T>()
            Helpers.ServiceHelper.Initialize(app.Services);

            return app;
        }
    }
}
