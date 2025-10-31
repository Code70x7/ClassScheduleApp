using ClassScheduleApp.Models;
using ClassScheduleApp.Services;
using ClassScheduleApp.Helpers;

namespace ClassScheduleApp.Views;

public partial class TermsPage : ContentPage
{
    private readonly AppDatabase _db;
    private List<Term> _terms = new();

    public TermsPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    public TermsPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTermsAsync();
    }

    private async Task LoadTermsAsync()
    {
        _terms = await _db.GetTermsAsync();
        TermsCollection.ItemsSource = _terms;
    }

    // ===== Buttons =====
    private async void AddTerm_Clicked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(TermDetailPage),
                new Dictionary<string, object> { ["termId"] = 0 });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation error", ex.Message, "OK");
        }
    }

    private async void EditTerm_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not Term t) return;

        try
        {
            await Shell.Current.GoToAsync(nameof(TermDetailPage),
                new Dictionary<string, object> { ["termId"] = t.Id });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation error", ex.Message, "OK");
        }
    }

    private async void OpenCourses_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is Term term)
        {
            await Shell.Current.GoToAsync($"{nameof(CoursesPage)}?termId={term.Id}");
        }
    }


    private async void DeleteTerm_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not Term t) return;

        var confirm = await DisplayAlert("Delete Term",
            $"Delete '{t.Title}'?", "Delete", "Cancel");
        if (!confirm) return;

        await _db.DeleteTermAsync(t.Id);
        await LoadTermsAsync();
    }

    private void ClearSelection(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView cv) cv.SelectedItem = null;
    }
}
