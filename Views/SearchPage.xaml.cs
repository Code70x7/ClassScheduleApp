// File: Views/SearchPage.xaml.cs
using System;
using System.Linq;
using ClassScheduleApp.Models;
using ClassScheduleApp.Services;
using ClassScheduleApp.Helpers;   // ServiceHelper

namespace ClassScheduleApp.Views;

public partial class SearchPage : ContentPage
{
    private readonly AppDatabase _db;
    private bool _isSearching;

    // ✅ Shell/XAML uses this one
    public SearchPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }

    // ✅ DI-friendly ctor
    public SearchPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    // === Search button handler ===
    private async void Search_Clicked(object sender, EventArgs e)
    {
        if (_isSearching) return;
        _isSearching = true;

        var query = QueryEntry?.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert("Search", "Please enter text to search.", "OK");
            _isSearching = false;
            return;
        }

        try
        {
            var results = await _db.SearchAsync(query);

            TermsList.ItemsSource = results.Terms;
            CoursesList.ItemsSource = results.Courses;
            AssessmentsList.ItemsSource = results.Assessments;
            TodosList.ItemsSource = results.Todos;

            EmptyLabel.IsVisible =
                (results.Terms?.Count ?? 0) == 0 &&
                (results.Courses?.Count ?? 0) == 0 &&
                (results.Assessments?.Count ?? 0) == 0 &&
                (results.Todos?.Count ?? 0) == 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex}");
            // Show the root message so we can see e.g. "no such column: Notes" or table name issues
            await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
        }
        finally
        {
            _isSearching = false;
        }
    }

    // === Selection handlers (navigate or show quick detail) ===

    private async void Term_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Term term)
        {
            await Shell.Current.GoToAsync(nameof(TermDetailPage), new Dictionary<string, object>
            {
                ["term"] = term
            });
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void Course_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Course course)
        {
            await Shell.Current.GoToAsync(nameof(CourseDetailPage), new Dictionary<string, object>
            {
                ["course"] = course
            });
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void Assessment_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Assessment a)
        {
            var end = a.EndDate == default ? (DateTime?)null : a.EndDate;
            var endText = end.HasValue ? end.Value.ToString("d") : "—";
            await DisplayAlert("Assessment", $"{a.Title}\nEnds: {endText}", "OK");
        }
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void Todo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is TodoItem t)
        {
            var dueText = t.DueDate.HasValue ? t.DueDate.Value.ToString("d") : "—";
            await DisplayAlert("To-Do", $"{t.Title}\nDue: {dueText}", "OK");
        }
        ((CollectionView)sender).SelectedItem = null;
    }
}
